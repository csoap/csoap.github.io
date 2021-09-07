---
layout:     post
title:      "灯泡时刻（不定期更新）"
subtitle:   "突然对某个知识点有疑问的笔记"
date:       2021-08-13
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 面试
---

- 加密问题
    - 游戏防作弊
        - 怎么做？
            - 用配置文件限定防作弊数值范围
                - 如果解包更改配置文件，有办法能够防止吗？
                    - 配置文件用ab加密
                        - ab如何加密的呢？加密解密，LoadFromMemory会造成什么问题？大批量造成内存过大？如何解决内存过大 （这个方案带来的次生风险更大）
                    - 还有什么方法？
                        - Anti-Cheat Toolkit https://www.bilibili.com/video/av668088669/

- 网络
    - TCP和UDP区别
    - HTTP报文都包含什么
    - HTTP 如何保持长连接
    - TCP可靠协议
    - 拥塞控制
    - TCP滑动窗口
    - TCP流式传输
- 算法
    - 两个栈实现一个队列
        - 让其中一个栈作为队列的入口，负责插入新元素；另一个栈作为队列的出口，负责移除老的元素
        - 具体操作:入队时，将元素压入s1。出队时，判断s2是否为空，如不为空，则直接弹出顶元素；如为空，则将s1的元素逐个“倒入”s2，把最后一个元素弹出并出队。这个思路，避免了反复“倒”栈，仅在需要时才“倒”一次。
        - https://blog.csdn.net/ailunlee/article/details/85100514
        - https://www.cnblogs.com/wanghui9072229/archive/2011/11/22/2259391.html
    - 第K大的数
        - 暴力法。先将数组排序（nlogn），如果是从大到小，返回第K个，如果从小到大，返回第n-k个（n表示元素个数）
        - 优先队列priorityQueue，内部实现是维护一个大小为k的最小堆。
        - 快速选择算法（快排）
    - 优先队列
    - 双向链表
    - DFS、BFS
    - 快排的时间复杂度和具体实现
    - 一篇英文文章，求每个单词的出现频率？算法的时间复杂度是多少？优化这个算法，能否加快这个过程？

- 计算机基础
    - 代码调用栈原理
    - 打开一个网址发生了什么
        - 浏览一个网页，要经过若干个路由器的转发才能达到目标计算机，这个经过路由器转发的过程称为路由。
        - 在互联网的世界中，本应使用 IP 地址这样的数字来标识计算机才是，而刚刚却能使用一串字符 www.grapecity.com 来标识 Grape City 的 Web 服务器。实际上，在互联网中还存在着一种叫作 DNS（Domain Name System，域名系统）的服务器。正是该服务器为我们把 www.grapecity.com 这样的域名解析为了 210.160.205.80 这样的 IP 地址 。如果一台 DNS服务器无法解析域名，它就会去询问其他的 DNS 服务器。
