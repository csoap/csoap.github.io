---
layout:     post
title:      "Unity下Lua开发注意事项"
subtitle:   ""
date:       2021-08-18
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    -lua
---
- 总结自分享课题：《Unity下Lua开发注意事项》，并归纳其他知识点进行总结笔记

- 不错的文章链接,建议反复阅读
    - https://www.cnblogs.com/zwywilliam/p/5999924.html
    - https://www.cnblogs.com/zwywilliam/p/5999980.html
    - https://www.cnblogs.com/zwywilliam/p/5992737.html

- 为什么使用lua热更
    - 之前Unity不支持热更新
    - UWA之前分享过 Android平台热更新解决方案，直接替换dll是一种解决方式(但iOS上因为使用IL2CPP故而无法实现，而且这事本质上为商业原因而非技术问题)
    - 考虑到以前项目有不少同事之前写的是Cocos2d-lua，而且也有对应后端框架如Skynet，因此立下项的时候考虑使用Unity+lua的方式来开发

- lua热更方案对比
    - https://blog.csdn.net/UWA4D/article/details/54344393
    - 比对：slua，ulua（不再维护，跳至tolua），tolua
    - 设备
        - 作者钱康来
        - 在 Android上对高、中、低配置的三款设备
            - 低端设备：三星 S3（Android OS 4.3）
            - 中端设备：红米 Note2（Android OS 5.0.2）
            - 高端设备：三星S6（Android OS 6.0.1）
        - iOS上对armv7和arm64的两款设备
            - armv7设备：iPhone 4s （OS 7.1.2）
            - arm64设备：iPhone 5s（OS 9.3.5）
    - 总结
        - 在Android设备上
            - 测试用例主要是大量的向量操作，slua较为占优
            - 测试用例主要是组件属性的赋值，和GameObject的创建等，tolua占用
            - 而ulua在中高端设备上基本介于两者之间，但在低端机上则有较为明显的性能问题
        - ios
            - tolua的性能表现最好
    - ILRuntime 原理
        - C#代码在编写后，是需要执行编译的，才能起效，这样如果在手机端，没有对应的编译环境，那么对应的c#代码就无法实现热更。ILRuntime实现的基础，也是基于AssetBundle的资源热更新方式，将需要热更新的c#代码打包成DLL，放在工程的StreamingAssets下，在每次完成资源打包后，对应的DLL会被作为资源热更新出去。这样就规避了编译相关的环节，实现了热更
    - tolua对比ILRuntime
        - 无论是ILRuntime还是toLua都是市面上有在用到的热更方案。直观上来讲，都可以通过把代码放在StreamingAssets中，从而达到热更的目的
        - 一般Unity都是C#编写的，所以单看迁移成本，ILRuntime是完胜


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

- luajit
    - trace compiler(跟踪编译)的特征：先运行字节码，针对热点代码做profile，了解了可以优化的点后再优化出最高效的机器码。这就是luajit目前的做法。
- 精简你的lua导出，否则IL2CPP会是你的噩梦
    - 网上已经有非常多IL2CPP导致包体积激增的抱怨，而基于lua静态导出后，由于生成了大量的导出代码。这个问题又更加严重。
    - 鉴于目前ios必须使用IL2CPP发布64bit版本，所以这个问题必须要重视，否则不但你的包体积会激增，binary是要加载到内存的，你的内存也会因为大量可能用不上的lua导出而变得吃紧
    - 移除你不必要的导出，尤其是unityengine的导出
    - 如果只是为了导出整个类的一两个函数或者字段，重新写一个util类来导出这些函数，而不是整个类进行导出。也可以使用[notolua]属性来标记不导出。
    - 如果有把握，可以修改自动导出的实现，自动或者手动过滤掉不必要导出的东西。
- ios在没有jit的加持下，luajit的性能特性与原生lua基本一致
    - 注意，这里说的不是“性能”一致，是“性能特性”一致。luajit不开启jit依然是要比原生lua快很多的。这里说的性能特性一致是指你可以按照原生lua的优化思路来优化luajit的非jit环境。因为ios下无法开启jit，只能使用interpreter，因为原生lua的优化方案基本都适用于ios下使用。这时，每一个a.b都意味着一次表查找，写代码的时候一定要考虑清楚，该cache的cache，该省的省。

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
    - 因为Lua的String是内部复用的，当我们创建字符串的时候，Lua首先会检查内部是否已经有相同的字符串了，如果有直接返回一个引用，如果没有才创建。
        - 优点：这使得Lua中String的比较和赋值非常地快速，因为只要比较引用是否相等、或者直接赋值引用就可以了
        - 缺点：降低了创建字符串时的效率，因为Lua需要去查找比较一遍。
    - 优化手段是，使用table.concat来代替。这里的原理主要是table.concat只会创建一块buffer，然后在此拼接所有的字符串，实际上是在用table模拟buffer；而用“..”来连接则每次都会产生一串新的字符串，开辟一块新的buffer。

        ```
        a = os.clock()
        local s = ''
        for i = 1, 30000 do
            s = s .. 'a'
        end
        b = os.clock()
        print(b - a) //6.65

        //用table模拟buffer
        a = os.clock()
        local s = ''
        local t = {}
        for i = 1, 300000 do
            t[#t + 1] = 'a'
        end
        s = table.concat(t, '')
        b = os.clock();
        print(b - a) // 0.07178  9倍多的效率提升

        ```

