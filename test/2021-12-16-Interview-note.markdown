---
layout:     post
title:      "面试记录"
subtitle:   ""
date:       2021-12-16
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 面试
---

- 记录一些没想过的问题,项目相关问题略过

- 天天玩家（北京）
    - 项目类型:休闲模拟,了解了项目情况就不感兴趣了
    - 没接触的问题
        - 特效节点如何在两个图片中显示
            - 特效挂在新的canvas下，canvas调整层级顺序
        - 特效遮罩 如何实现
            - https://www.freesion.com/article/7333908188/
            - 求出容器的边界
            - 把边界传给每一个条目的shader
            - 判断顶点坐标是否超出边界坐标，把超出的部分透明度设为0
- 龙创悦动(北京)
    - 项目:SLG(已上线处于数据调优阶段)
    - 一面
        - 如何实现链表的快速查询(回答多级链表)
    - 二面 主程
- 完美世界(北京)
    - 项目:二次元卡牌
    - 快速排序
    - 备注:
        - 技术面 + 制作人面
        - hr 双面
- 光宇游戏(北京)
    - 变量a,b在不用其他辅助变量交换值
    - 实现斐波那契的几种做法
    - C#
        - string StringBuilder区别 以及对应线程安全
        - ref out
        - struct class
        - 接口和 抽象类
        - try catch finally,try中return,finally是否会执行,为何会执行finally(当时回答会)
        - 堆内存,栈内存, 整形能否存在堆内存中(如int a 放在某个对象中即可)
    - lua
        - local 实现机制
        - lua 垃圾回收 三色标记状态分别是什么
        - lua 元表, 应用的场景
    - unity
        - unity生命周期(按顺序)
        - overdraw (是什么? 怎么做)
        - drawcall(是什么? 怎么做)
    - a,b两个变量在不借助其他变量,如何交换值