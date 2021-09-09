---
layout:     post
title:      "Unity下Lua开发注意事项"
subtitle:   ""
date:       2021-08-13
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    -lua
---
- 总结自公司项目组主程分享课题：《Unity下Lua开发注意事项》，并归纳其他知识点进行总结笔记

- Unity+Lua，性能注意点
    - lua跟c#交互时的性能
    - lua代码本身的性能，CPU & 内存

- Lua和C#交互的成本，Lua中如果要调用一次C#的函数，至少有几个步骤,拿ToLua举例
    - 在Lua层面，找到C#这个函数的Wrapper的C指针
    - C#层面，进行参数个数，参数类型的验证
    - 不同类型的参数校验成本又是不一样的
        - Number类型，调用LuaDLL.luaL_checknumber  进行一次验证即可
        - String类型，需要先LuaDLL.lua_type 获取类型，根据不同类型再调用一次LuaDLL的对应tostring接口
        - Struct类型，如Vector3等，需要调用LuaDLL.tolua_getvec3获取结构体的值，再new一个Vector3
    -  好不容易走完前面的步骤，还有返回值需要处理，调用成本很高，交互次数少一次都是赚的

    ```
    //csharp脚本
    public static Vector3 GetLocalPosition(this Component cmpt)
    {
        return cmpt.transform.localPosition;
    }
    public static void GetLocalPositionEx(this Component cmpt, out float x, out float y, out float z)
    {
        Transform trans = cmpt.transform;
        x = trans.localPosition.x;
        y = trans.localPosition.y;
        z = trans.localPosition.z;
    }

    ```
    - 测试脚本，访问次数为100000次
        - 例1:
            - local pos = me.Root.transform.position
            - local pos = me.Root:GetLocalPosition()
            - local x,y,z = me.Root:GetLocalPositionEx()
            - 测试结果（单位秒）：
                - 0.22617602348328
                - 0.1167140007019
                - 0.052457094192505
        - 例2:
            - local y = me.Root.transform.localPosition.y
            - local y = me.Root:GetLocalPositionY()
            - 测试结果：
                - 0.2229311466217
                - 0.052457094192505

- 精简你的lua导出，否则IL2CPP会是你的噩梦
    - 网上已经有非常多IL2CPP导致包体积激增的抱怨，而基于lua静态导出后，由于生成了大量的导出代码。这个问题又更加严重。
    - 鉴于目前ios必须使用IL2CPP发布64bit版本，所以这个问题必须要重视，否则不但你的包体积会激增，binary是要加载到内存的，你的内存也会因为大量可能用不上的lua导出而变得吃紧
    - 移除你不必要的导出，尤其是unityengine的导出
    - 如果只是为了导出整个类的一两个函数或者字段，重新写一个util类来导出这些函数，而不是整个类进行导出。也可以使用[notolua]属性来标记不导出。
    - 如果有把握，可以修改自动导出的实现，自动或者手动过滤掉不必要导出的东西。

- lua中的GC
    - 在 Lua 中，一共只有 9 种数据类型，分别为 nil 、boolean 、lightuserdata 、number 、string 、 table 、 function 、 userdata 和 thread 。其中，只有 string table function thread 四种在 vm 中以引用方式共享，是需要被 GC 管理回收的对象。其它类型都以值形式存在。
    - Unity中的Vector3、Vector2、Color、Quaternion等结构体，toLua都在Lua中定义相应功能的table，以方便使用。因为以上结构体，都是table的方式，所以，如果使用频繁的话，就容易产生大量的堆内存。
    - SLG项目，内城、外城移动地图的时候，都会产生大量对象的位置修改，必定会用到巨大量的Vector3变量，如果每次使用都去新建table，会产生巨多的GC，可能每秒会有几十KB的堆内存增长。
- 项目中频繁使用table举例
    - 位置设置的时候使用Vector3
        - 直接使用x,y,z去设置位置
        - 获取为Vector3做一个对象池，高频使用的地方向池子申请和回收对象
    - 事件传递避免创建table

    ```
    handler(target,function),用于事件传递
    SoraDAddMessage(self, “MESSAGE_CHAT_DATA”, handler(self, self.onServerHistoryData))   //错误示例
    GameMsg.AddMessage(“BUILD_JUMPTO_GET_RES”, self, self.OnTrainSoldierGetRes) //正确
    ```
