---
layout:     post
title:      "lua的upvalue和闭包"
subtitle:   "摘录自风云来"
date:       2021-08-13
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    -lua
---
- 非原创，摘录自风云来，地址：
https://blog.csdn.net/chenjiayi_yun/article/details/25219937

- Lua函数可以被当成参数传递，也可以被当成结果返回，在函数体中仍然可以定义内嵌函数。lua闭包是Lua函数生成的数据对象。每个闭包可以有一个upvalue值，或者多个闭包共享一个upvalue数值。
    - upvalue
        - 如果函数f2定义在函数f1中，那么f2为f1的内嵌函数，f1为f2的外包函数，外包和内嵌都具有传递性，即f2的内嵌必是f1的内嵌，f1的外包必定是f2的外包
        - 内嵌函数可以访问外包函数已经创建的局部变量，而这些局部变量则成为该内嵌函数的局部变量

        ```
        function　f1(n)
            --　函数参数也是局部变量
            local　function　f2()
                print(n)　--　引用外包函数的局部变量
            end
            return　f2
        end
        g1　=　f1(1979)
        g1()　--　打印出1979
        g2　=　f1(500)
        g2()　--　打印出500
        ```

        - 当执行完g1　=　f1(1979)后，局部变量n的生命本该结束，但因为它已经成了内嵌函数f2的upvalue，它又被赋给了变量g1，所以它仍然能以某种形式继续“存活”下来，从而令g1()打印出正确的值。
            - 什么形式?
                - 一旦upvalue即将离开自己的作用域，在从堆栈上消除之前，闭包就会为它分配空间并保存当前的值，以后便可通过指向新分配空间的引用来访问该upvalue。
    - 闭包
        - Lua编译一个函数时，其中包含了函数体对应的虚拟机指令、函数用到的常量值(数，文本字符串等等)和一些调试信息。在运行时，每当Lua执行一个形如function...end　这样的函数时，它就会创建一个新的数据对象，其中包含了相应函数原型的引用、环境(用来查找全局变量的表)的引用以及一个由所有upvalue引用组成的数组，而这个数据对象就称为闭包。由此可见，函数是编译期概念，是静态的，而闭包是运行期概念，是动态的。g1和g2的值严格来说不是函数而是闭包，并且是两个不相同的闭包，而这两个闭包保有各自的upvalue值。

        ```
        function　f1(n)
            local　function　f2()
                print(n)
            end
            n　=　n　+　10
            return　f2
        end
        
        g1　=　f1(1979)
        g1()　--　打印出1989
        ```

        - g1()打印出来的是1989，原因是打印的是upvalue的值。
        - upvalue实际是局部变量，而局部变量是保存在函数堆栈框架上的，所以只要upvalue还没有离开自己的作用域，它就一直生存在函数堆栈上。这种情况下，闭包将通过指向堆栈上的upvalue的引用来访问它们，一旦upvalue即将离开自己的作用域，在从堆栈上消除之前，闭包就会为它分配空间并保存当前的值，以后便可通过指向新分配空间的引用来访问该upvalue。当执行到f1(1979)的n　=　n　+　10时，闭包已经创建了，但是变量n并没有离开作用域，所以闭包仍然引用堆栈上的n，当return　f2完成时，n即将结束生命，此时闭包便将变量n(已经是1989了)复制到自己管理的空间中以便将来访问。

    - upvalue和闭包数据共享
        - 单重内嵌函数的闭包 （函数创建的闭包）
            - 一个函数创建的闭包共享一份upvalue。

            ```
            function　Create(n)
                local　function　foo1()
                    print(n)
                end
                local　function　foo2()
                    n　=　n　+　10
                end

                return　foo1,foo2
            end

            f1,f2　=　Create(1979)--创建闭包
            f1()　--　打印1979
            f2()
            f1()　--　打印1989
            f2()
            f1()　--　打印1999
            ```
            - f1,f2这两个闭包的原型分别是Create中的内嵌函数foo1和foo2，而foo1和foo2引用的upvalue是同一个，即Create的局部变量n。执行完Create调用后，闭包会把堆栈上n的值复制出来，那么是否f1和f2就分别拥有一个n的拷贝呢？其实不然，当Lua发现两个闭包的upvalue指向的是当前堆栈上的相同变量时，会聪明地只生成一个拷贝，然后让这两个闭包共享该拷贝，这样任一个闭包对该upvalue进行修改都会被另一个探知。上述例子很清楚地说明了这点：每次调用f2都将upvalue的值增加了10，随后f1将更新后的值打印出来。upvalue的这种语义很有价值，它使得闭包之间可以不依赖全局变量进行通讯，从而使代码的可靠性大大提高。

        - 多重内嵌函数的闭包 （闭包创建的闭包）
            - 同一闭包创建的其他的闭包共享一份upvalue。
闭包在创建之时其需要的变量就已经不在堆栈上，而是引用更外层外包函数的局部变量（实际上是upvalue）。
            ```
            function　Test(n)
                local　function　foo()
                    local　function　inner1()
                        print(n)
                    end

                    local　function　inner2()
                        n　=　n　+　10
                    end

                    return　inner1,inner2
                end
                return　foo
            end

            t　=　Test(1979)--创建闭包（共享一份upvalue）
            f1,f2　=　t()--创建闭包
            f1()　　　　　　　　--　打印1979
            f2()
            f1()　　　　　　　　--　打印1989
            g1,g2　=　t()
            g1()　　　　　　　　--　打印1989
            g2()
            g1()　　　　　　　　--　打印1999
            f1()　　　　　　　　--　打印1999
            ```

            - 执行完t = Test(1979)后，Test的局部变量n就结束生命周期了，所以当f1,f2这两个闭包被创建时堆栈上根本找不到变量n。Test函数的局部变量n不仅是foo的upvalue，也是inner1和inner2的upvalue。t　=　Test(1979)之后，闭包t  已经把n保存为upvalue，之后f1、f2如果在当前堆栈上找不到变量n就会自动到它们的外包闭包(这里是t的)的upvalue引用数组中去找.
            - g1和g2与f1和f2共享同一个upvalue。因为g1和g2与f1和f2都是同一个闭包t 创建的，所以它们引用的upvalue  (变量n)实际也是同一个变量，而它们的upvalue引用都会指向同一个地方。