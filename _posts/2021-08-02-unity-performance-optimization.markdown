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

- 其他优化
    - 定点GC
    - 关闭不用的碰撞矩阵？这是啥
    - 尽量异步加载，防止阻塞
