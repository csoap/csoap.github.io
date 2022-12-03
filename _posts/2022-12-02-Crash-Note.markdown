
[toc]

# 背景介绍

D项目反馈，在某一个SVN版本下，调用reload函数两次，会稳定出现crash问题，其他版本和其他项目组都没有出现过此问题

最终我们定位出lua5.1的setupvalue实现不能兼容upvaluejoin的问题，特写下本篇文章，用于日后再次复盘

感谢双帅同学的定位与分析

## TL；DR

lua5.1本来没有upvaluejoin，lua5.2才有，从lua5.2移植的upvaluejoin实现跟lua5.1版本的setupvalue一起使用，会导致Upvalue引用的TValue在Upvalue为黑色的时候被释放掉，在下一次gc的traverseclosure过程被继续读取，read invalid memory address



# 基本概念&实验

本章节的目的是通过一些实验，分析出Lua是如何创建，标记，销毁Closure，Upvalue的。

在了解这些实验之前，推荐一篇非常好的文章，这篇文章详细解释了Closure，Upvalue，Proto这些概念

[构建Lua解释器Part11：Upvalue](https://zhuanlan.zhihu.com/p/358423900)

ps：本文不再去赘述这些基本概念

## 提前准备

在lua源码中输出一些printf，用于观察整个lua代码解析和执行的过程：

### 解析部分

f_parser:

```
static void f_parser (lua_State *L, void *ud) {
  int i;
  Proto *tf;
  Closure *cl;
  struct SParser *p = cast(struct SParser *, ud);
  printf("==yc== f_parser: load name %s \n", p->name);
  int c = luaZ_lookahead(p->z);
  luaC_checkGC(L);
#ifdef LUA_PARSER_ENABLE
  tf = ((c == LUA_SIGNATURE[0]) ? luaU_undump : luaY_parser)(L, p->z,
                                                             &p->buff, p->name);
#else
  tf = luaU_undump(L, p->z, &p->buff, p->name);
#endif

  printf("==yc== f_parser: new proto %p\n", tf);
  cl = luaF_newLclosure(L, tf->nups, hvalue(gt(L)));

  printf("==yc== f_parser: new closure %p\n", cl);
  cl->l.p = tf;

  for (i = 0; i < tf->nups; i++)  /* initialize eventual upvalues */
  {
    cl->l.upvals[i] = luaF_newupval(L);
    printf("==yc== f_parser: new upvalue %p\n", cl->l.upvals[i]);
  }
  setclvalue(L, L->top, cl);
  incr_top(L);
}
```

LoadFunction是一个生成Proto的过程，需要注意的是，Proto是可以嵌套的，定义在Proto内的函数所需的Proto就是嵌套Proto


### 执行部分

因为要研究OP_CLOSURE，所以执行部分的修改如下：


```
      case OP_CLOSURE: {
        Proto *p;
        Closure *ncl;
        int nup, j;
        p = cl->p->p[GETARG_Bx(i)];
        nup = p->nups;
        printf("==yc== OP_CLOSURE line %d\n", p->lineinfo);
        ncl = luaF_newLclosure(L, nup, cl->env);
        ncl->l.p = p;
        for (j=0; j<nup; j++, pc++) {
          if (GET_OPCODE(*pc) == OP_GETUPVAL)
          {
            printf("==yc== OP_CLOSURE:OP_GETUPVAL index %d\n", j);
            ncl->l.upvals[j] = cl->upvals[GETARG_B(*pc)];
          }
          else {
            lua_assert(GET_OPCODE(*pc) == OP_MOVE);
            printf("==yc== OP_CLOSURE:OP_MOVE index %d\n", j);
            ncl->l.upvals[j] = luaF_findupval(L, base + GETARG_B(*pc));
          }
        }
        setclvalue(L, ra, ncl);
        Protect(luaC_checkGC(L));
        continue;
      }
```




## closure和upvalue是如何被创建的


### 实验一

Lua代码代码片段如下所示：

```
print("start in test")
local M = {}

local upa = 1
local upb = 2
function M.functionOne()
    print(upa)
end

function M.functionTwo()
    print(upa)
    print(upb)
end

print("end in test")
return M
```

输出结果如下所示，==用（）表示我的解读部分==：


```
（先执行parse部分，解析了test.lua文件）
==yc== f_parser: load name @.\test.lua
（为这个文件生成了Proto与Closure（这个概念在[1]也有说到，Top-Level Closure）
==yc== open_func proto 0000026BB46E84D0
==yc== open_func proto 0000026BB46F2D40
==yc== open_func proto 0000026BB46E1A70
==yc== f_parser: new proto 0000026BB46E84D0  （可以发现Proto 0000026BB46E84D0 嵌套了两个Proto）
==yc== f_parser: new closure 0000026BB46EFB10

（开始执行文件，可以看到整个文件的入口也是一个toplevel的 closure，并且这个closure 0000026BB46EFB10 与parse的可以对应上）
==yc== execute closure 0000026BB46EFB10
start in test
==yc== OP_CLOSURE create closure 0000026BB46EFBF0 with proto 0000026BB46F2D40
==yc== OP_CLOSURE:OP_MOVE index 0
==yc== findupval : not found for level 3027106144
==yc== findupval : create new upval 0000026BB46EF720
==yc== OP_SETTABLE key functionOne
==yc== OP_CLOSURE create closure 0000026BB46E7E10 with proto 0000026BB46E1A70
==yc== OP_CLOSURE:OP_MOVE index 0
==yc== OP_CLOSURE:OP_MOVE index 1
==yc== findupval : not found for level 3027106160
==yc== findupval : create new upval 0000026BB46EFD40
（functionOne和functionTwo共享了upvalue 0000026BB46EF720，指向了upa这个值）
==yc== OP_SETTABLE key functionTwo
end in test

（在整个文件closure的结束后，会调用luaF_close，其中会把栈上的openupvalue变成closedupvalue）
==yc== close upvalue 0000026BB46EFD40
==yc== close upvalue 0000026BB46EF720
```

### 实验二

如果我们在closure内再定义一层closure，会发生什么？

Lua代码如下：


```
print("start in test")
local M = {}

local upa = 1
function M.functionOne()
    print("execute function one")
    print(upa)
    local functionLocal = 10
    
    local nestedFunction = function()
        print(upa)
        print(functionLocal)
    end


    print("execute function one end")
end

M.functionOne()
print("end in test")
return M
```


执行后结果如下所示：


```
（同实验一）
==yc== f_parser: load name @.\test2.lua
==yc== open_func proto 000001710ACF8A50
==yc== open_func proto 000001710ACF9290
==yc== open_func proto 000001710ACF9110
==yc== f_parser: new proto 000001710ACF8A50
==yc== f_parser: new closure 000001710ACF6770

（同实验二，生成函数clsoure 000001710ACF5B30的过程）
==yc== execute closure 000001710ACF6770
start in test
==yc== OP_CLOSURE create closure 000001710ACF5B30 with proto 000001710ACF9290
==yc== OP_CLOSURE:OP_MOVE index 0
==yc== findupval : not found for level 181297808
==yc== findupval : create new upval 000001710ACF5BA0
==yc== OP_SETTABLE key functionOne

（调用函数 functionOne，并且发生了closure切换）
==yc== execute closure 000001710ACF5B30
execute function one
1
（在函数内嵌套生成了新的closure）
==yc== OP_CLOSURE create closure 000001710ACED9A0 with proto 000001710ACF9110
==yc== OP_CLOSURE:OP_GETUPVAL index 0
==yc== OP_CLOSURE:OP_MOVE index 1
==yc== findupval : not found for level 181297840
==yc== findupval : create new upval 000001710ACF5D60
execute function one end
（退出执行closure的时候，把这个栈上的upvalue从open变为close）
==yc== close upvalue 000001710ACF5D60

（切换回文件域的top-level closure）
==yc== execute closure 000001710ACF6770
end in test
（同实验一）
==yc== close upvalue 000001710ACF5BA0
```


## closure是如何被标记和释放的

在开始这段代码分析之前，先简单看下[Lua GC实现解析](https://github.com/lichuang/Lua-Source-Internal/blob/master/doc/ch08-GC.md)

这里概述一下每个对象的颜色经历会是：

一次fullgc：白色1（新创建）----灰色-----黑色-----白色1（这里会删除白色2的所有对象）
一次fullgc：白色2（新创建）----灰色-----黑色-----白色2（这里会删除白色1的所有对象）

通过这两种白色循环交替，不断删除不被引用的过程




### 实验一

本实验，想观察一下closure的创建和销毁时候的打印，因此在创建，标记，销毁那里打印一下即可

#### Lua源码修改


只想观察closure的创建标记
```
void luaC_link (lua_State *L, GCObject *o, lu_byte tt) {
  global_State *g = G(L);
  o->gch.next = g->rootgc;
  g->rootgc = o;
  o->gch.marked = luaC_white(g);
  o->gch.tt = tt;

  const char* name = "";
  if(tt == LUA_TFUNCTION)
  {
    name = "closure";
  }
  if(name != "")
  {
    printf("==yc== luaC_link %s %p mark white %d \n", name, o, luaC_white(g));
  }
}
```


```
void luaF_freeclosure (lua_State *L, Closure *c) {
  printf("==yc== free closure %p \n", c);
  int size = (c->c.isC) ? sizeCclosure(c->c.nupvalues) :
                          sizeLclosure(c->l.nupvalues);
  luaM_freemem(L, c, size);
}
```

#### Lua代码


```
print("start in test")
local M = {}

local upa = 1
function M.functionOne()
    print(upa)
end


M.functionOne = nil
collectgarbage("collect")
print("end in test")

return M
```

#### 执行结果

需要注意的是，还有许多Lua内置的closure创建和销毁过程，我们直接在输出中隐藏，为了避免干扰项：


```
（解析阶段，生成closure并标记成白色）
==yc== f_parser: load name @.\test3.lua
==yc== luaC_link  0000022CE1F45DD0 mark white 1
==yc== open_func proto 0000022CE1F45DD0
==yc== luaC_link  0000022CE1F49EB0 mark white 1
==yc== luaC_link  0000022CE1F4A170 mark white 1
==yc== open_func proto 0000022CE1F4A170
==yc== luaC_link  0000022CE1F49630 mark white 1
==yc== f_parser: new proto 0000022CE1F45DD0
==yc== luaC_link closure 0000022CE1F51430 mark white 1
==yc== f_parser: new closure 0000022CE1F51430
==yc== luaC_link closure 0000022CE1F517B0 mark white 1

（执行阶段）
==yc== execute closure 0000022CE1F51430
start in test
==yc== luaC_link  0000022CE1F4A0B0 mark white 1
（标记刚刚生成的closure）
==yc== luaC_link closure 0000022CE1F51890 mark white 1
==yc== OP_CLOSURE create closure 0000022CE1F51890 with proto 0000022CE1F4A170
==yc== OP_CLOSURE:OP_MOVE index 0
==yc== findupval : not found for level 3790866112
==yc== findupval : create new upval 0000022CE1F51BA0
==yc== OP_SETTABLE key functionOne
==yc== OP_SETTABLE key functionOne
（这里是因为将functionOne这个key设置成了nil，然后执行了fullgc，失去了关联引用，因此将该closure销毁）
==yc== free closure 0000022CE1F51890
end in test
```

### 实验二

本实验想观察一下closure在整个gc过程的颜色是如何一步步发生变化的


#### Lua源码修改


GC第一阶段入口函数，用于标识几个主对象：

```
/* mark root set */
static void markroot (lua_State *L) {
  printf("==yc== markroot begin \n");
  global_State *g = G(L);
  g->gray = NULL;
  g->grayagain = NULL;
  g->weak = NULL;
  markobject(g, g->mainthread);  //主线程
  /* make global table be traversed before main stack */
  markvalue(g, gt(g->mainthread)); //主线程的global表
  markvalue(g, registry(L)); //主线程的module表
  markmt(g);
  g->gcstate = GCSpropagate;
  printf("==yc== markroot end\n");
}
```


核心函数reallymarkobject，用于将白色object标记成灰色
```
static void reallymarkobject (global_State *g, GCObject *o) {
  lua_assert(iswhite(o) && !isdead(g, o));
  white2gray(o);
  printf("==yc== object %p type %d white to gray \n", o, o->gch.tt);
  switch (o->gch.tt) {
    case LUA_TSTRING: {
      return;
    }
    case LUA_TUSERDATA: {
      Table *mt = gco2u(o)->metatable;
      gray2black(o);  /* udata are never gray */
      if (mt) markobject(g, mt);
      markobject(g, gco2u(o)->env);
      return;
    }
    case LUA_TUPVAL: {
      UpVal *uv = gco2uv(o);
      markvalue(g, uv->v);
      if (uv->v == &uv->u.value)  /* closed? */
        gray2black(o);  /* open upvalues are never black */
      return;
    }
    case LUA_TFUNCTION: {
      gco2cl(o)->c.gclist = g->gray;
      g->gray = o;
      break;
    }
    case LUA_TTABLE: {
      gco2h(o)->gclist = g->gray;
      g->gray = o;
      break;
    }
    case LUA_TTHREAD: {
      gco2th(o)->gclist = g->gray;
      g->gray = o;
      break;
    }
    case LUA_TPROTO: {
      gco2p(o)->gclist = g->gray;
      g->gray = o;
      break;
    }
    default: lua_assert(0);
  }
}
```

核心函数propagatemark，用于将object从灰色标记为黑色，并且遍历自身引用的object，将他们从白色变成灰色，这是一个无限递归的过程，直到所有的灰色变成黑色为止
```
static l_mem propagatemark (global_State *g) {
  GCObject *o = g->gray;
  lua_assert(isgray(o));
  gray2black(o);
  printf("==yc== object %p type %d gray to black \n", o, o->gch.tt);
  switch (o->gch.tt) {
    case LUA_TTABLE: {
      Table *h = gco2h(o);
      g->gray = h->gclist;
      if (traversetable(g, h))  /* table is weak? */
        black2gray(o);  /* keep it gray */
      return sizeof(Table) + sizeof(TValue) * h->sizearray +
                             sizeof(Node) * sizenode(h);
    }
    case LUA_TFUNCTION: {
      Closure *cl = gco2cl(o);
      g->gray = cl->c.gclist;
      traverseclosure(g, cl);
      return (cl->c.isC) ? sizeCclosure(cl->c.nupvalues) :
                           sizeLclosure(cl->l.nupvalues);
    }
    case LUA_TTHREAD: {
      lua_State *th = gco2th(o);
      g->gray = th->gclist;
      th->gclist = g->grayagain;
      g->grayagain = o;
      black2gray(o);
      traversestack(g, th);
      return sizeof(lua_State) + sizeof(TValue) * th->stacksize +
                                 sizeof(CallInfo) * th->size_ci;
    }
    case LUA_TPROTO: {
      Proto *p = gco2p(o);
      g->gray = p->gclist;
      traverseproto(g, p);
      return sizeof(Proto) + sizeof(Instruction) * p->sizecode +
                             sizeof(Proto *) * p->sizep +
                             sizeof(TValue) * p->sizek + 
                             sizeof(int) * p->sizelineinfo +
                             sizeof(LocVar) * p->sizelocvars +
                             sizeof(TString *) * p->sizeupvalues;
    }
    default: lua_assert(0); return 0;
  }
}
```

sweep阶段（先忽略str部分），所有的对象从黑色变成白色（白1or白2）的过程


```
static GCObject **sweeplist (lua_State *L, GCObject **p, lu_mem count) {
  /* add for profiler */
  luaP_beginGCCB(L);

  GCObject *curr;
  global_State *g = G(L);
  int deadmask = otherwhite(g);
  while ((curr = *p) != NULL && count-- > 0) {
    if (curr->gch.tt == LUA_TTHREAD)  /* sweep open upvalues of each thread */
      sweepwholelist(L, &gco2th(curr)->openupval);
    if ((curr->gch.marked ^ WHITEBITS) & deadmask) {  /* not dead? */
      lua_assert(!isdead(g, curr) || testbit(curr->gch.marked, FIXEDBIT));
      makewhite(g, curr);  /* make it white (for next cycle) */
      printf("==yc== sweep not dead obj %p to white \n", curr);
      p = &curr->gch.next;
    }
    else {  /* must erase `curr' */
      lua_assert(isdead(g, curr) || deadmask == bitmask(SFIXEDBIT));
      *p = curr->gch.next;
      if (curr == g->rootgc)  /* is the first element of the list? */
        g->rootgc = curr->gch.next;  /* adjust first */


      printf("==yc== sweep dead obj %p free it \n", curr);
      freeobj(L, curr);
    }
  }

  /* add for profiler */
  luaP_afterGCCB(L);
  
  return p;
}
```


#### Lua代码

代码非常简单，就是看一下closure的颜色变化过程

```
print("start in test")

