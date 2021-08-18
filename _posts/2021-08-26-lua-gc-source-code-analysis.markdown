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
        - 添加一个监控，监控程序正常运行过程中的所有引用改变。如果一个black对象需要引用一个white对象，存在两种处理办法。
            - 将white对象设置成gray，并添加到gray列表中等待扫描。等同于帮整个GC的标志过程推进一步
            - 将black对象改成gray，并添加到gray列表中等待扫描。等同于使GC的标志过程后退一步。
    - 这种垃圾回收方式称为"Incremental Garbage Collection"(简称为"IGC"），增量垃圾回收。lua 就是采用这种方法。
        - 代价？ IGC所检测出来的垃圾对象集合比实际的集合要少。有些GC过程中变成垃圾的对象，有可能在本轮GC中检测不到，不过这些残余的垃圾对象一定会在下一轮GC被检测出来，不会造成泄露。