- 再谈Vector3
    - 进行位置计算的时候，经常会用到Vector3 + - 等运算
        - Vector3.__add = function(va, vb)	return _new(va.x + vb.x, va.y + vb.y, va.z + vb.z)end
    - Local pos = pos1+pos2, pos1和pos2都是Vector3的Table，这行语句就会返回一张新的table，以此类推，- * / 都是一样的
    - 建议：
        - function Vector3.Add(self,b)        self[1],self[2],self[3]=self[1]+b[1],self[2]+b[2],self[3]+b[3]    end

- 字符串拼接使用table.concat
    - 因为Lua的String是内部复用的，当我们创建字符串的时候，Lua首先会检查内部是否已经有相同的字符串了，如果有直接返回一个引用，如果没有才创建。这使得Lua中String的比较和赋值非常地快速，因为只要比较引用是否相等、或者直接赋值引用就可以了
    - 优化手段是，使用table.concat来代替。这里的原理主要是table.concat只会创建一块buffer，然后在此拼接所有的字符串，实际上是在用table模拟buffer；而用“..”来连接则每次都会产生一串新的字符串，开辟一块新的buffer。

- 关于table
    - 每个表包含两部分：数组（array）部分和哈希（hash）部分。数组部分保存的项以整数为键（key），从 1 到某个特定的 n，（稍后会讨论 n 是怎么计算的。）所有其他的项（包括整数键超出范围的）则保存在哈希部分
    - 当 Lua 想在表中插入一个新的键值而哈希数组已满时，Lua 会做一次重新哈希（rehash）。重新哈希的第一步是决定新的数组部分和哈希部分的大小。所以 Lua 遍历所有的项，并加以计数和分类，然后取一个使数组部分用量过半的最大的 2 的指数值，作为数组部分的大小。而哈希部分的大小则是一个容得下剩余项（即那些不适合放在数组部分的项）的最小的 2 的指数值。





- 使用local
    - 5.0开始，Lua 使用了一个基于寄存器的虚拟机。这些「寄存器」跟 CPU 中真实的寄存器并无关联，因为这种关联既无可移植性，也受限于可用的寄存器数量。Lua 使用一个栈（由一个数组加上一些索引实现）来存放它的寄存器。每个活动的（active）函数都有一份活动记录（activation record），活动记录占用栈的一小块，存放着这个函数对应的寄存器。因此，每个函数都有其自己的寄存器。由于每条指令只有 8 个 bit 用来指定寄存器，每个函数便可以使用多至 250 个寄存器。
    - 对于需要频繁访问的全局table或者函数，应该先定义一个local引用，再通过这个local变量进行访问。
        - 不管是函数还是对象，不管是Lua自身的库，还是我们自己写的，在Lua中都是储存在_G中的某个节点下。当我们调用一个函数/对象的时候，Lua首先会去他们的表中查找到这个函数/对象，而我们使用局部变量的话，由于Lua的局部变量是储存在寄存器(这里的寄存器不对应CPU的寄存器)内的，所以这个访问就会快很多。更严重的是，如果使用的是父类的函数/对象，还会触发__index，这样不仅会有额外的耗时还有内存开销。

    - 有了这么多的寄存器，Lua的预编译器能把所有的local变量储存在其中。这就使得Lua在获取local变量时其效率十分的高。
    - 假设a和b为local变量，a = a + b的预编译会产生一条指令
    
    ```
    //a是寄存器0，b是寄存器1
    ADD 0 0 1
    ```

    - 若a和b都没有声明为local变量，则预编译会产生如下指令：

    ```
    GETGLOBAL 0 0
    GETGLOBAL 1 1
    ADD       0 0 1
    SETGLOBAL 0 0
    ```

    ```
    for i = 1, 1000000 do 
    local x = math.sin(i) 
    end
    下面的实现方式性能比上门的好30%
    local sin = math.sin 
    for i = 1, 1000000 do 
    local x = sin(i) 
    end
    ```