- C#
    - c#中，ArrayList和list，dictionary和hashtable区别，dictionary的底层实现原理
        - https://www.cnblogs.com/yiyi20120822/p/11429137.html
        https://www.cnblogs.com/TiestoRay/p/4891026.html
        - Dictionary与Hashtable
            - 数据结构上来说都属于Hashtable, 都是对关键字（键值）进行散列操作,将关键字散列到Hashtable的某一个槽位中去，不同的是处理碰撞的方法
            > Dictionary采用链表法处理碰撞,通过Hash算法来碰撞到指定的Bucket上，碰撞到同一个Bucket槽上所有数据形成一个单链表。
            ```
            int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            //将HashCode的返回值转化为数组索引
            int bucketIndex = hashCode % buckets.Length;
            ```
            HashTable采用开放寻址法方法中的双重散列处理碰撞
            - dictionary
                - 有泛型优势（类型安全，性能更好），对于值类型，不存在装箱和拆箱的性能损耗
                - 读取速度快（体现在单条数据上）
                - 容量利用更充分
                - 非线程安全,多线程必须人为使用 lock 语句进行保护,  效率大减.
                - 有序（遍历时输出的顺序就是加入的顺序）
            - hashtable
                - 适合多线程
                - 通过静态方法Synchronize方法可获得完全线程安全的类型
                - 无序
        - List与Dictionary
            - List有点类似于Dictionary。二者都具有使用泛型的优点(联想到ArrayList)，Dictionary没有在内存中移动后续元素的性能开销。
            > ArrayList是可变长数组，你可以将任意多的数据Add到ArrayList里面。其内部维护的数组，当长度不足时，会自动扩容为原来的两倍。但是ArrayList也有一个缺点，就是存入ArrayList里面的数据都是Object类型的，所以如果将值类型存入和取出的时候会发生装箱、拆箱操作(就是值类型与引用类型之间的转换)，这个会影响程序性能。在.Net 2.0泛型出现以后，就提供了List<T>。List<T>是ArrayList的泛型版本，它不再需要装箱拆箱，直接取，直接用
            - List是在数组的基础上做的封装,本质是有序的动态数组，遍历查询更快(数据较多时)，Dictionary单条查询更快
            > 同样是集合，为什么性能会有这样的差距。我们要从存储结构和操作系统的原理谈起。首先我们清楚List<T>是对数组做了一层包装，我们在数据结构上称之为线性表，而线性表的概念是，在内存中的连续区域，除了首节点和尾节点外，每个节点都有着其唯一的前驱结点和后续节点。我们在这里关注的是连续这个概念。而HashTable或者Dictionary，他是根据Key和Hash算法分析产生的内存地址，因此在宏观上是不连续的，虽然微软对其算法也进行了很大的优化。由于这样的不连续，在遍历时，Dictionary必然会产生大量的内存换页操作，而List只需要进行最少的内存换页即可，这就是List和Dictionary在遍历时效率差异的根本原因。
            >> 在这里我们除了刚才的遍历问题，还要提到Dictionary的存储空间问题，在Dictionary中，除了要存储我们实际需要的Value外，还需要一个辅助变量Key，这就造成了内存空间的双重浪费。而且在尾部插入时，List只需要在其原有的地址基础上向后延续存储即可，而Dictionary却需要经过复杂的Hash计算，这也是性能损耗的地方。

    - CLI、CIL、JIT、Mono
        - C#编译和执行都要依赖CLI(公共语言基础结构)，c#生成中间语言指令，也就是公共中间语言（CIL）
        - CLI是理解C#程序的执行环境以及C#如何与其他程序和库（甚至是用其他语言写的）进行无缝交互的一个重要规范
        - Mono是开源的跨平台的CLI实现。
        - C#通过C#编译器（如 Mono），编译称公共中间语言（CIL），将CIL转换成处理器能执行的指令。
        - “虚拟执行系统”负责管理C#程序的执行，它能理解CIL语句，并将其编译程机器码，这个组件称为即时编译器(JIT)

    - 什么是.NET？什么是CLI？什么是CLR？IL是什么？JIT是什么，它是如何工作的？GC是什么，简述一下GC的工作方式？
    - 类（class）和结构（struct）的区别是什么？它们对性能有影响吗？.NET BCL里有哪些是类（结构），为什么它们不是结构（类）？在自定义类型时，您如何选择是类还是结构？
    - 在.NET程序运行过程中，什么是堆，什么是栈？什么情况下会在堆（栈）上分配数据？它们有性能上的区别吗？“结构”对象可能分配在堆上吗？什么情况下会发生，有什么需要注意的吗？
    - 泛型的作用是什么？它有什么优势？它对性能有影响吗？它在执行时的行为是什么？.NET BCL中有哪些泛型类型？举例说明平时编程中您定义的泛型类型。
    - 异常的作用是什么？.NET BCL中有哪些常见的异常？在代码中您是如何捕获/处理异常的？在“catch (ex)”中，“throw”和“throw ex”有什么区别？您会如何设计异常的结构，什么情况下您会抛出异常？
    - List<T>和T[]的区别是什么，平时你如何进行选择？Dictionary<TKey, TValue>是做什么的？.NET BCL中还有哪些常用的容器？它们分别是如何实现的（哪种数据结构）？分别是适用于哪些场景？
    - 抽象类和接口有什么区别？使用时有什么需要注意的吗？如何选择是定义一个“完全抽象”的抽象类，还是接口？什么是接口的“显式实现”？为什么说它很重要？
    - 字符串是引用类型类型还是结构类型？它和普通的引用类型相比有什么特别的地方吗？使用字符串时有什么需要注意的地方？为什么说StringBuilder比较高效？在连接多个字符串时，它无论何时都比直接相加更高效吗？
    - 如何高效地进行数组复制？“二维数组”和“数组的数组”有什么区别？在使用双重循环遍历一个二维数组时，如何选择内外层的遍历顺序？
什么是元编程，.NET有哪些元编程的手段和场景？什么是反射？能否举一些反射的常用场景？有人说反射性能较差，您怎么看待这个问题？有什么办法可以提高反射的性能吗？
    - 委托是什么？匿名方法是什么？在C# 3.0中，Lambda表达式是什么？扩展方法是什么？LINQ是什么？您觉得C# 3.0中还有哪些重要的特性，它们带来了什么优势？BCL中哪些类库和这些特性有关？您平时最常用哪些？
    - 工作之外您看哪些技术相关的书、网站、社区、项目等等？您还接触哪些.NET以外的技术，能和.NET或.NET中有针对性的部分做个对比吗？

