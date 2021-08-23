---
layout:     post
title:      "面经"
subtitle:   "面经总结"
date:       2021-08-13
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 面试
---
- 网络
    - TCP可靠协议
    - 拥塞控制
    - TCP滑动窗口
    - TCP流式传输
- 算法
    - 第K大的数
    - 优先队列

- unity
    - Unity 与Mono和.Net的关系
        - .Net拥有跨语言，跨平台性。Unity引擎需求也是需要跨平台，支持多语言（C#，Js，Boo）。就参考微软开发.Net Core的概念，于是，推出了Mono.
        - Unity之所以能跨平台是因为有通用中间语言（CIL），CIL是一种可读性比较低的面向对象的语言。它是基于堆栈的，与具体CPU中的寄存器无关。CIL运行在Mono运行时（虚拟机）上，其实它能运行在任何支持CLI(通用语言基础结构)的平台上，包括.NET与Mono，所以能实现跨平台。即 虽然IOS系统不能运行.exe文件，但是mono虚拟机可以，所以可以跨平台。
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



