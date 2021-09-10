---
layout:     post
title:      "Unity的性能优化"
subtitle:   "性能优化的目标以及原理"
date:       2019-11-29
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 性能优化
---
- 优化主要是对CPU、GPU、内存的优化。
- Canvas 优化要点
    - 一个canvas下的所有UI原色都是合在一个Mesh中，过大的Mesh在更新时开销很大
    - 一般建议每个较复杂的UI界面，都拆成一个Canvas（可以是子canvas），在UI界面复杂时，甚至要划分更多的子Canvas
    - 动静分离
    - canvas又不能细分的太多，因为会导致DrawCall的上升。每一次canvas的绘制都会对显卡造成一次drawcall，显卡流水线的切换，造成性能开销。一个场景最多也就十几个canvas，不能再多了。。
    - 背景大图不要和小图放在一个图集里
- Overdraw（GPU）
    - 遮挡剔除
    - 造成GPU性能瓶颈的主要原因：
        - 复杂的vertext shader 或 pixel shader计算 顶点或像素。
        - overdraw
            - 光栅化阶段的填充元素过多？
            - 在UGUI中使用alpha=0的不可见Image参与raycast，比如在屏幕空白处点击的相应，然后这些元素虽然在屏幕色不可见，但依然参与绘制，会造成overdraw。如引导
            - 半透明的UI元素
    - 解决方案距离：
        - 禁用不可见的UI
            - 比如当打开一个系统时如果完全挡住了另外一个系统，则可以将被遮挡住的系统禁用。（如何做？需要思考，如何禁用，涉及到ui排序+ui层级）
- 不处理点击的物体不要挂raycaster
- 少用LayoutGroup或者ContentSizeFitter,因为这个是应用UGUI的所有情况的网格排布计算。
    - 用户操作->重绘->消耗大量计算时间，建议自己写一套算法（怎么算？需要思考）
    -layout原理：操作的所有地方都设置一个脏标志，遍历SetDirty对象会消耗性能

- 模型优化有什么好办法？
    - static Batching 多个相同模型静态合批，空间换时间 Mesh.CombineMesh()
        - 在线合批：程序运行中实时的合并模型，如果场景中模型数量、位置不固定，则必须在线合并
        - 离线合批：程序启动之前利用Unity的合批功能，提前把模型合并好
    - Dynamic Batching 动态合批
        - 骨骼蒙皮
            - CPU Skinnig
                - 利用unity官方的animationInstancing插件，原理：利用模型的贴图全部合并到一张纹理贴图上面
            - GPU Skinning，比较适合用来优化的是植被系统，有一个限制，GPU Skinng不支持带有骨骼动画的mesh（SkinnedMeshRender），。why？原因，因为我们带有骨骼动画的mesh他们在最终通过pose的施加，它的最终顶点数据是不一样的。即使是同一个动画，因为处在不同的动画的帧数中，我们无法通过一次提交把这些物体多次画出来。具体内容：https://www.cnblogs.com/murongxiaopifu/p/7250772.html
            https://www.sohu.com/a/150788160_466876
    - LOD(level of detail),越远三角面越少，需要本身模型导入就自带lod才支持
        - 优点：靠的远的时候三角面少
        - 缺点：加载到显卡显存的时候需要同时将多个级别的显存数据都保存到显卡现存中，典型空间换时间
        - animator LOD ？ 动画也支持,有空需要了解一下
    - 地形 ? 不懂：
        - Terrain->Mesh
        - MMO分区域地图
        - 动态加载、卸载
        - 接缝处理
    - 纹理贴图？ 不懂，这个需要去看下
        - Mipmap
        - 三线性插值
        - 注意半透明优化

- 脚本优化
    - Transfor.SetPositionAndRotation
        - 每次调用Transform.SetPosition或Transform.SetRotation时，Unity都会通知一遍所有的子节点。
        - 当位置和角度信息都可以预先知道时，我们可以通过Transform.SetPositionAndRotation一次调用来同时设置位置和角度，从而避免两次调用导致的性能开销。
    - Animator.Set..
        - m_animator.SetTrigger(“Attack”)是用来触发攻击动画。然而在这个函数内部，“Attack”字符串会被hash成一个整数。如果我们需要频繁触发攻击动画，我们可以通过Animator.StringToHash来提前进行hash，来避免每次的hash运算

        ```
        private static readonly int s_Attack = Animator.StringToHast("Attack");
        m_animator.SetTrigger(s_Attack);
        ```

    - Material.Set...
        - 与Animator类似，Material也提供了一系列的设置方法用于改变Shader。例如：m_mat.SetFloat(“Hue”, 0.5f)是用来设置材质的名为Hue的浮点数。同样的我们可以通过Shader.PropertyToID来提前进行hash

        ```
        private static readonly int s_Hue = Shader.PropertyToID("HUE");
        m_mat.SetFloat(s_Hue, 0.5f);
        ```
    - Coroutine
        - 当需要实现一些定时操作时，有些同学可能会在Update中每帧进行一次判断，假设帧率是60帧，需要定时1秒调用一次，则会导致59次无效的Update调用。
        - 用Coroutine则可以避免掉这些无效的调用，只需要yield return new WaitForSeconds(1f);即可。当然这里的最佳实践还是用一个变量缓存一下new WaitForSeconds(1f)，这样省去了每次都new的开销。

- IL2CPP
    - I2LCPP是Unity提供的将C#的IL码转换为C++代码的服务，由于转成了C++，所以其最后会转换成汇编语言，直接以机器语言的方式执行，而不需要跑在.NET虚拟机上，所以提高了性能。同时由于IL的反编译较为简单，转换成C++后，也会增加一定的反汇编难度。

- 与C#交互
    - 关于与C#的交互，不同的Lua解决方案有不同的策略，但是有些基本的点都是一样的。
    - 关于MonoBehaviour的三大Update的桥接，最佳策略是通过一个管理器继承MonoBehaviour的Update，然后将其派发给Lua端，然后Lua端所有的Update都注册于这个管理器当中。这样可以避免了多次Lua与C#的桥接交互，可以大量节省时间。
    - 需要考虑GC问题，默认的struct比如Vector3传递到Lua中都需要经历一次装箱操作，会带来额外的GC Alloc，可以采用特殊配置的方式将其避免。参考：https://github.com/Tencent/xLua/blob/master/Assets/XLua/Doc/XLua%E5%A4%8D%E6%9D%82%E5%80%BC%E7%B1%BB%E5%9E%8B%EF%BC%88struct%EF%BC%89gc%E4%BC%98%E5%8C%96%E6%8C%87%E5%8D%97.md
    - 优化思路：https://blog.uwa4d.com/archives/USparkle_Lua.html


- 其他优化
    - 定点GC
    - 关闭不用的碰撞矩阵？这是啥
    - 尽量异步加载，防止阻塞