- unity
    - untiy游戏 C#和lua是如何交互的？
    - Unity 与Mono和.Net的关系
        - .Net拥有跨语言，跨平台性。Unity引擎需求也是需要跨平台，支持多语言（C#，Js，Boo）。就参考微软开发.Net Core的概念，于是，推出了Mono.
        - Unity之所以能跨平台是因为有通用中间语言（CIL），CIL是一种可读性比较低的面向对象的语言。它是基于堆栈的，与具体CPU中的寄存器无关。CIL运行在Mono运行时（虚拟机）上，其实它能运行在任何支持CLI(通用语言基础结构)的平台上，包括.NET与Mono，所以能实现跨平台。即 虽然IOS系统不能运行.exe文件，但是mono虚拟机可以，所以可以跨平台。
    - mvc 是什么？描述一下如何运用
    - 背包一滚动 就卡?

    - 模拟unity管理GameObject上Mono组件的生命周期？
        - Mono是什么？包含了实现特定脚本函数的组件类
        - 组件时什么？组件在unity中是如何实现？gameobjet是组件的容器
        - 生命周期是什么？unity 是如何管理生命周期，本质上通过遍历
        - unity 引擎是什么？管理项目中的资源+程序运行中的游戏对象树

    - 打动态图集有什么规范？图片的压缩格式用什么？背包商店的图片怎么打成图集是最优的？动态图集的大小是怎么设定的？使用动态图集有什么缺点？图集能不能做缓存?缓存的策略是什么？
    - ui框架实现了什么功能？如何管理面板层级关系，遮挡关系，互斥关系？如何做缓存策略？
    - 如果ui界面打开比较慢，有可能有什么问题。
        - 第一次加载慢，什么问题？
        - 第二次加载慢？
    - 一个界面打开的时候ui动画应该怎么做效率最高？
    - 断线重连? 对于帧同步、状态同步处理方法是不一样的
        - 状态同步需要恢复游戏的状态数据。
        - 帧同步需要从第一帧开始追帧到当前最新游戏帧。如何实现？
    - 战斗系统怎么实现？
        - 如果放了一个技能，如何判定对方是否收到伤害？
        - 收到伤害如何做飘出伤害文字？伤害文字怎么优化？
    - 协程底层原理
        > unity有多条渲染线程，但是对于代码的调度是在一个主线程，协程是靠操作系统在主线程的调度实现的
        - 实现原理
            - yield 相当是暂停本次生命周期执行，或者是转移控制权给unity继续生命周期
                - 每次执行到yield 暂停一下，然后下次又继续执行 如何实现？ 本质上是通过IEnumetator.MoveNext()实现的
            - 生成一个CountCoroutine的类,本质是状态机。构造方法会设置一个初始状态state为0，当state != 1 结束协程。这个类持有一个NevBehaviourScript的脚本对象，也就是为什么访问到脚本对象的各个字段
        - 协程的缺点
            - 依赖于MonoBehaviour
                - 大型商业游戏很有可能脚本不会继承自MonoBehaviour
                - MVC框架，基本只有V跟monoBehaviour打交道，M和C基本不会打交道，如果想在M或C启动协程（例：计算buff持续伤害等）就会有问题。
                - 怎么解决？实现一个自定义调度器，怎么实现？需要了解一下
            - 不能有返回值
            - 回调地狱，回调层数多了以后维护起来太复杂。。
                - 例子：如果需求是下载一张图片后下载另一张，持续很多张图片，就变成俄罗斯套娃了。
                ```
                hw.GET("xxx", (www1) =>
                {
                    hw.Get("yyy", (www2)=>
                    {

                    });
                });

                //怎么解决，改成用 Async/Await? await什么原理?又需要去了解了。。
                await hw.Get("xxx");
                //do something
                await hw.Get("yyy");
                //do something
                ```
        - 协程有什么坑点
            - 协程的停止和创建必须用相同形式。字符串、IEnumerator、Coroutine形式
            - 禁用脚本(this.enabled = false)不会停止协程执行
            - 删除脚本（Destory(this)）或禁用GameObject会停止协程执行
    - 请简述GC（垃圾回收）产生的原因，并描述如何避免
        - 当用new创建对象时，当可分配的内存不足GC就会去回收未使用的对象，但是GC的操作是非常复杂的，会占用很多CPU时间，对于移动设备来说频繁的垃圾回收会严重影响性能。下面的建议可以避免GC频繁操作。
            - 减少用new创建对象的次数，在创建对象时会产生内存碎片，这样会造成碎片内存不法使用；
            - 使用公用的对象（静态成员，常量），但是不能乱用，因为静态成员和常量的生命周期是整个应用程序；
            - 在拼接大量字符串StringBuilder;并设置初始大小如：StringBuilder sbHtml = new StringBuilder (size);
            - 使用object pool(对象池)；
            - 5）定时进行主动GC操作，如场景切换；
            - 6）调用 StartCoroutine()会产生少量的内存垃圾，因为unity会生成实体来管理协程，yield在协程中不会产生堆内存分配，但是如果yield带有参数返回，则会造成不必要的内存垃圾；
    - C#的委托是什么？有何用处？
        - 委托类似于一种安全的指针引用，应当做类来看待而不是一个方法，相当于对一组方法的列表的引用。
	    - 用处：使用委托使程序员可以将方法引用封装在委托对象内。然后可以将该委托对象传递给可调用所引用方法的代码，而不必在编译时知道将调用哪个方法。与C或C++中的函数指针不同，委托是面向对象，而且是类型安全的。
    - Unity中Lua造成的堆内存泄露问题
        - 当Unity的Object被销毁时，并没有机会通知到Lua。此时，如果引用该对象的Lua变量没有通过LuaGC掉（LuaGC会通知ToLua的字典清理对应数据），则这个已经被Destroy的对象就一直被引用住了。项目中的Lua变量没有被LuaGC掉的情况有以下几种情况:
            - Lua对象是全局变量，直接放在_G中
                - 禁止定义全局变量，给现有的全局变量前加载local声明。可以使用一些Lua静态语法检查的手段，如Luacheck来检查。
            - Lua对象被一些全局的Table引用。
                - 我们每个UI面板都对应MVC结构，用了面向对象的概念。其中view在面板关闭时会直接置空，但Ctrl和Model都不会，它们都放在一个全局的管理类（Table）。当Model中持有了面板上的对象时，会出现对象销毁了，但Model中的变量不为空的情况。
            - Lua对象的function字段被赋值给了C#的事件/委托。
                - 比如UI控件的按钮点击事件。在LuaGC时，发现C#对象对其有引用，GC不掉。导致Lua中的对象通过Tolua引用住了C#对象，而C#对象又通过ToLua引用Lua对象。
                    - 对于每一个提供给Lua注册事件/委托的C#类，都继承一个IClear接口，该接口内实现清理事件/委托。
                    - 在MonoBehavior的OnDestroy函数内，调用IClear的接口。但要注意的是，这并不能保证所有的组件都是清理完毕，因为deactvie状态的组件，是不会触发OnDestroy的。因此需要手动的调用清理。
                    - 提供一个清理GameObject Lua事件/委托的接口，该接口会找到GameObject上所有继承于IClear接口的类，执行清理操作。需要手动清理的GameObject都需要调用该函数。
                    - 提供一个新的Destroy函数全局替换Unity原生的销毁GameObject接口。该函数在做真正销毁前，通过（3）清理所有注册的事件/委托。
    - 简述值类型与引用类型的区别
        - 值类型存储在内存栈中，引用类型数据存储在内存堆中，而内存单元中存放的是堆中存放的地址。
        - 值类型存取快，引用类型存取慢。
        - 值类型表示实际数据，引用类型表示指向存储在内存堆中的数据的指针和引用。
        - 栈的内存是自动释放的，堆内存是.NET中会由GC来自动释放。
        - 值类型继承自System.ValueType,引用类型继承自System.Object。
    - 请描述Interface与抽象类之间的不同
        - 抽象类中可以有字段，接口没有。
        - 抽象类中可以有实现成员，接口只能包含抽象成员。
        - 抽象类中所有成员修饰符都可以使用，接口中所有的成员都是对外的，所以不需要修饰符修饰。
        - 用法不同处：
            - 抽象类是概念的抽象，接口关注于行为。
            - 抽象类的子类与父类的关系是泛化关系，耦合度较高，而实现类和接口之间是实现的关系，耦合度较低。
            - 一个类只能继承一个类，但是可以实现多个接口。
    - 结构体和类有何区别
        - 结构体：是值类型，结构体对象分配在堆栈上而不是堆上。类：是引用类型，对象分配在堆上。
        - 结构体：不能从另外一个结构或者类继承，本身也不能被继承；类： 完全可扩展的，除非显示的声明sealed，否则类可以继承其他类和接口，自身也能被继承。
        - 结构体：不包含显式默认构造函数；没有析构函数，不能继承；不能有protected修饰符，可以不使用new初始化，在结构中初始化实例字段是错误的；类： 有默认的构造函数，有析构函数，可以使用abstract和sealed，有protected修饰符，必须使用new初始化。
    - 进程，Unity线程和协程之间关系
        - 进程：是运行一个程序所需要的基本资源。运行的应用程序在操作系统中被视为一个进程，进程可以包括一个或多个线程。
            - unity协程是利用C#的迭代器实现的一个机制，编译器会根据yield return，展开成一段类似状态机的代码。
            - StopCoroutine方法可以把协程停下来，物体被关闭了协程也会停。enbale = false 不会阻止协程的运行。
            - 协程代码卡的话，会把主线程卡住。同一时间只能执行某个协程，协程适合对某个任务进行分时处理。
            - 协程不是线程，也不是异步执行，跟Update一样，在主线程中执行；多线程是会开子线程的。
            - 子线程不能访问Unity的资源，比如 UI、GameObject、Debug.Log。
            - 协程(协同程序): 同一时间只能执行某个协程。开辟多个协程开销不大。协程适合对某任务进行分时处理。线程: 同一时间可以同时执行多个线程。开辟多条线程开销很大。线程适合多任务同时处理。
    - 性能优化
        - GC
            - 垃圾是怎么产生的？
            - 为什么要进行垃圾收集？
            - 垃圾收集放在什么时间点比较合适？
            - 如何成代码设计的角度避免垃圾的产生？
        - 内存管理
            - 在C#中分堆内存 栈内存，有什么区别？
            - 内存中还分成mono管理（托管堆）的内存、资源内存、代码内存管理。
            - drawcall 优化
            - 耗电问题怎么优化
    - Unity性能要点参考标准
        - CPU:主要看帧数，如果多数帧在预期中可忽略，对于重点卡帧的点进行优化
        - 内存：
            - Reserved Memory：总体内存，对于低端机总体内存尽量控制在150M以内
            - Reserved Mon Memory：堆内存，建议控制在50M以内； 对于堆内存较高的可以检测是都具有较大的Container、Array、List、Dictionary等容器存在，比如缓冲池，控制其开辟的大小
        - 资源加载：主要是场景切换时对象的创建和销毁，对于纹理可将其由RGBA32和RGBA16转换为ETC1进行加载即可以降低App和内存的大小又可以提高加载速度，进而提高切换场景的速度，场景中的网格数据可以进一步压缩减少不必要的数据占用
        - 用缓存池来进行缓存减少频繁使用对象的创建次数
        - 可以将Shader统一分离打包成AssetBundle，在游戏开始时加载进来，Shader体积较小，可常驻内存。
        - 纹理内存低端机建议控制在50M以内，推荐使用格式Android：ETC1，iOS：PVRTC；
        - 代码的CPU占用 Camera.Rende，MonoBehaviour.Update，Animator.Update & MeshSkinning.Update 建议关闭GPU Skinning
        - DrawCalls的优化，尽量做到分层合批，光效的粒子Order in layer的设置，材质相同的发射器使用相同的 Order in layer；避免材质打断，还有UI资源分块打包成图集
        - 如何优化内存
            - 将暂时不用的以后还需要使用的物体隐藏起来而不是直接Destroy掉
            - 释放AssetBundle占用的资源；AssetBundle资源包
            - 降低模型的片面数，降低模型的骨骼数量，降低贴图的大小
            - 使用光照贴图，使用多层次细节(LOD)，使用着色器(Shader)，使用预设(Prefab)
    - alpha blend 工作原理
        - 实际显示颜色 = 前景颜色*Alpha/255 + 背景颜色*(255-Alpha)/255
    - 光照计算中的diffuse的计算公式
        - 实际光照强度 I= 环境光(Iambient) + 漫反射光(Idiffuse) + 镜面高光(Ispecular)
        - 环境光：Iambient= Aintensity* Acolor;			(Aintensity环境光强度，Acolor环境光颜色)
        - 漫反射光：Idiffuse = Dintensity*Dcolor*N.L	 	(Dintensity漫反射强度，Dcolor漫反射光颜色，N法向量，L光源向量)
        - 镜面反射光：Ispecular = Sintensity*Scolor*(R.V)^n;	(Sintensity表示镜面光照强度，Scolor镜面光颜色，R为光的反射向量，V为观察者向量，n称为镜面光指数)
    - 描述MeshRender和SkinnedMeshRender的关系与不同
        - Mesh就是指模型的网格（同名组件是用于调整网格属性的）
        - MeshFilter：网格过滤器，MeshFilter决定使用哪一个Mesh。
        - MeshRender ：负责渲染 MeshFilter 指定的 Mesh，在 Transform 的位置渲染这个Mesh。
        - SkinnedMeshRenderer蒙皮网格过滤器，具有蒙皮信息（Skin数据）的 Mesh 就是SkinnedMesh。
        - 动画的网格用SkinnedMeshRenderer，静止不动的网格用MeshRender
    - MeshCollider和其他Collider的一个主要不同点
        - Meshcollider再快也是基于V3顶点，boxcollider本身是基于算法，没有面的概念
    - LOD是什么，优缺点是什么
        - LOD技术是Levels of Detail的简称，即多细节层次。LOD技术指根据物体模型的节点在显示环境中所处的位置和重要度，决定物体渲染的资源分配，降低非重要物体的面数和细节度，从而获得高效率的渲染运算。
            - 优点：可根据距离动态地选择渲染不同细节的模型;
            - 缺点：加重美工要准备不同细节的同一模型，同样的会增加游戏的容量
    - Unity AI 行为树
        - 是一棵用于控制 AI 决策行为的、包含了层级节点的树结构
