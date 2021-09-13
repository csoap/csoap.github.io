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

- 关键词 IEnumerator
    - 一个迭代器，三个基本的操作：Current/MoveNext/Reset
    ```csharp
    public interface IEnumerator
    {
        bool MoveNext();
        void Reset();
        Object Current{get;}
    }
    ```

- c#对协程调用的编译结果

    ```csharp
    class Test
    {
        static IEnumerator GetCounter()
        {
            for(int count = 0; count < 10; count++)
            {
                    yiled return count;
            }
        }
    }
    ```
    - 编译后的c++结果
    ```cpp
    internal class Test
    { 
        // GetCounter获得结果就是返回一个实例对象
        private static IEnumerator GetCounter()
        {
            return new <GetCounter>d__0(0);
        }

        // Nested type automatically created by the compiler to implement the iterator
        [CompilerGenerated]
        private sealed class <GetCounter>d__0 : IEnumerator<object>, IEnumerator, IDisposable
        {
            // Fields: there'll always be a "state" and "current", but the "count"
            // comes from the local variable in our iterator block.
            private int <>1__state;
            private object <>2__current;
            public int <count>5__1;

            [DebuggerHidden]
            public <GetCounter>d__0(int <>1__state)
            {
            //初始状态设置
                this.<>1__state = <>1__state;
            }

            // Almost all of the real work happens here
            //类似于一个状态机，通过这个状态的切换，可以将整个迭代器执行过程中的堆栈等环境信息共享和保存
            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<count>5__1 = 0;
                        while (this.<count>5__1 < 10)        //这里针对循环处理
                        {
                            this.<>2__current = this.<count>5__1;
                            this.<>1__state = 1;
                            return true;
                        Label_004B:
                            this.<>1__state = -1;
                            this.<count>5__1++;
                        }
                        break;

                    case 1:
                        goto Label_004B;
                }
                return false;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            object IEnumerator<object>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
    ```
- **所以我们在执行开启一个协程的时候，其本质就是返回一个迭代器的实例，然后在主线程中，每次update的时候，都会更新这个实例，判断其是否执行MoveNext的操作，如果可以执行(比如文件下载完成)，则执行一次MoveNext，将下一个对象赋值给Current(MoveNext需要返回为true， 如果为false表明迭代执行完成了**



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
            - WWW : 常见的web操作，在每帧末调用，会检查isDone/isError，如果true，则 call MoveNext
            - WaitForSeconds: 检测间隔时间是否到了，返回true， 则call MoveNext
            - null: 直接 call MoveNext
            - WaitForEndOfFrame: 在渲染之后调用， call MoveNext
        - yield break; 跳出协程的操作，一般用在报错或者需要退出协程的地方。

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

- gc
    - 调用 StartCoroutine()会产生少量的内存垃圾，因为unity会生成实体来管理协程。所以在游戏的关键时刻应该限制该函数的调用。基于此，任何在游戏关键时刻调用的协程都需要特别的注意，特别是包含延迟回调的协程。
    - yield在协程中不会产生堆内存分配，但是如果yield带有参数返回，则会造成不必要的内存垃圾,比如 yield return 0;, 由于返回0，引发了装箱操作，所以会产生内存垃圾。为了避免可以写:yield return null;
        >> 在Unity的装箱操作中，对于值类型会在堆内存上分配一个System.Object类型的引用来封装该值类型变量，其对应的缓存就会产生内存垃圾。装箱操作是非常普遍的一种产生内存垃圾的行为，即使代码中没有直接的对变量进行装箱操作，在插件或者其他的函数中也有可能会产生。最好的解决办法是尽可能的避免或者移除造成装箱操作的代码。

- lua 中的协程
    - Lua中的协程和unity协程的区别，最大的就是其不是抢占式的执行，也就是说不会被主动执行类似MoveNext这样的操作，而是需要我们去主动激发执行
        - coroutine.create()/wrap: 构建一个协程, wrap构建结果为函数，create为thread类型对象
        - coroutine.resume(): 执行一次类似MoveNext的操作
        - coroutine.yield(): 将协程挂起

        ```lua
        local func = function(a, b)
            for i= 1, 5 do
                print(i, a, b)
            end
        end

        local func1 = function(a, b)
            for i = 1, 5 do
                print(i, a, b)
                coroutine.yield()
            end
        end

        co =  coroutine.create(func)
        coroutine.resume(co, 1, 2)
        --此时会输出 1 ，1， 2/ 2，1，2/ 3， 1，2/4，1，2/5，1，2

        co1 = coroutine.create(func1)
        coroutine.resume(co1, 1, 2)
        --此时会输出 1， 1，2 然后挂起
        coroutine.resume(co1, 3, 4)
        --此时将上次挂起的协程恢复执行一次，输出： 2, 1, 2 所以新传入的参数3，4是无效的
        ```
    - lua协程的准备工作
        
        ```lua
        lua.Start(); -- 虚拟机的 lua.Start 函数初始化
        LuaBinder.Bind(lua); -- 调用LuaBinder的静态方法
        looper = gameObject.AddComponent<LuaLooper>(); -- 为你的一个游戏对象添加组件  LuaLooper
        looper.luaState = lua; --  LuaLooper  的内部虚拟机引用指定为我们创建的虚拟机

        -- 然后就可以正常的使用Lua中的协程了，它会在c#每一帧驱动lua的协同完成所有的协同功能，这里的协同已经不单单是lua自身功能，而是tolua#模拟unity的所有的功能
        ```