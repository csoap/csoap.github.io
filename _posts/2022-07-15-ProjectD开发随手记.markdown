---
layout:     post
title:      "ProjectD开发随手记"
subtitle:   ""
date:       2022-07-15
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - slg
---
- 采集逻辑
    - 初始化采集
        - 初始化地块占有情况，客户端显示资源点和采集情况（是否采集中、拥有者信息）
    - 发起采集
        - 条件：1.选择部队胜利 2.存在空闲农民 3.临街地块
        - 表现：客户端选择部队行军到资源点，胜利：农民-1，开启占领逻辑
        - 结果：成功：空闲农民-1
    - 占领阶段：资源进度变化,服务端同步占领成功，客户端做采集表现
    - 取消采集，农民表现撤退
    - 资源同步
        - 服务端同步，客户端面板数量更新
    - 资源被打
        - 服务端同步，农民撤退，面板农民 + 1

- 内城城建
    - 内城建筑状态（生成建筑位置，控制建筑状态刷新，建筑主界面）
        - 数据格式：美术资源、有哪些建筑
        - 
    - aoi维护建筑数据
        - 他人实时同步？

- 大地图扩张
    - 资源是否显示地块逻辑

- aoi刷新优化
    - 测试环境：PC编辑器-luaProfiler
    - aoi变化刷新耗时过高（从高到低），耗时 300ms左右
        - 计算视野 VAoiSystem:_updateVision() ，127ms，初步定位原因：1.全量计算 2.静态地块重复计算 3. 分帧
            - 99.71ms 为静态地块计算耗时 MapVisionSystem:addVisionByBlock()
            - 21.32ms 为VWarFogSystem:updateAoi()
        - 模板获取 VAoiSystem:_updateMapCellByTemplate(),62.29ms，slgMap:getHexSpiralRingBlocks() 会频繁创建对象
            - 陈鑫解决创建对象问题
            - 新增cache
        - 更新静态物体（创建、删除 植被、建筑、资源点），VAoiSystem:_updateStatic(),44.91ms
            -  VAoiSystem:_updateStatic() 27ms
                - slgMap:getResIdByIndex() new lua table , 思考避免每次获取都创建表 cache？
                - VAoiSystem:handleResPointState() + MapEntityFactory:createResPoint()，处理：客户端剔除
        - 刷新地块、海岸线 VAoiSystem:_handleBlocks()， 28ms
            - VAoiSystem:_handleEdges(edgeInfos) 边界线 10.9ms
            - MapEntityFactory:createBlock 地块 6.12ms
    - 优化之后 总量100ms~115ms之间
- 大地图拆分优化
    - 测试环境：pc环境下 luaProfiler
    - 拆分前
        - lua内存：36MB ，写入整个地图数据 300MB =》227MB
    - 拆分后
        - lua内存：38MB, 写入区域地图数据 60MB
- lua 堆内存分配
    - uwa ,slgMap:getBlockListSpiralRing() 计算模板区域地块，创建大量对象， 144.22 MB

- uwa 性能报告
    - 卡顿分析 10.10 - 10.21
        - vectorpool 模块开发，并修改已有代码，除了部队个别引用vector复杂的代码没修改，减少大量 堆内存分配
        - 打开 出征界面 ArmySetOffPanel， 耗时85ms
        - 发起出征，部队从无到有，部队移动
            - createHud：耗时85ms
            - 2个部队移动，视野更新触发迷雾计算耗时44ms，可以延迟刷新迷雾，已优化
        - 动态视野变化导致动态视野全计算，修改为增量计算, 已优化（占比消耗大头）
        - slgMap 等getCellByIndex() 会new table，如果只需要获取resid，修改成 =》grid:getResIdByIndex(index)

        - 迷雾、视野 代码效率优化 已优化
        - processHit 点击选中部队，耗时79ms，其中showArmyOpHud()耗时33ms
        - 打开部队配置界面ArmyEditPanel，耗时100ms，思考：主界面可以打开的一级界面是否可以加入preloadPanel列表里面
        - 某一帧单次_onGotBlockData() =>_updateAoi() 耗时228

        - aoi变更 服务端通知大量信息，延时刷新，因为服务端通知不可控，如果一通知就刷新 不合理，请求了多次 enter_map_aoi, 耗时626ms，刷新了15次aoi_update
            - 修改成：
                - 如果是第一次进入，不延时
                - 其他刷新状态都要延时极短时间
                - aoi中心变更定时器移除，改为屏幕移动时候判断
    - xlua c# 优化

    - ECS
        - 功能逻辑为核心，对对象的数据进行切片和组合的设计框架，被称作ECS(Entity+Component+System)框架
    - 撒点
        - 哈尔顿稀疏算法
    - 编辑器
        - cpu OnGUI绘制格子，改为GPU Instancing
    - 异步记载
        - https://zhuanlan.zhihu.com/p/561534866
        - 优先队列
    - 预计算遮挡剔除应用
        - https://zhuanlan.zhihu.com/p/573720220
    - 寻路
        - 单单位
        - 多单位
    - 批处理
        - 早期unity只支持动态、静态批处理，后来又支持了GPU Instancing，最后SRP推出SRP Batcher
        - 静态批处理
        - 动态批处理
            - 原理
                - 每一帧把可以进行批处理的模型网格进行合并，再把合并好的数据传给CPU，然后使用同一个材质进行渲染。
            - 好处
                - 经过批处理的模型仍然可以移动
            - 缺点
                - unity每帧都会重新合并一次网格
                - 在逐对象的材质属性时会失效，网格顶点属性规模要小于900等，该技术适用于共享材质的小型的网格

        - GPU Instancing
            - 介绍
                - 将数据一次性发给GPU，再使用一个绘制函数让渲染流水线利用这些数据绘制多个相同的物体（相同网格）提升性能
                - CPU收集每个物体的材质属性和变换，放入数组发送到GPU，GPU遍历数组按顺序进行渲染。
            - 举例
                - 假设需要渲染100个相同的模型，每个模型有256个三角形，那么需要两个缓冲区，一个用来描述模型的定点信息，因为待渲染的模型是i相同的，所以这个缓冲区只存储了256个三角形；另一个缓冲区就是用来描述模型在世界坐标下的位置信息。假如不考虑旋转和缩放，100个模型及占用100个float3类型的存储空间
            - 多个相同的物体合并，减少drawcall，绘制1023个小球产生了3个drawcall，每个drawcall的最大缓冲区不一样，因此需要几个drawcall是根据不同机器不同平台来决定的，单个网格的绘制顺序与我们提供数组数据的顺序相同
        - SRP Batcher
            - 介绍
                - 不会减少DrawCall数量，但可以减少Set Pass Call 的数量，并减少绘制调用命令的开销。CPU不需要每帧都给GPU发送渲染数据，如果这些数据没有发生变化则会保存在GPU内存中，每个绘制调用仅需包含一个指向正确内存位置的偏移量
                - SRP Batcher是否会被打断的依据是Shader变种，即使物体之间使用了不同的材质，但是使用的Shader变种相同就不会被打断，传统的批处理方式是要求使用同一材质为前提的。
                - SRP Batcher 会在主存中将模型的坐标信息、材质信息、主光源阴影参数和非主光源阴影参数分别保存到不同的CBUFFER（常量缓冲区）中，只有CBUFFER发生变化才会重新提交到GPU并保存