**Composites（复合类）**：主要用于控制行为树的走向，也是用的最多最重要的一类，任何一个相对复杂的行为树都包含这类Task节点，但它本身不做任何具体行为，所以它们一般位于父节点或根节点。
            - Selector（选择）：相当于Or操作，下面的子Task节点只要有一个返回成功了它就返回成功，只有当所有的都返回失败了才返回失败。
            - Sequence（序列）：相当于And操作，下面的子Task节点只要有一个返回失败了它就返回失败，只有当所有的都返回成功了才返回成功。
            - Parallel Node（并发： 并发执行它的所有Child Node，指定数量的Child Node返回True或False后才决定结果。
        - **Decorators（装饰类）**：多用于对其下的子Task节点执行额外操作，例如反转结果，重复执行等。再返回给它的Parent Node
        - **Actions（行为类）**：数量最多，为具体执行行为的Task，一般位于行为树的叶子节点右侧，该类Task可能并非单帧就能完成。没必要每个Action都搞清楚，因为可以很容易的自己扩展Action。后面会具体介绍如何扩展。
        - **Conditionals（条件类）**：一般放在Action节点左侧进行约束，只有当条件满足（或不满足）时才继续往下执行，单帧内完成一次判断。更多时候配合复合节点进行打断或任务跳转，这一点后面会详细说明。

