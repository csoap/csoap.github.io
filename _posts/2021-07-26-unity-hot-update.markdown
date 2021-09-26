---
layout:     post
title:      "unity游戏热更新原理笔记"
subtitle:   "unity游戏热更新"
date:       2019-11-29
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 热更新
---

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
        ![编译流程图](/img/in-post/post-js-version/mono-complier.png "编译流程图")
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
        ![IL2CPP编译流程图](/img/in-post/post-js-version/il2cpp-complier.png "IL2CPP编译流程图")
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

        ![ios代码](/img/in-post/post-js-version/complier-ios.png "ios代码")
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
            - 使用lua热更新就是在Unity环境里内嵌一个lua虚拟机，经常变动的和对执行效率没要求的逻辑用Lua实现，游戏启动时加载服务器上最新的lua文件，并加载到内存，由lua虚拟机负责解释执行脚本。lua代码都是运行时才编译的，不运行的时候就如同一张图片、一段音频一样，都是文件资源；所以更新逻辑只需要更新脚本，不需要再编译，因而lua能轻松实现“热更新”。
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

- ToLua
    - Tolua是Unity静态绑定lua的一个解决方案，它通过C#中集成lua的插件，可以自动生成用于在lua中访问Unity的绑定代码，并把C#中的常量、变量、函数、属性、类以及枚举暴露给lua。其从cstolua衍变而来。
    - wrap文件是如何生成，为什么要生成wrap？
    -  lua是怎么获取、调用c#的静态方法、成员方法？c#对象在lua栈里是以什么形式存在的？
    - c#如何调用到lua的方法的？tolua是怎么把lua的table、function转成c#的table、function实例的？
    - tolua把对象存在objects里，而值类型的Struct如果存在objects了，会发生封箱、拆箱的操作，tolua是如何避免的？
    - objects里的对象是什么时候会被移除？lua怎样才算正确释放了c#对象？
    - 利用tolua如何实现热更？
    - 针对lua和c#的交互有什么优化手段？