local function functionOne()
end

collectgarbage("collect")
print("end in test")
```

#### 执行结果

短短的几行代码，执行结果多达2000多行，我们先简化一下输出结果，只关注和closure与大流程相关的日志，结果如下：


```
（同上面的实验）
==yc== luaC_link  000002DF5C1E96D0 mark white 1 
==yc== open_func proto 000002DF5C1E96D0 
==yc== luaC_link  000002DF5C1E9050 mark white 1 
==yc== f_parser: new proto 000002DF5C1E9610
==yc== luaC_link closure 000002DF5C1F13C0 mark white 1 
==yc== f_parser: new closure 000002DF5C1F13C0
==yc== luaC_link closure 000002DF5C1F1200 mark white 1 
（虚拟机执行阶段）
==yc== execute closure 000002DF5C1F13C0 
start in test
（创建closure并标记成白色）
==yc== luaC_link closure 000002DF5C1F1270 mark white 1 
==yc== OP_CLOSURE create closure 000002DF5C1F1270 with proto 000002DF5C1E96D0
（gc入口，先将4个重要的obj标记成灰色）
==yc== markroot begin 
==yc== object 000002DF5C1DE5E0 type 8 white to gray 
==yc== object 000002DF5C1D70E0 type 5 white to gray 
==yc== object 000002DF5C1D30E0 type 5 white to gray 
==yc== object 000002DF5C1E8AD0 type 5 white to gray 
==yc== markroot end
（通过之前的几个root节点，开始递归root引用的对象，这里可以看到closure变灰，变黑的过程）
==yc== object 000002DF5C1E8AD0 type 5 gray to black 
==yc== object 000002DF5C1F1270 type 6 white to gray 
==yc== object 000002DF5C1EE9D0 type 4 white to gray 
==yc== object 000002DF5C1F1270 type 6 gray to black 
==yc== atomic begin 
==yc== object 000002DF5C1D4D30 type 5 gray to black 
==yc== object 000002DF5C1DE5E0 type 8 gray to black 
==yc== atomic end 

