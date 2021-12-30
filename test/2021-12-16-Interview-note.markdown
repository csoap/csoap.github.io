---
layout:
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
        - 还有什么查找算法
    - 二面 主程
- 完美世界(北京)
    - 项目:二次元卡牌
    - 快速排序
    - 备注:
        - 技术面 + 制作人面
            - unity如何编译脚本
                - 默认情况下，Unity 几乎将所有游戏脚本都编译到预定义 程序集 Assembly-CSharp.dll 中
                    - 每次更改一个脚本时，Unity 都必须重新编译所有其他脚本
                    - 所有脚本都针对所有平台进行编译
                - 怎么优化编译速度
                    - 打包DLL
                        - 把部分代码打包成dll，就像DOTween这种插件直接提供了dll一样， 缺点：处理不同的宏和DLL引用
                    - 利用Unity多阶段编译（分四步编译），四个阶段：https://docs.unity3d.com/cn/2017.4/Manual/ScriptCompileOrderFolders.html
                        - 插件Mad Compile Time Optimizer
                        - 只要将不常修改的代码放到特定文件夹就完事儿了其实…这里我选择的是Standard Assets文件夹。这样唯一的一个限制是Standard Assets里的代码无法引用外面的代码，不过这里全是放的插件
        - hr 双人面
- 光宇游戏(北京)
    - 变量a,b在不用其他辅助变量交换值
    - 实现斐波那契的几种做法
    - C#
        - string StringBuilder区别 以及对应线程安全
            - string 线程安全, stringbuilder线程不安全,频繁拼接用stringbuffer
        - ref out
            - https://blog.csdn.net/qiaoquan3/article/details/51201398
        - struct class
            - https://www.cnblogs.com/gsk99/archive/2010/12/13/1904552.html
            - class和struct最本质的区别是class是引用类型，而struct是值类型
            - class
                - new一个类的实例时，在堆栈（stack）上存放该实例在托管堆（managed heap）中的地址，而实例的值保存在托管堆（managed heap）中
            - struct
                - struct实例在创建时分配在线程的堆栈（stack）上，它本身存储了值
        - 接口和 抽象类
            - https://www.cnblogs.com/lidaying5/p/10515251.html
            - 抽象类主要用来规定某些类的基本特征
            - 主要用来表示不同类之间的共有特征
        - try catch finally,try中return,finally是否会执行,为何会执行finally(当时回答会)
        - 堆内存,栈内存, 整形能否存在堆内存中(如int a 放在某个对象中即可)
    - lua
        - local 实现机制
        - lua 垃圾回收 三色标记状态分别是什么
        - lua 元表, 应用的场景
    - unity
        - unity生命周期(按顺序)
        - overdraw (是什么? 怎么做)
            - https://blog.csdn.net/yu1368072332/article/details/85676537
            - what
                - 在unity中，每次CPU准备数据并通知GPU的过程就称之为一个DrawCall。
                - 具体过程就是：设置颜色-->绘图方式-->顶点坐标-->绘制-->结束，所以在绘制过程中，如果能在一次DrawCall完成所有绘制就会大大提高运行效率，进而达到优化的目的。
        - drawcall(是什么? 怎么做)
            - what
                - 也就是过度绘制，就是在游戏运行过程中重复绘制同一像素的问题
    - a,b两个变量在不借助其他变量,如何交换值

- 自我介绍思考
    - 引导到工作内容,取得什么成就
- 阿里灵犀互娱（上海）
    - 项目：MMO 仙侠类 （已研发2年，预计22年下半年上线，已耗资2亿）
    - 公司 文化
        - 客户第一：客户是衣食父母；
        - 团队合作：共享共担，平凡人做非凡事；
        - 拥抱变化：迎接变化，勇于创新；
        - 诚信：诚实正直，言行坦荡；
        - 激情：乐观向上，永不放弃；
        - 敬业：专业执着，精益求精。