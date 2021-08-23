---
layout:     post
title:      "LuaGC源码剖析"
subtitle:   "LuaGC源码剖析"
date:       2019-11-29
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - LuaGC源码解析
---
- 参考云风的luaGC的源码剖析与yuanlin2008的探索Lua5.2内部实现:Garbage Collection 原理系列文章。
https://blog.codingnow.com/2011/03/lua_gc_1.html
https://blog.csdn.net/yuanlin2008/article/details/8558103

- lua 采用简单的**标记清除算法**的GC系统（mark-and-sweep）
- lua 一共9种数据类型，分别问nil、boolean、lightuserdata、number、string、table、function、userdata和thread。其中只有string、table、function、thread四种在vm中以**引用**方式共享，是需要被GC管理回收䣌对象，其它类型都以**值**形式存在。
- 首先，系统管理者所有已创建的对象。每个对象都有对其他对象的引用。root集合代表已知的系统级别的对象引用。从root触发，就可以访问到系统引用到的所有对象。而没有被访问到的对象就是垃圾，需要被销毁。

- 将对象分成三个状态
    - white 待访问状态，表示对象还没有被垃圾回收的过程访问到。
    - gray 待扫描状态，表示对象已被垃圾回收访问到，但是对象本身对于其他对象的引用还没有进行遍历访问。
    - black 已扫描状态，表示对象已被访问，且也遍历了对象对其他对象的引用

- 基本算法描述如下

```
当前所有对象都是White状态;
将root集合引用到的对象从White设置成Gray，并放到Gray集合中;
while(Gray集合不为空)
{
	从Gray集合中移除一个对象O，并将O设置成Black状态;
	for(O中每一个引用到的对象O1) {
		if(O1在White状态) {
			将O1从White设置成Gray，并放到到Gray集合中；
		}
	}
}
for(任意一个对象O){
	if(O在White状态)
		销毁对象O;
	else
		将O设置成White状态;
}
```

- 上面的算法如果一次性执行，在对象很多的情况下会执行很长时间，严重影响程序本身的响应速度。
    - 解决？将上面的算法分步执行。
        - 首先标志所有root对象
            - 当前所有对象都是White状态;
            - 将root集合引用到的对象从White设置成Gray，并放到Gray集合中;
        - 遍历访问所有的gray对象。如果超出了本次计算量上限，退出等待下一次遍历

            ```
            while(Gray集合不为空,并且没有超过本次计算量的上限){
                从Gray集合中移除一个对象O，并将O设置成Black状态;
                for(O中每一个引用到的对象O1) {
                    if(O1在White状态) {
                        将O1从White设置成Gray，并放到到Gray集合中；
                    }
                }
            }
            ```
        - 销毁垃圾对象

        ```
        for(任意一个对象O){
            if(O在White状态)
                销毁对象O;
            else
                将O设置成White状态;
        }
        ```
    - 每个步骤之间，由于程序可以正常执行，所以会破坏当前对象之间的引用关系。black对象表示已经被扫描的对象，所以他应该不能引用到一个white对象。当程序的改变使得一个white对象引用到一个black对象时，就会造成错误。如何解决？
        - 添加一个barrier，我理解为监控，监控程序正常运行过程中的所有引用改变。如果一个black对象需要引用一个white对象，存在两种处理办法。
            - 将white对象设置成gray，并添加到gray列表中等待扫描。等同于帮整个GC的标志过程推进一步
            - 将black对象改成gray，并添加到gray列表中等待扫描。等同于使GC的标志过程后退一步。
    - 这种垃圾回收方式称为"Incremental Garbage Collection"(简称为"IGC"），增量垃圾回收。lua 就是采用这种方法。
        - 代价？ IGC所检测出来的垃圾对象集合比实际的集合要少。有些GC过程中变成垃圾的对象，有可能在本轮GC中检测不到，不过这些残余的垃圾对象一定会在下一轮GC被检测出来，不会造成泄露。

- GCObject
    - Lua使用union GCObject来表示所有的垃圾回收对象

        ```
        182 /*
        183 ** Union of all collectable objects
        184 */
        185 union GCObject {
        186   GCheader gch;  /* common header */
        187   union TString ts;
        188   union Udata u;
        189   union Closure cl;
        190   struct Table h;
        191   struct Proto p;
        192   struct UpVal uv;
        193   struct lua_State th;  /* thread */
        194 };
        ```

    - 这就相当于在C++中，将所有的GC对象从GCheader派生，他们都共享GCheader。
        ```
        74 /*
        75 ** Common Header for all collectable objects (in macro form, to be
        76 ** included in other objects)
        77 */
        78 #define CommonHeader    GCObject *next; lu_byte tt; lu_byte marked
        79 
        80 
        81 /*
        82 ** Common header in struct form
        83 */
        84 typedef struct GCheader {
        85   CommonHeader;  
        86 } GCheader;
        ```

    - marked这个标志用来记录对象与GC相关的一些标志位。其中0和1位表示对象的white状态和垃圾状态。当垃圾回收的标识阶段结束后，剩下的white对象就是垃圾对象。由于lua并不是立即清除垃圾对象，这些对象还会再系统中存在一段时间，所以需要能够区分同为white状态的垃圾对象和非垃圾对象。
        - 如何解决？
            - lua 使用两个标志位来表示white。这个标志位会轮流被当作white状态标志，另一个表示垃圾状态。在global_state中保存着一个currentwhite，来表示当前是哪个标志位用来标识white。每当GC标识阶段完成，系统会切换这个标志位，这样原来white的所有对象不需要遍历就变成了垃圾对象，而真正的white对象则使用新得标志位标识。
    - 第2个标志位用来标识black状态，而既非white也非black就是gray
        - 除了short string和open upvalue之外，所有的GCObject都通过next被串接到全局状态global_State中的allgc链表上。我们可以通过遍历allgc链表来访问系统中的所有GCObject。short string被字符串标单独管理。open upvalue会在被close时也连接到allgc上。
            - open、close 如何理解
                - 当前作用域结束（当退出upvalue的语法域），open upvalue变为close

                ![upvalue的关系](http://csoap.github.io/img/in-post/post-js-version/gc_2.png "upvalue的关系")
        - upavlue ?
            - Upvalue对象在垃圾回收中的处理是比较特殊的。
                - 对于open状态的upvalue，其v指向的是一个stack上的TValue，所以open upvalue与thread的关系非常紧密。引用到open upvalue的只可能是其从属的thread，以及lua闭包。如果没有lua闭包引用这个open upvalue，就算他一定被thread引用着，也已经没有实际的意义了，应该被回收掉。也就是说thread对open upvalue的引用完全是一个弱引用。所以Lua没有将open upvalue当作一个独立的可回收对象，而是将其清理工作交给从属的thread对象来完成。在mark过程中，open upvalue对象只使用white和gray两个状态，来代表是否被引用到。通过上面的引用关系可以看到，**有可能引用open upvalue的对象只可能被lua闭包引用到。所以一个gray的open upvalue就代表当前有lua闭包正在引用他，而这个lua闭包不一定在这个thread的stack上面。在清扫阶段，thread对象会遍历所有从属于自己的open upvalue。如果不是gray，就说明当前没有lua闭包引用这个open upvalue了，可以被销毁。**
                - 当退出upvalue的语法域或者thread被销毁，open upvalue会被close。所有close upvalue与thread已经没有弱引用关系，会被转化为一个普通的可回收对象，和其他对象一样进行独立的垃圾回收。




