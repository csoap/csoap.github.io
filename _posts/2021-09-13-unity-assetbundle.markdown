---
layout:     post
title:      "unity的assetbundle归纳"
subtitle:   ""
date:       2021-09-13
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - assetbundle
---

![ab内存引用](/img/in-post/post-js-version/ab_1.png "ab内存引用")

- 注：CreateFromFile 已改为LoadFromFile

- 压缩格式
    - LZMA格式。默认格式 BuildAssetBundleOptions.None
        - LZMA是一种序列化流文件，因此在默认情况下，打出的AssetBundle包处于LZMA格式的压缩状态，在使用AssetBundle前需要需要**全部**被解压。
        - 使用LZMA格式压缩的AssetBundle的包体积最小（高压缩比），但是相应的会增加解压缩时的时间。
        - 比较蛋疼的是，一旦这个bundle被解压之后，在磁盘上又会以LZ4的格式重新压缩，LZ4的压缩格式，在使用资源的时候不需要全部解压
    - LZ4格式 BuildAssetBundleOptions.ChunkBasedCompression
        - kow 使用这个选项
        - 算法基于chunk。缺点：压缩比一般，因此经过压缩后的AssetBundle包体的体积较大
        - 优点：LZ4格式的好处在于解压缩的时间相对要短。运行文件以chunk或者piece的方式加载，只解压一个chunk文件，而无需解压bundle中其余不相关的chunk。
    - 不压缩 BuildAssetBundleOptions.UncompressedAssetBundle
        - 无压缩的打包，加载的文件更大，但是时间更快(省去解压的时间)
    - kow策略
        - ab包压缩策略，安卓,iOS使用lz4 +gzip
- 分包策略
    - 对包的大小和数量进行一个平衡。所有资源打成一个包，一个资源打一个包，都是比较极端的做法
    - 打成一个包的缺点
        - 加载了这个包，我们不需要的东西也会被加载进来，占用额外内存，而且不利于热更新
    - 打成多个包的全二点
        - 容易造成冗余，首先影响包的读取速度，然后包之间的内容可能会有重复，且太多的包不利于资源管理哪些模块打成一个包
    - 例如游戏中每个怪物都需要打成一个包，因为每个怪物之间是独立的
    - 游戏的基础UI，可以打成一个包，因为他们在各个界面都会出现
- assetbundle加载
    - new www
        - 优点:即可网络加载又可加载本地
        - 缺点：缺点是这种方式加载资源，所占的内存要比LoadFromFile高，相比LoadFromFile加载速度也会要慢一些。
    - AssetBundle.LoadFromFiles
        - 目前版本最推荐的加载本地AssetBundle的方式，从性能上目前是最好的，内存占用相比new www也要小
    - LoadFromFile 与LoadFromFileAsync对比
        - LoadFromFile
            - 同步加载，return 直接返回AssetBundle对象
        - LoadFromFileAsync
            - 异步加载，return 返回AssetBundleCreateRequest对象
        - 同步加载，直接就返回了资源的AssetBundle；异步加载，则返回了异步加载的追踪对象AssetBundleCreateRequest。追踪对象，用于之后进行资源异步加载情况跟踪，被协程轮询判断是否已经异步加载完毕，若完成了可从追踪对象里获取加载资源。

    - 综上所述，推荐大家使用www来下载由多个AB包压缩生成的压缩包，下载然后解压到本地后再通过LoadFromFile来加载，这样的做法又快又高效，资源还小
- assets加载
    - 用AssetBundle.Load(同Resources.Load) 这才会从AssetBundle的内存镜像里读取并创建一个Asset对象，创建Asset对象同时也会分配相应内存用于存放(反序列化)。如果多次Load同名对象，除第一次外都只会返回已经生成的Asset对象，也就是说多次Load一个Asset并不会生成多个副本（singleton）
    - 异步读取用AssetBundle.LoadAsync
    - 一次读取多个用AssetBundle.LoadAll，加载完后立即AssetBundle.Unload(false),释放AssetBundle文件本身的内存镜像，但不销毁加载的Asset对象。（这样你不用保存AssetBundle的引用并且可以立即释放一部分内存
    - Instantiate（object)：Clone一个object的完整结构，包括其所有Component和子物体（详见官方文档）,浅Copy，并不复制所有引用类型。
- 卸载
    - AssetBundle.Unload(flase)是释放AssetBundle文件的内存镜像，不包含Load创建的Asset内存对象。
    - AssetBundle.Unload(true)是释放那个AssetBundle文件内存镜像和并销毁所有用Load创建的Asset内存对象。
    - 从assetBundle里Load出来 里面可能包括：Gameobject transform mesh texture material shader script和各种其他Assets
    - Instantiate一个Prefab，是一个对Assets进行**Clone(复制)+引用**结合的过程，GameObject transform 是Clone是新生成的。其他mesh / texture / material / shader 等，这其中有些是纯引用的关系的
    - 引用的Asset对象不会被复制，只是一个简单的**指针指向已经Load的Asset对象**。所以你Load出来的Assets其实就是个数据源，用于生成新对象或者被引用
    - 当你Destroy一个实例时，只是释放那些Clone对象，并不会释放引用对象和Clone的数据源对象，Destroy并不知道是否还有别的object在引用那些对象
    - 等到没有任何游戏场景物体在用这些Assets以后，这些assets就成了没有引用的游离数据块了，是UnusedAssets了，这时候就可以通过**Resources.UnloadUnusedAssets**来释放
    - Reources.UnloadAsset(Object):显式的释放已加载的Asset对象，只能卸载磁盘文件加载的Asset对象
    - Unity系统在加载新场景时，所有的内存对象都会被自动销毁，包括你用AssetBundle.Load加载的对象和Instaniate克隆的。但是不包括AssetBundle文件自身的内存镜像，那个必须要用Unload来释放，用.net的术语，这种数据缓存是非托管的。既然加载场景不会释放AssetBundle文件自身的内存镜像，那我们就手动释放。AssetBundle.Unload(true)

- 注意事项
    - 想打包进AssetBundle中的二进制文件，文件名的后缀必须为“.bytes”

- 差量更新
    - https://zhuanlan.zhihu.com/p/31810166
    - 生成差分补丁包
        - 是什么?
            - 比较两个版本的文件的二进制数据然后生成一个patch文件，将这个patch打到旧文件就可以产生新文件，也就是我们常见的更新补丁包
        - 怎么做?
            - 我这里用的是BsDiff的C#版：LogosBible/bsdiff.net 或者C版的mendsley/bsdiff
            - 这个东西操作其实并不复杂，就是保持上个版本的全部文件，然后和新版本的文件做一次对应的Diff，然后把Diff和新版本文件一起上传至CDN。

            - 如果有玩家跨了多个版本没有更新，就下载多个版本的Diff然后按顺序依次打包。如果版本太多可能会导致总补丁包比下载新包更大，Patch的时间可能也过长，可以在中间插入几个大版本之间的Diff，减少补丁包的数量
            - BsDiff的主要缺点是更新包大小有浪费，和Patch的耗时
        - 缺点
            - 毕竟只是二进制对比还要考虑效率，生成的文件大小还是会比打小包要大一些的（当然这也要看小包到底有多小）