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

- 天天玩家（北京）
    - 基本都是问项目（略过）
    - 没接触的问题
        - 特效节点如何在两个图片中显示
            - 特效挂在新的canvas下，canvas调整层级顺序
        - 特效遮罩 如何实现
            - https://www.freesion.com/article/7333908188/
            - 求出容器的边界
            - 把边界传给每一个条目的shader
            - 判断顶点坐标是否超出边界坐标，把超出的部分透明度设为0