- 热更新
    - xlua 特点
        - xLua在功能、性能、易用性都有不少突破，这几方面分别最具代表性的是：
            - 可以运行时把C#实现（方法，操作符，属性，事件等等）替换成lua实现；
            - 出色的GC优化，自定义struct，枚举在Lua和C#间传递无C# gc alloc；
            - 编辑器下无需生成代码，开发更轻量；
            - 对C#的实现进行热更新。
                - 原理：需要更新的类打上[HotFix]标签后，执行XLua/Generate Code后，xLua会根据内置的模板代码生成器在XLua目录下生成一个DelegateBridge类，这个类中的__Gen_Delegate_Imp*函数会映射到xlua.hotfix中的function。在执行XLua/Hotfix inject in Editor后，xLua会使用Mono.Cecil库对当前工程下的Assembly-CSharp.dll程序集进行IL注入。IL是.NET平台上的C#、F#等高级语言编译后产生的中间代码，该中间代码IL再经.NET平台中的CLR（类似于JVM）编译成机器码让CPU执行相关指令。移动平台无法把C#代码编译成IL中间代码，热更新方案都会涉及到IL注入，Unity内置的VM才能对热更新的代码进行处理。

    - unity 热更新的底层原理？ Scripting Backend 脚本引擎后台是什么？
        参考视频讲解：https://www.bilibili.com/video/BV1Ra411c7Gz
        - 打包选项，有两种
            - Mono
                - ARMv7、x86
                - 只支持32位系统，内存最大4G, 2^32 = 4G
                - 打包后apk修改后缀为.zip，查看项目目录结构，是.dll（动态链接库,此为项目脚本文件）后缀的文件 + mono虚拟机文件(limonoXXX.so文件),.dll文件必须加载到mono虚拟机文件中执行。
            - IL2CPP
                - ARMv7、ARM64、x86
                - 支持32位、64位系统
                - 打包后无DLL文件，libil2cpp.so = libmono.so + Assembly-CSharp.dll
                - 可以显著减少构建的游戏包体大小
                - 必须提前编译
            - android 支持 Mono 和IL2CPP
            - ios 仅支持IL2CPP，此外大部分平台仅支持IL2CPP
            > ios在早期是支持Mono的Full-AOT方式，仅支持32位，但是在2016年1月平台要求所有新上架的游戏必须支持64位架构，所以必须要选IL2CPP。
        - Mono方式脚本编译流程
            - 脚本被编译成IL
                - C#code 通过Mono C# Compiler编译生成CIL(中间汇编语言，不同平台的CIL可能不一样)，在游戏运行时候，IL和项目里其他的第三方兼容的DLL一起放入Mono VM虚拟机，由虚拟机解析成机器码，并且执行。
                - 等到需要真正执行的时候，这些IL会被加载到运行时库，也就是VM中，由VM动态的编译程汇编代码（JIT）再执行
            - CLI = CIL + CLR
            ![编译流程图](http://csoap.github.io/img/in-post/post-js-version/mono-complier.png "编译流程图")
                - 编译：通过C#编译器，运行前把C#编译成CIL（实现平台无关汇编）
                - 运行：通过CLR，在运行时把CIL转换成各平台的原生码
                >  CLI: Common Language Infrastructure 公共语言基础结构 | CIL: Common Intermediate Language 公共中间语言 | CLR: Common Language Runtime
            - CRL 通用语言平台，是微软的.Net虚拟机
                - 抽象了平台相关部分，因为windows、linux、android等操作系统的以下行为是不一样的（如进程线程管理、内存分配、垃圾回收、文件管理等），并在运行时调用平台相关的实现。
            - Mono
                - 一个基于CLR的开源项目，运行引擎和用户的托管代码运行在每一个目标平台上
                - 微软的传统虚拟机叫做.NET平台虚拟机，本身是不支持跨平台的，经过mono的移植，才可以支持跨平台。unity也是利用mono这个开源项目实现跨平台
            - Mono虚拟机如何运行CIL？
                - JIT（Just In Time）模式。在编译的时候，把C#编译成CIL，在运行时，逐条读入，逐条解析成原生码交给CPU再执行
                - AOT（Ahead Of Time）模式。在编译程CIL之后，会把CIL再处理一边，编译成原生码，在运行的时候交给CPU直接执行，Mono下的AOT只会处理部分的CIL，还有一部分CIL采用JIT的模式
                - Full AOT。在编程CIL之后，把所有的CIL编译成原生码，在运行时的时候直接执行
                    - 不允许运行时动态加载代码了，也就是说**不支持热更**，如ios只支持Full AOT,andorid支持三种。
                    - 安全性比较好
        - IL2CPP方式脚本编译流程
            ![IL2CPP编译流程图](http://csoap.github.io/img/in-post/post-js-version/il2cpp-complier.png "IL2CPP编译流程图")
            - C#code 通过Mono C# Compiler编译生成CIL，通过IL2CPP将CIL重新编程C++代码，然后再由各个平台的C++编译器（例:x86：vc++,linux、android:gcc）直接编译程能执行的原生汇编代码,然后通过IL2CPP VM虚拟机生成机器码
            - 仅支持AOT方式
        - 理想的热更流程
            - 热更的功能写在一个DLL中
            - 游戏启动时用新的同名DLL覆盖旧的
            - Assembly.Load动态加载该DLL
            - 反射创建热更DLL中的类的实例或静态方法
            - 这么简单？理论上android支持jit，所以是可以的。但是ios不支持，因为ios禁止为动态分配内存赋予执行权限
        ```C++
        void* create_space(size_t size) {
            void* ptr = mmap(0, size, PROT_READ | PROT_WRITE | PROT_EXEC，
                    MAP_PRIVATE | MAP_ANON, -1, 0);
            return ptr;
        }
        // 本段C++代码相当于创建一块内存，并未内存赋予权限，在windows、android上是正常的。
        //在ios平台上会报错，ios禁止为动态创建的内存赋予执行权限，PROT_EXEC是禁止的。
        ```
        - 所以只能采用静态编译
            - Mono的Full-AOT
            - IL2CPP
            - 但也不能完全避免问题。在IOS平台上运行的程序,如果在运行的时候才知道泛型的实际类型，用上述2种编译会直接跳过这段代码的编译，编译器会认为说我既然是静态编译的，就不能执行在程序运行中动态指令的代码，会导致报错，如下：

            ![ios代码](http://csoap.github.io/img/in-post/post-js-version/complier-ios.png "ios代码")
            - 怎么解决？
                - 比较不好的解决方法。强制AOT生成具象类型代码，这样就丧失的泛型的灵活性。如：OnMessage(AnyEnum.Zero);需要思考有没有更好的办法？思考kow项目中tolua 是怎么解决的
        - 解决方案
            - 所以要嵌入脚本语言
                - lua
                    - ToLua /XLua，ios和android都是用lua脚本热更，基于lua虚拟机
                - C#
                    - ILRuntime,未来更有前景，需要了解一下，毕竟unity官方支持。。
                        - ios用c#脚本热更（运行效率跟lua差不多），android直接用C#反射热更
            - LUA/ILRuntime热更方案都会把脚本加载到内存并执行，那么这两种方式就能正常执行动态加载的脚本呢？
                - 拿lua来举例，脚本语言工作原理：每次启动游戏的时候首先从服务器检测脚本更新，如果有更新就下载到客户端，客户端启动后，脚本会加载到内存中运行，由lua虚拟机负责解释执行脚本
                - lua热更比C#热更缺点：
                    - 学习成本提高
                    - 招聘成本提高
                    - 引擎层到脚本层的转换会降低程序执行效率。增加lua虚拟机来执行lua脚本，增加了一层unity的C#到lua脚本的开销。
        - 小结与思考
            - unity的脚本后台有哪几种
            - 每种脚本后台分别支持哪几种编译方式
            - 安卓/苹果分别可以选择哪几种编译方式
            - C#脚本对反射的使用有限制，那么什么样反射方法可以使用呢？ 反射又是啥？
            - .net项目和netcore项目之前的区别
            - 热更新的底层原理
            - 热更新GC问题
            - 热更新的lua代码的对象绑定问题
        - 进一步学习内容
            - 资源热更新框架
                - 如何打包资源
                - 如何上传资源
                - 如何进行版本比对
                - 如何环节资源更新服务器的压力
            - 脚本热更新框架
            - lua脚本语言的学习？lua面向对象
            - ToLua/ILRuntime热更新方案及其背后各项技术的原理
                - 如何实现绑定
                - 如何实现反射
                - 如何实现重定向

- lua
    - lua代码的性能瓶颈定位与优化
        - CPU
            - 找到瓶颈函数，一旦找到瓶颈函数，接下去的优化过程无非就是优化方案，无非就是修改实现，或者通过策略减少瓶颈函数的调用。
            - 思考？通过什么手段快速找到瓶颈函数
        - 内存
            - 总内存,lua本身有函数可以直接查询当前的总内存占用。监控总内存主要是为检查内存泄漏。内存泄露一般出现在与另外一个语言交互过程导致的。操作过程：查看总内存曲线，luagc能把总内存降低在一个正常的水平说明没问题。至于内存占用多少，具体看游戏项目类型了。十几M到一百多M都有~
            > collectgarbage("count"): 以 K 字节数为单位返回 Lua 使用的总内存数。 这个值有小数部分，所以只需要乘上 1024 就能得到 Lua 使用的准确字节数（除非溢出）
            - 临时分配->Lua GC
                - lua中做大量临时分配容易造成lua频繁GC，虽然lua在5.3之后是分布执行GC。也就是说触发Lua GC，lua不会在一帧内强制把所有的垃圾回收都执行一遍，而是在持续一段时间内，每帧去做一定的GC工作，相对来说比较不会造成明显的卡顿，但是会造成CPU上的一些耗时。
                - 如何减少临时分配
                    - 尽可能复用一些table。
                    - 思考还有什么。。
        - 工具：
            - https://www.uwa4d.com/#download UWAGOT
            - https://github.com/leinlin/Miku-LuaProfiler lua profiler

- 图形学
    - unity shader
        - 什么是Shader？
            - Shader就是着色器,就是专门用来渲染图形的一种技术，通过shader，我们可以自定义显卡渲染画面的算法，使画面达到我们想要的效果。
        - Shader分为两类
            - 顶点Shader(Vertex Shader): 3D图形都是由一个个三角面片组成的，顶点Shader就是计算每个三角面片上的顶点，并为最终像素渲染做准备;
            - 像素Shader(Fragment Shader):以像素为单位，计算光照、颜色的一系列算法;
        - Unity Shader
            - Shader编程语言有GLSL、HLSL、Cg等；Unity Shader是Unity自身封装后的一种便于书写的Shader，又称为ShaderLab；主要写法Surface Shaders 表面着色器，Vertex/Fragment Shaders 顶点/片断着色器。
        - Shader与材质的关系
            - 一个Shader可以与无数个材质关联。一个材质同一时刻只能关联于一个Shader。
            - 材质可以赋与模型，但是Shader不行。
            - 材质就像是Shader的实例，每个材质都可以参数不一样呈现不同的效果，但是当Shader改变时，关联它的所有材质都会相应的改变。
        - Shaders的结构

            ```
            Shader "name" { //Shader名称路径；
            [Properties] {}	  //材质球面板中显示的贴图和一些参数什么的都是在此Properties中进行定义设置的；
            SubShaders  //shader的主体，Pass的意思就是渲染一次模型，由CGPROGRAM开始，由ENDCG结束；
            {
            Pass { 
            CGPROGRAM 
            #pragma vertex vert 
            #pragma fragment frag 
            #pragma surface surf Standard fullforwardshadows
            fixed4 _Color; 
            float4 vert ( float4 vertex : POSITION ) : SV_POSITION	 //顶点
            { 
                return UnityObjectToClipPos(vertex); 
            } 
                fixed4 frag () : SV_Target 		//片段
            { 
                return _Color;
            } 
            void surf (Input IN, inout SurfaceOutputStandard o) //表面着色器
            {
            }
            ENDCG
            }
            }
            [FallBack] 	  //备胎shader的路径，遇到不支持的时候来处理的；
            [CustomEditor]  //自由定义材质面板的显示结果，它可以改写Properties中定义的显示方式；
            ```


    - 什么是渲染管道
        - 渲染管线也称为渲染流水线，是我们准备一些数据，让GPU对这些数据做一些处理，最后得出一张二维图像，渲染流程主要分为几个大的阶段：
            - 数据准备阶段：根据用户提供的顶点及索引信息，构建多边形，数据包括顶点数据（位置，法线，颜色，纹理坐标）和常量（世界矩阵，观察矩阵，投影矩阵，纹理因子等）;
            - 顶点处理阶段：是通过一系列坐标系的变换，让各个顶点通过一定的规律在摄像机前位移，最终在屏幕上对应这些顶点的过程;
            - 光栅操作阶段：合并阶段。它的主要功能是将面转换成一帧中的像素集合;
            - 像素着色阶段：将像素区域着色，然后赋予贴图;
    - 前向渲染和延迟渲染的区别
        - 前向渲染和延迟渲染是两种光照渲染模式。
            - 前向渲染：先渲染一遍物体，把法线和高光存在ARGB32的渲染纹理中（法线用rgb通道，高光用a通道），存在了z buffer里；然后通过深度信息，法线和高光信息计算光照（屏幕空间），光照信息缓存在Render Texture中；最后混合。如果是逐像素的，复杂度：片段的个数*光照的个数。
            - 延迟渲染：先不进行光照运算，对每个像素生成一组数据(G-buffer)，包括位置，法线，高光等，然后用这些数据将每个光源以2D后处理的方式施加在最后图像上（屏幕空间）。复杂度：屏幕的分辨率*光源个数。
    - OpenGL中要用到哪几种Buffer？
        - 帧缓冲(Frame Buffer)：用于写入颜色值的颜色缓冲、用于写入深度信息的深度缓冲和允许我们根据一些条件丢弃特定片段的模板缓冲，这些缓冲结合起来叫做帧缓冲(Framebuffer)，它被储存在内存；
        - 模板缓冲(Stencil Buffer)：与深度缓冲大小相同,通过设置模版缓冲每个像素的值,我们可以指定在渲染的时候只渲染某些像素,从而可以达到一些特殊的效果；
        - 顶点缓冲(Vertice Buffer)：直接将顶点数据存储在gpu的一段缓冲区，不需要从cpu拷贝到gpu。提高了程序的运行效率；
        - 深度缓冲(Depth Buffer) ：与帧缓冲区对应,用于记录上面每个像素的深度值,通过深度缓冲区,我们可以进行深度测试,从而确定像素的遮挡关系,保证渲染正确；
        - 渲染缓冲(render Buffer)：可用于分配和存储颜色，深度或模板值，并可用作帧缓冲区对象中的颜色，深度或模板附件；