（sweep阶段，因为是另一种白色，所以标记成白色，进入下一个循环）
==yc== sweep not dead obj 000002DF5C1F1270 to white 
==yc== gc end 
end in test
（这里来自lua_state close的时候的luaC_freeAll，所以没有其他gc阶段，直接销毁所有对象）
==yc== sweep dead obj 000002DF5C1F1270 free it
```

==可以看出，lua的对象们在白，灰，黑三个阶段无限循环，只要不被markroot入口的四个跟对象引用，最终一定会被free==


### 实验三

在实验二的基础上，我们观察一下closure如何被显示的标记删除

#### Lua代码


```
print("start in test")
local M = {}

function M.functionOne()
end


M.functionOne = nil
collectgarbage("collect")
print("end in test")
return M
```

#### 执行结果


```
==yc== luaC_link  00000173AC569750 mark white 1 
==yc== open_func proto 00000173AC569750 
==yc== luaC_link  00000173AC568F50 mark white 1 
==yc== f_parser: new proto 00000173AC569690
==yc== luaC_link closure 00000173AC570950 mark white 1 
==yc== f_parser: new closure 00000173AC570950
==yc== luaC_link closure 00000173AC570B10 mark white 1 
==yc== execute closure 00000173AC570950 
start in test
==yc== luaC_link  00000173AC5693D0 mark white 1 
（closure生成，并标记为白色）
==yc== luaC_link closure 00000173AC5708E0 mark white 1 
==yc== OP_CLOSURE create closure 00000173AC5708E0 with proto 00000173AC569750
==yc== OP_SETTABLE key functionOne
==yc== fullgc begin 
==yc== markroot begin 
==yc== object 00000173AC55E5D0 type 8 white to gray 
==yc== object 00000173AC55A5E0 type 5 white to gray 
==yc== object 00000173AC555EF0 type 5 white to gray 
==yc== object 00000173AC569550 type 5 white to gray 
==yc== markroot end 
（第一次fullgc，还被引用，标记成黑色）
==yc== object 00000173AC5708E0 type 6 white to gray 
==yc== object 00000173AC5708E0 type 6 gray to black 
==yc== atomic begin 
==yc== object 00000173AC561DB0 type 5 gray to black 
==yc== object 00000173AC55E5D0 type 8 gray to black 
==yc== atomic end  
（sweep阶段，被标记成白色）
==yc== sweep not dead obj 00000173AC5708E0 to white 
==yc== gc end 
==yc== fullgc end 
（将functionOne解引用）
==yc== OP_SETTABLE key functionOne
==yc== fullgc begin 
（这里是每次fullGC进入下一次循环前，将所有的object标记成白色，所以还会被标记一次）
==yc== sweep not dead obj 00000173AC5708E0 to white 
（第二次fullgc流程开始）
==yc== markroot begin 
==yc== object 00000173AC55E5D0 type 8 white to gray 
==yc== object 00000173AC55A5E0 type 5 white to gray 
==yc== object 00000173AC555EF0 type 5 white to gray 
==yc== object 00000173AC569550 type 5 white to gray 
==yc== markroot end
（找不到引用关系，注意比对第一次，这里没有变成黑色）
==yc== atomic begin 
==yc== object 00000173AC561DB0 type 5 gray to black 
==yc== object 00000173AC55E5D0 type 8 gray to black 
==yc== atomic end  
（发现是另一种白，直接删除closure对象）
==yc== sweep dead obj 00000173AC5708E0 free it 
==yc== gc end 
==yc== fullgc end 
end in test
（对比上一个实验，这里close_state，也找不到被删掉的closure啦）
==yc== sweep dead obj 00000173AC5693D0 free it 
```


## upvalue是如何被标记和释放的

理解完closure颜色标记的过程，我们来观察一下upvalue颜色标记的过程

### 实验一

#### 源码修改

```
UpVal *luaF_findupval (lua_State *L, StkId level) {
  global_State *g = G(L);
  GCObject **pp = &L->openupval;
  UpVal *p;
  UpVal *uv;
  while (*pp != NULL && (p = ngcotouv(*pp))->v >= level) {
    lua_assert(p->v != &p->u.value);
    if (p->v == level) {  /* found a corresponding upvalue? */
      if (isdead(g, obj2gco(p)))  /* is it dead? */
        changewhite(obj2gco(p));  /* ressurect it */
      return p;
    }
    pp = &p->next;
  }
  printf("==yc== findupval : not found for level %p\n", level);
  uv = luaM_new(L, UpVal);  /* not found: create a new one */
  printf("==yc== findupval : create new upval %p\n", uv);
  uv->tt = LUA_TUPVAL;
  uv->marked = luaC_white(g);
  printf("==yc== findupval : upval %p make white \n", uv);
  uv->v = level;  /* current value lives in the stack */
  printf("==yc== findupval : link upval %p to value %p object %p\n", uv, level, gcvalue(level));
  uv->next = *pp;  /* chain it in the proper position */
  *pp = obj2gco(uv);
  uv->u.l.prev = &g->uvhead;  /* double link it in `uvhead' list */
  uv->u.l.next = g->uvhead.u.l.next;
  uv->u.l.next->u.l.prev = uv;
  g->uvhead.u.l.next = uv;
  lua_assert(uv->u.l.next->u.l.prev == uv && uv->u.l.prev->u.l.next == uv);

  return uv;
}
```

#### Lua代码

```
print("start in test")
local M = {}