- 关于table
    - 每个表包含两部分：数组（array）部分和哈希（hash）部分。数组部分包含所有从1到n的整数键，其他的所有键都储存在哈希部分中。
    - 哈希部分其实就是一个哈希表，哈希表本质是一个数组，它利用哈希算法将键转化为数组下标，若下标有冲突即同一个下标对应了两个不同的键，则它会将冲突的下标上创建一个链表，将不同的键串在这个链表上，这种解决冲突的方法叫做：链地址法。
    - 当我们把一个新键值赋给表时，若数组和哈希表已经满了，则会触发一个再哈希rehash。再哈希的代价是高昂的。首先会在内存中分配一个新的长度的数组，然后将所有记录再全部哈希一遍，将原来的记录转移到新数组中。新哈希表的长度是最接近于所有元素数目的2的乘方
    - 最开始，Lua创建了一个空表a，在第一次迭代中，a[1] = true触发了一次rehash，Lua将数组部分的长度设置为2^0，即1，哈希部分仍为空。在第二次迭代中，a[2] = true再次触发了rehash，将数组部分长度设为2^1，即2。最后一次迭代，又触发了一次rehash，将数组部分长度设为2^2，即4

        ```
        // 当创建一个空表时，数组和哈希部分的长度都将初始化为0
        local a = {}
        for i=1,3 do
            a[i] = true
        end

        // 与上一段代码类似，只是其触发了三次表中哈希部分的rehash而已
        a = {}
        a.x = 1; a.y = 2; a.z = 3
        ```
    - 只有三个元素的表，会执行三次rehash；然而有一百万个元素的表仅仅只会执行20次rehash而已，因为2^20 = 1048576 > 1000000。但是，如果你创建了非常多的长度很小的表（比如坐标点：point = {x=0,y=0}），这可能会造成巨大的影响。如果你有很多非常多的很小的表需要创建时，你可以将其预先填充以避免rehash。比如：{true,true,true}，Lua知道这个表有三个元素，所以Lua直接创建了三个元素长度的数组。类似的，{x=1, y=2, z=3}，Lua会在其哈希部分中创建长度为4的数组。
    - 给table的key赋值nil并不会回收table的空间，除非触发下一次hash分配
    - 一张空表会产生64个字节的内存（64位的CPU）

- 闭包
    - lua内存增量有很大一部分来自临时table与闭包，所以尽量减少临时表与不必要的闭包

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

- 惯用法（精巧用法）
    - 一行代码实现表的拷贝
        - u = {unpack(t)}
    - 一行代码判断表是否为空
        - 用#t == 0并不能判断表是否为空，因为#运算符会忽略所有不连续的数字下标和非数字下标。
        - if next(t) == nil then --表为空  end   因为表的键可能为false，所以必须与nil比较，而不直接使用not next(t)来判断表是否空。

- 垃圾回收
    - Lua的垃圾回收器是一个增量运行的机制。即回收分成许多小步骤（增量的）来进行。
    - 频繁的垃圾回收可能会降低程序的运行效率。
    - 对于批处理的Lua程序来说，停止垃圾回收collectgarbage("stop")会提高效率，因为批处理程序在结束时，内存将全部被释放。
    - 对于垃圾回收器的步幅来说，实际上很难一概而论。更快幅度的垃圾回收会消耗更多CPU，但会释放更多内存，从而也降低了CPU的分页时间。只有小心的试验，我们才知道哪种方式更适合。

- lua 语法细节注意
    - 混淆 a.x 和 a [ x ］ 。 实际上，a.x 代表的是 a ［” x ”］ ，即由字符串” x ”索引的表；而 a[x ］则是指由变量 x 对应的值索引的表
    - 安全访问
        - 表的嵌套深度变得比较深时，这种写法就会很容易出错
        
        ``` lua
        zip = company and company.director and
                company.director.address and
                    company.director.address.zipcode

        -- 优化
        local E = {}
        zip = (((company or E).director or E).address or E).zipcode
        ```
- 总结
    - 我们应该在写代码时，按照高标准去写，尽量避免在事后进行优化。
    - 如果真的有性能问题，我们需要用工具量化效率，找到瓶颈，然后针对其优化
    - 终极武器
        - 使用LuaJIT，LuaJIT可以使你在不修改代码的情况下获得平均约5倍的加速。注意，Luajit只支持到Lua5.1，并不支持5.1之上的Lua版本。
        - 将瓶颈部分用C/C++来写。因为Lua和C的天生近亲关系，使得Lua和C可以混合编程。但是C和Lua之间的通讯会抵消掉一部分C带来的优势。（这两者并不是兼容的，你用C改写的Lua代码越多，LuaJIT所带来的优化幅度就越小。）
        