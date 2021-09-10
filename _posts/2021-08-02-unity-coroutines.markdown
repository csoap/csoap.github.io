---
layout:     post
title:      "Unity的协程理解"
subtitle:   ""
date:       2019-11-29
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - unity,协程
---
- 什么是协程
    - unity协程是一个能够暂停协程执行，暂停后立即返回主函数，执行主函数剩余的部分，直到中断指令完成后，从中断指令的下一行继续执行协程剩余的函数。函数体全部执行完成，协程结束。由于中断指令的出现，使得可以将一个函数分割到多个帧里去执行。
    - 好处
        - 让原来要使用异步 + 回调方式写的非人类代码, 可以用看似同步的方式写出来。
        - 能够分步做一个比较耗时的事情，如果需要大量的计算，将计算放到一个随时间进行的协程来处理，能分散计算压力
    - 坏处
        - 协程本质是迭代器，且是基于unity生命周期的，大量开启协程会引起gc
        - 如果同时激活的协程较多，就可能会出现多个高开销的协程挤在同一帧执行导致的卡帧
    - 协程有什么坑点
        - 协程的停止和创建必须用相同形式。字符串、IEnumerator、Coroutine形式
        - 禁用脚本(this.enabled = false)不会停止协程执行
        - 删除脚本（Destory(this)）或禁用GameObject会停止协程执行
- 协程底层原理
    > unity有多条渲染线程，但是对于代码的调度是在一个主线程，协程是靠操作系统在主线程的调度实现的
    - 实现原理
        - yield 相当是暂停本次生命周期执行，或者是转移控制权给unity继续生命周期
            - 每次执行到yield 暂停一下，然后下次又继续执行 如何实现？ 本质上是通过IEnumetator.MoveNext()实现的
        - 生成一个CountCoroutine的类,本质是状态机。构造方法会设置一个初始状态state为0，当state != 1 结束协程。这个类持有一个NevBehaviourScript的脚本对象，也就是为什么访问到脚本对象的各个字段
    - 协程的缺点
        - 依赖于MonoBehaviour
            - 大型商业游戏很有可能脚本不会继承自MonoBehaviour
            - MVC框架，基本只有V跟monoBehaviour打交道，M和C基本不会打交道，如果想在M或C启动协程（例：计算buff持续伤害等）就会有问题。
            - 怎么解决？实现一个自定义调度器，怎么实现？需要了解一下
        - 不能有返回值
        - 回调地狱，回调层数多了以后维护起来太复杂。。
            - 例子：如果需求是下载一张图片后下载另一张，持续很多张图片，就变成俄罗斯套娃了。
            ```
            hw.GET("xxx", (www1) =>
            {
                hw.Get("yyy", (www2)=>
                {

                });
            });

            //怎么解决，改成用 Async/Await? await什么原理?又需要去了解了。。
            await hw.Get("xxx");
            //do something
            await hw.Get("yyy");
            //do something
            ```

- 进程，Unity线程和协程之间关系
    - 进程：是运行一个程序所需要的基本资源。运行的应用程序在操作系统中被视为一个进程，进程可以包括一个或多个线程。
        - unity协程是利用C#的迭代器实现的一个机制，编译器会根据yield return，展开成一段类似状态机的代码。
        - StopCoroutine方法可以把协程停下来，物体被关闭了协程也会停。enbale = false 不会阻止协程的运行。
        - 协程代码卡的话，会把主线程卡住。同一时间只能执行某个协程，协程适合对某个任务进行分时处理。
        - 协程不是线程，也不是异步执行，跟Update一样，在主线程中执行；多线程是会开子线程的。
        - 子线程不能访问Unity的资源，比如 UI、GameObject、Debug.Log。
        - 协程(协同程序): 同一时间只能执行某个协程。开辟多个协程开销不大。协程适合对某任务进行分时处理。线程: 同一时间可以同时执行多个线程。开辟多条线程开销很大。线程适合多任务同时处理。
- 协程和线程的区别
    - 线程是完全异步的,线程是利用多核达到真正的并行计算，缺点是会有大量的锁、切换、等待的问题，而协程是非抢占式，需要用户自己释放使用权来切换到其他协程, 因此同一时间其实只有一个协程拥有运行权, 相当于单线程的能力。
    - 协程则是在线程中执行的一段代码，它并不能做到真正异步。协程的代码可以只执行其中一部分，然后挂起，等到未来一个恰当时机再从原来挂起的地方继续向下执行。
    - 协程是 C# 线程的替代品, 是 Unity 不使用线程的解决方案。
    - 使用协程不用考虑同步和锁的问题
    - 多个协程可以同时运行，它们会根据各自的启动顺序来更新
- yield
    - yield是一个多义英文单词，有放弃、投降、 生产、 获利等诸多意思，在这边我理解为生产、产量的意思
    - 在C#中，yield语句有以下两种用法
        - yield return <expression>;
        - yield break;

    ```
    static void Main()
    {
        IEnumerable<int> numbers = SomeNumbers();
        foreach (int number in numbers)
        {
            Console.Write(number.ToString() + " ");
        }
        // 代码执行完毕后将会输出: 3 5 8
    }

    public static System.Collections.IEnumerable SomeNumbers()
    {
        yield return 3;
        yield return 5;
        yield return 8;
    }
    ``
    - SomeNumbers是一个简单的迭代器方法。SomeNumbers被调用的时候，其函数本体并不会执行，而是给变量numbers赋值了一个IEnumerable<int>类型。
    - 每一次的foreach循环执行的时候，numbers的MoveNext方法会被调用到，MoveNext方法会执行SomeNumbers方法内的语句，直到碰到yield return语句。
    - yield return的意义是**返回一个值给foreach的每次迭代，然后终止迭代器方法的执行。等到下一次迭代时，迭代器方法会从原来的位置继续往下运行**
    - 可以理解为迭代器方法SomeNumbers为一个生产者，foreach循环的每次循环MoveNext是一个消费者。每一次循环往下移动一格，消费者便要向迭代器方法要一个值。所以，我想yield在其中的意思应该是**产出**的意思，而且是按需生产的意思。
    - yield break则可以视为终止产出的意思，即结束迭代器的迭代。