local up = {}

function M.functionOne()
    print(up)
end

print("end in test")
return M
```

#### 执行结果


```

==yc== luaC_link  000001EE0009A2C0 mark white 1 
==yc== f_parser: load name @.\test6.lua 
==yc== luaC_link  000001EE000961F0 mark white 1 
==yc== open_func proto 000001EE000961F0 
==yc== luaC_link  000001EE0009A140 mark white 1 
==yc== luaC_link  000001EE0009A680 mark white 1 
==yc== open_func proto 000001EE0009A680 
==yc== luaC_link  000001EE00099DC0 mark white 1 
==yc== f_parser: new proto 000001EE000961F0
==yc== luaC_link closure 000001EE000A2400 mark white 1 
==yc== f_parser: new closure 000001EE000A2400
==yc== luaC_link closure 000001EE000A2F60 mark white 1 
==yc== execute closure 000001EE000A2400 
start in test
==yc== luaC_link  000001EE0009A1C0 mark white 1 
==yc== luaC_link  000001EE0009A240 mark white 1 
==yc== luaC_link closure 000001EE000A2A90 mark white 1 
==yc== OP_CLOSURE create closure 000001EE000A2A90 with proto 000001EE0009A680
==yc== OP_CLOSURE:OP_MOVE index 0

（引用local up = {} 这个upvalue没有被创建出来，直接创建一个upvalue并标记成白色）
==yc== findupval : not found for level 000001EE00090AE0
==yc== findupval : create new upval 000001EE000A2550
==yc== findupval : upval 000001EE000A2550 make white 
（这里把这个upvalue的地址指向了 栈上的对象）
==yc== findupval : link upval 000001EE000A2550 to value 000001EE00090AE0 object 000001EE0009A240
==yc== OP_SETTABLE key functionOne
end in test
（根据之前的实验，跳出closure的时候，会把还在该closure堆栈上的upvalue关闭）
==yc== close upvalue 000001EE000A2550 
（根据之前的实验，close的时候会freeAll，删除所有object）
==yc== sweep dead obj 000001EE000A2550 free it 
==yc== sweep dead obj 000001EE000A2A90 free it 
==yc== sweep dead obj 000001EE0009A240 free it 
```


### 实验二

本实验想观察一下，upvalue以及upvalue指向的对象是怎么保持黑色的

#### Lua代码


```
print("start in test")
local M = {}

local up = {}

function M.functionOne()
    print(up)
end

collectgarbage("collect")

print("end in test")
return M
```


#### 执行结果

从结果可以分析出整个链条是：

closure ---- upvalue ---- object（真正被引用的对象）

```
==yc== execute closure 00000269436636A0 
start in test

（同上一个实验的创建过程）
==yc== luaC_link  000002694365ADF0 mark white 1 
==yc== luaC_link  000002694365AF70 mark white 1 
==yc== luaC_link closure 0000026943662E50 mark white 1 
==yc== OP_CLOSURE create closure 0000026943662E50 with proto 000002694365BAB0
==yc== OP_CLOSURE:OP_MOVE index 0
==yc== findupval : not found for level 000002694364F360
==yc== findupval : create new upval 0000026943663550
==yc== findupval : upval 0000026943663550 make white 
==yc== findupval : link upval 0000026943663550 to value 000002694364F360 object 000002694365AF70
==yc== OP_SETTABLE key functionOne
==yc== fullgc begin 
==yc== markroot begin 
==yc== markroot end
==yc== traverse closure 00000269436547B0 
==yc== object 000002694365AF70 type 5 white to gray 
==yc== object 00000269436608D0 type 4 white to gray 
==yc== object 000002694365AF70 type 5 gray to black 
==yc== traverse table 000002694365AF70 
==yc== object 000002694365ADF0 type 5 gray to black 
==yc== traverse table 000002694365ADF0 
==yc== object 00000269436606F0 type 4 white to gray 
==yc== object 0000026943662E50 type 6 white to gray 
==yc== object 0000026943662E50 type 6 gray to black 
（这里可以看到通过closure遍历到了upvalue）
==yc== traverse closure 0000026943662E50 
==yc== object 000002694365BAB0 type 9 white to gray 
==yc== traverse closure try to mark upvalue 0000026943663550 
==yc== object 0000026943663550 type 10 white to gray 
（这里看到通过upvalue找到了最终指向的object，在这个用例下，该对象已经不是白色，所以没有真正标记）
==yc== try to mark upvalue value 000002694365AF70 
==yc== object 000002694365BAB0 type 9 gray to black 
==yc== object 00000269436636A0 type 6 gray to black 
==yc== atomic begin 
==yc== object 000002694364F710 type 5 gray to black 
==yc== traverse table 000002694364F710 
==yc== object 0000026943651040 type 8 gray to black 
==yc== atomic end 
==yc== sweep not dead obj 000002694365AF70 to white 
==yc== sweep not dead obj 0000026943663550 to white 
==yc== gc end 
==yc== fullgc end 
end in test
==yc== close upvalue 0000026943663550 
==yc== sweep dead obj 0000026943663550 free it 
==yc== sweep dead obj 0000026943662E50 free it 
==yc== sweep dead obj 000002694365AF70 free it
```


# 排查思路

这里详细记录了我排查这个问题的过程，通过这个过程，可以不断提醒自己该如何更好更快地接触到问题本质

## 排查过程

1. D项目反馈说连续两次重复执行reload会导致crash
2. 考虑到lua虚拟机5.1.5的稳定性，不可能是原有代码导致的，因为自己从lua5.2移植过upvaluejoin，且只有reload使用，大概率推测是这里出了问题
3. 在reload的代码中，把upvaluejoin的部分注释掉，发现确实不会crash了，笃定是upvaluejoin的实现有问题，开始排查问题
4. 每次观察crash堆栈，发现挂的问题都不一样，于是开始在lua虚拟机里插装，用于观测最后出问题的地址总是一个table
5. 继续插桩，检测每次table生成是由哪段lua代码产生的，参考lua虚拟机内置的traceback即可
6. 通过4,5发现，每次都不一样，试了几次后，想起了Asan（==应该早点想到的，内存问题，一定要引入asan==，早点想到这里，可以节约好多试错时间）
7. 通过6之后，每次的错误都稳定在了gc环节的traverseclosure，发现traverse的过程，此时upvalue引用的对象table已经被释放掉了
8. 怀疑upvaluejoin的实现有问题，开始不断魔改，还是无法解决
9. 重新回退思路，看一下是哪个table一直出问题，发现是LogManager生成的Logger，开始从代码排查这个Logger有啥特殊的
10. 排查出来这个logger同时调用了upvaluejoin和setupvalue两个方法
11. 有没有可能upvaluejoin是对的，setupvalue的实现和他不兼容（一个是5.1.5，一个是5.2的）
12. 含泪发现确实如此，解决方案是setupvalue参考5.2改了一版，解决了问题


## 最终导致问题的原因


### 简略版

1. f（非黑） --》 up(自己原有,什么颜色都行 --> tvalue（什么颜色都行）
2. 调用upvaluejoin， f(非黑） ---> up(其他的，颜色为黑） --->tvalue(黑色）
3. 调用setupvalue    f(非黑） -----up(保持不变，黑色） ---> tvalue(新的，白色），这里绑定的是f的关系，因为f非黑，tvalue保持为白色
4. traverseClosure，从f开始标记，因为up是黑色，所以tvalue没办法被标记到，最终变为垃圾，被回收，导致crash

### 详细版

需要明确的是，因为lua的gc是增量式gc，所以中断在哪一步都是正常的

```
local newTvalue = {}

local old = {}
local functionOld = function（）
    print(old)
end

local new = {}
local functionNew = function（）
    print(new)
end
（此时所有的变量应该上一轮sweep，被标记成白色1）

xxxxxx(lua虚拟机执行一大堆其他代码)

（开始了新一轮gc过程，markroot时，切换成白色2）

（在标记阶段，把functionNew，upvalue和tvalue都标记成黑色）

debug.upvaluejoin(functionOld, 1, functionNew, 1)

(此时functionOld是白色的，指向了一个黑色的upvalue与tvalue)
debug.setupvalue(functionOld, 1, newTvalue)

(此时functionOld是白色的，指向了黑色的upvalue和白色的tvalue，这里本质是lua5.1.5的setupvalue实现有问题，绑定的是closure关系而不是upvalue关系，越过了upvalue直接去找closure带来了问题)


（继续刚才的标记阶段，标记到functionOld，因为upvalue是黑色，阻断了整个链条向tvalue遍历，tvalue保持为白色1，所以目前的情况是黑色的functionOld指向了黑色的upvalue指向了白色1的tvalue）

（进入sweep阶段，发现了白色1的tvalue，与当前白色2不同，直接清理掉tvalue）

xxxxxx（lua虚拟机执行了一堆逻辑后）
（第三次进入gc的标记阶段，从functionOld开始遍历，遍历到upvalue引用的tvalue时，crash，因为这块内存已经被释放）
```


# reference

1. [构建Lua解释器Part11：Upvalue](https://zhuanlan.zhihu.com/p/358423900)
2. [Lua GC实现解析](https://github.com/lichuang/Lua-Source-Internal/blob/master/doc/ch08-GC.md)



