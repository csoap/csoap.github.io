---
layout:     post
title:  "他人面经"
subtitle:   ""
date:       2021-08-25
author:     "CSoap"
header-img: "img/post-bg-js-module.jpg"
tags:
---

- 21.08.25 字节一面 ,微信群分享
	- 输入一个整数数组，实现一个函数来调整该数组中数字的顺序，使得所有的奇数位于数组的前半部分，所有的偶数位于数组的后半部分，并保证奇数和奇数，偶数和偶数之间的相对位置不变。
		- https://www.nowcoder.com/practice/beb5aa231adc45b2a5dcc5b62c93f593?tpId=13&tqId=11166&tPage=1&rp=1&ru=%2Fta%2Fcoding-interviews&qru=%2Fta%2Fcoding-interviews%2Fquestion-ranking
	- 性能优化聊得比较深
	- GC怎么做
	- 资源管理怎么做
	- 帧同步
	- 假设我有一个八倍镜，开镜看到敌人的时候，怎么会不卡顿
- 21.09.02 米哈游 网游 原神项目组
	- C#各种关键字、细节和区别
		- ref、out的区别
		- 什么是重写、什么是重载
	- unity
		- Update和fixUpdate、LateUpdate 的区别
			- 当MonoBehaviour启用时，其Update和fixUpdate、LateUpdate在每一帧被调用
			- Update
				- update是每次渲染新的一帧时才会调用，和机器性能有着紧密的联系。如果机器的性能较差或者场景较复杂，Update的更新频率会出现较大的波动，因此和机型性能以及需要精准计算的操作不宜出现在Update中
			- fixUpdate 固定更新
				- FixedUpdate是固定时间更新，也就是说这个函数和帧率无关，他的更新频率是固定的。在unity中默认的更新频率是0.02秒，当然你也可以通过Edit->ProjectSetting->Time->FixedTimeStep来设置更新频率。
				- FixedUpdate应用于物理碰撞和倒计时等需要精准数据的操作。
					- 比如Force，Collider，Rigidbody等。外设的操作也是，比如说键盘或者鼠标的输入输出Input，因为这样GameObject的物理表现的更平滑，更接近现实。
				- 如果把物理操作放在update中，会有什么问题？
					- 因为碰撞检测的时间不是固定的，当物体发生碰撞时，如果人物未停止移动，则下一帧物体会继续移动，这样就会产生抖动。
					- 通俗来讲就是Update的更新频率太快大于了碰撞检测的频率，物体已经向前移动了，但还未进行碰撞检测，因此人物会穿越过去，待碰撞检测完毕后人物又会被弹出去。
						- 举例子

							```
							void Update()
							{
								a.transform.position = b.transform.position;
							}
							```

							- Unity有个主循环，一直在更新物体位置，然后渲染物体。
							- 如果在一次循环中，先更新B的Transform，再执行A和B的同步，那么结果会很完美。
							- 如果先执行A和B的同步，然后又更新B的Transform，然后再渲染物体，显然A和B不在同一个位置，A就会出现抖动现象。
			- LateUpdate 晚于更新
				- 当物体在Update里移动时，跟随物体的相机可以在LateUpdate里实现。
				- 每个脚本的update先后顺序无法确定。假如一个宿舍4个人，每个人的起床在update中执行，出发在某个人中的lateupdate执行，这样就可以保证每个人都起床了才会出发。
				- 例如物体的跟随（相机视角跟随人物移动）如果相机放在Update有可能会出现抖动，这是因为相机可能先于人物执行了跟随移动指令，人物之后再执行移动指令，此时相机的视距就会发生变化，因此就会产生抖动。如果经相机操作写在Update中可以确保人物位置确定完毕后再确定相机的位置和旋转。
- 米哈游 网友分享面经 水曜日鸡
	- https://mp.weixin.qq.com/s/dep-wfhZN3qRI2DdcSTryA
	- 一面：
		- 上来就问了topk问题，还问了随机数范围扩展的问题。还有unity底层的aot和jit是什么东西，unity工程里特殊文件夹。另外挑了我简历上的spine相关的东西，问了点spine的基础，问的不深。总共在半小时左右就结束了。

	- 二面：
		- 在一面两个礼拜后接到了二面的通知。推测是一面答得不太好，但是他们又缺人所以捞了起来。这次接着问了spine动画的东西，因为我简历上有写spine。问了一个很细的问题：动画上有关键帧，在播动画的时候如何知道当前时间应不应该触发这个关键帧。答案是记录上一帧的时间，如果关键帧落在上一帧和这一帧的范围内，说明应该触发。
		- 然后问了spine相关的项目经验，诸如你们spine是这么这么用的，为什么不那么用的问题。然后问了网络协议的问题。大概是你们项目使用的是udp还是tcp，知道kcp吗？（kcp是一种改良的可靠有序的udp，具体你们可以百度） 接着问了多线程的问题。因为当时面的时候完全不会多线程的东西，所以没有答出来。最后面试依旧是半小时左右就结束了。
	- 总结：
		- 米哈游作为我今年第一家面试的公司，可以说答得非常垃圾。这也跟我在公司里呆了两年从来没有出去面试过有比较大的关系。总结一下经验教训：
		- 经典的算法题需要准备。topK是一道经典的数据结构算法题，对于这种题目需要答出多种解法以及每种解法的复杂度分析。
		- 基础知识不扎实。像我这种转行学编程的最大的毛病就是基础不够扎实，和计算机专业出身的有天壤之别。
		- 对使用的工具不够了解。面试问的spine的问题，其实只要把官方文档看一遍就能懂得差不多了。
		- 缺乏对项目里技术方案的思考。典型的知其然不知其所以然。只是知道它是怎么做的，不知道为什么这么做，也没有考虑过和其他纵向方案的比较。
- 心动 网友分享面经 水曜日鸡
	- 基础方面问了网络协议。物理方面问了两个高速物体连续碰撞检测怎么实现、不规则物体的碰撞检测。UI方面问了UI和特效的显示层级处理。Lua方面问了很多，为什么使用xlua或者tolua、比较一下这两个方案、c#和lua之间交互的原理。制作人问了一下剧情系统要怎么设计，都会包含哪些功能。
	- 总结
		- 对lua不够了解，没有思考不同技术方案的区别。之前用lua只会在xlua或者tolua这种现成框架上写lua，不懂得它们是怎么和c#之间交互。后来把 《Lua程序设计》 看了一遍才懂。
		- 物理方面，会考到各种图形的碰撞检测算法。比如圆形和矩形相交，判断点是否在多边形里等等问题。还有SAT分离轴算法肯定要懂的，这个在 《实时碰撞检测算法技术》 这本书里有非常详细的讲解。
- 凉屋 网友分享面经 水曜日鸡
	- 问的全部都是基础，因为凉屋进去后需要程序员自己设计游戏，所以普通游戏公司的项目经验并没有什么用。
	- 面试的问题大部分都很简单我记得不是很清楚了，我记得的有引用类型和值类型的区别、为什么要拆箱装箱、空间变换矩阵包括旋转矩阵怎么写、w分量有什么用、向量点积和叉积。
	- 总结
		- 凉屋是一家典型的独游公司，它的面试和其它公司有比较大的不同。总结一下成功的经验：
		- 基础比项目经验重要。独立游戏公司做的游戏和商业游戏很不一样，所以普通的项目经验是基本没有用的。
		- 独游公司会考察你和公司的相性。需要你对独立游戏有一定的了解，对于对方公司的游戏最好也要玩过。像进凉屋这种做roguelike的公司，至少你得玩过《以撒》、《死亡细胞》、《传奇法师》、《挺进地牢》、《废土之王》等经典roguelike游戏吧，最好你还是个《元气骑士》资深玩家。
- 网易 上海某事业部 网友分享面经 水曜日鸡
	- 一面：
		- 一面上来就问了项目，一点没问基础。主要问了我简历上的帧同步架构和物理系统。物理系统方面，因为之前项目用到的物理系统是公司自己写的，所以问得很深。这方面因为我之前已经看过了公司的源码，并且自己还写了一遍改良版的，所以回答得还不错。（关于如何自己写一个简单的物理系统，我也分享过：【物理篇】从零搭建2D物理系统①）具体问题包括网格法和四叉树都是怎么实现的，两个算法各有什么问题，四叉树怎么改进，还有碰撞检测算法等等。帧同步没有问的很深，基本上是介绍一下帧同步，以及遇到了的一些问题怎么解决，比如说卡顿、数据传输、掉线等问题。
		- 因为事先有准备一面答得还算不错，很快就安排了二面。
	- 二面：
		- 二面依旧没问基础，只问项目。这次很深入的问了帧同步的东西。包括为什么要使用帧同步，为什么要用ecs，为什么要用预测回滚，除了预测回滚还有没有其他方案解决卡顿的问题等。这次的面试有一两个问题没答上来，最后的结果是没有通过。面试官评价是有一定潜力，但是深度不太够。
	- 总结：
		- 网易这次的面试完全不问基础让我挺意外。总结一下失败经验：
		- 对于简历上写的东西思考得还是不够深入。还是我第一篇文章说的那三大问题：怎么做？为什么这么做？为什么不那么做？详细来说就是简历上写的技术方案你要十分了解，能够知道该技术方案遇到的问题以及怎么解决，还要与其他技术方案进行比较。这个就是面试官问项目经验的思路。所以你必须把可能会问的的问题都提前准备好。
- 网易 上海另一个事业部 网友分享面经 水曜日鸡
	- 一面：
		- 这次面试只问基础，是我遇到过的问基础问的最难的一次。首先是数组和链表的区别，插入、删除复杂度的分析。算法题问了用栈实现队列。又问了红黑树和B+树。到这里其实还好。接着开始问起了计算机组成原理和操作系统，包括虚拟内存和物理内存，动态链接和静态链接，cpu多进程调度算法。到这里我就完全不会了，最后面试毫无悬念的没过。
	- 总结：
		- 这次的面试问到了计算机组成原理的知识，也是我之前完全没有准备的部分，所以失败得很惨。总结一下失败经验：
		- 计算机基础知识很重要，光准备算法和数据结构是不够的。 这方面的知识小公司不怎么考，不过大公司还是可能会考的。如果你像我一样也是非计算机专业转行的，这部分基础一定要找时间补起来，就算不为了面试了解这些知识对你也是很有帮助的。
- 腾讯 上海某工作室 网友分享面经 水曜日鸡
	- 一面
		- 也是只问项目，不问基础。一面问的关于项目的问题我全部都准备过了，所以总体下来非常顺利，没什么可以说的。面完第二天就约第二场面试了。
	- 二面：
		- 二面上来先发了一个笔试题过来，需要40分钟在自己电脑写完，然后再发过去。笔试题是腾讯内部自己出的，题目是给了几个数据结构，按照一定规则排序，题目有c++代码（所以游戏程序最好还是会点c++哪怕你只用unity），答案可以用你熟悉的语言写。做起来不算太难，也就leetcode简单难度的水平吧。需要注意审题，以及良好的代码习惯，毕竟这题完整写完要300多行。最后我因为审题错误答得不太好，后来面试官让我面试结束后重新做一遍给他发过去。笔试题做完之后依旧是问项目，依旧没有什么可以说的，问的问题我都准备过了。面完了之后我在40分钟内重新写了一遍笔试题给对方发过去。
	- 三面：
		- 这一面就是hr面了，hr面也没什么好说的。常见的会问的问题网上能找一大堆，诸如为什么考虑我们公司，自己的优点和缺点，工作中遇到的问题，领导怎么评价你的、未来的职业规划等等。值得一提的是hr还问了一下你最近玩什么游戏，喜欢什么游戏之类的话题。 目的应该是考察你对游戏行业的了解程度。 要回答这种问题很简单，你需要提前把对方公司做的游戏都了解一下，然后顺带了解一下他们的竞品。（比如我当时就简单说了一下天涯明月刀和一梦江湖的区别。）
	- 四面：
		- 在三面后两个多礼拜没有任何消息，就在我以为凉了的时候，对方过来联系我了。最后一轮面试是另一个hr面，跟上一轮hr问的问题差不多，还问了什么时候可以去报道之类的。最后顺利拿到了offer。
	- 总结：
		- 腾讯的面试没有我想象中那么难，一方面可能是因为我之前面试过很多公司，经验已经很丰富了吧。另一方面可能是运气好吧，我推测他们可能正缺人。
- 面试没通过之后怎么办
	- 面试没通过是很正常的，重要的在于需要知道自己为什么没通过。如果你面试答得本来就不好拿那就很简单了，自己缺那部分知识点有针对性的补。如果感觉自己还可以，但是面试没过可以问hr自己哪方面的能力不太行，希望以后还有机会跟贵公司合作。不要怕不好意思，反正你都没过了还怕啥。一般hr都会告诉你你的不足在哪里。

- 杭州无端 xgf
	- cpu 123级缓存大小
	- 怎么提高缓存命中率,具体一点,chunk内存需要控制在多少内缓存命中率最高
	- C#
		- 使用List和Dict对耗时有什么需要注意的实现
		- Dict里再Hash是什么?
		- delegate Action Func 这三个在使用中有什么注意实现
		- interface是在栈里还是堆里
		- class继承interface是在哪里
		- struct继承interface是在哪里
	- 多线程在移动平台怎么选择绑定大小核
		- https://developer.huawei.com/consumer/cn/forum/topic/0201676881402080409
			1. Unity绑核策略
			Unity中会优先将重载线程放到Unity判定的大核运行。

			Unity默认判定大核条件：

			优先级1：读取每个的cpu的cpu_capacity

			当前cpu_capacity / 所有cpu中cpu_capacity的最大值 >=0.85则认为是大核

			优先级2：读取CPU的最高频率（cpuinfo_max_freq或scaling_max_freq），

			当前cpu的最高频率 / 所有cpu最高频率最大值 >=0.85则认为是大核

			cpu_capacity路径：/sys/devices/system/cpu/cpu?/cpu_capacity

			cpuinfo_max_freq路径：/sys/devices/system/cpu/cpu?/cpufreq/cpuinfo_max_freq

			scaling_max_freq路径：/sys/devices/system/cpu/cpu?/cpufreq/scaling_max_freq

			说明：部分机型无cpu_capacity，如Kirin960

			cpu实时频率：/sys/devices/system/cpu/cpu?/cpufreq/scaling_cur_freq

			2. Unity绑核默认策略可能会遇到的问题
			a.问题描述：部分高端芯片平台（1+3+4架构），可能因Unity 0.85的阈值的设置，只把CPU7作为大核，导致逻辑线程和渲染线程等均在CPU7运行，造成了功耗过高的结果。

			b.问题现象：Systrace可以看到，主要线程都被安排在CPU7上，CPU456处于空闲状态。

			这种情况会导致，逻辑和渲染线程之间互相抢占，增加掉帧的风险。

			大多情况大核能耗整体会高于中核，造成功耗浪费。

			c.可能触发条件：

			-        大核+中核+小核架构，且中核能力也很强，中核就可以满足游戏需求。

			-        游戏的逻辑和渲染线程在同一核上抢占

			-        手机系统层面没有解绑线程并重新绑定线程的调度策略

			3. 修改建议
			a. 游戏存在逻辑线程渲染线程等多个线程，建议不要将逻辑渲染等放在一个线程，可以放在中核或中大核上

			b. 在Unity给出现如上问题的机型加上添加白名单，根据问题机型0.85的阈值降低，最终将使得中核和大核都被认定为大核。
- 广州阿里 一面
	- xgf
		- lua的next是返回的什么
			- next会去访问index吗
		- 元方法index如果是个函数怎么办
		- lua gc的时候标记对象为灰色是从哪里开始递归的
		- 小兵类似帝国时代那种寻路应该怎么做？（回答了rvo和流场寻路，rvo性能上如何优化）
		- 天空盒 半透明 不透明的一个渲染顺序
		- luajit
- 广州字节 二面 xgf
	- c# lambda表达式的优缺点，作用域
	- 为什么会有shader变体？变体最后会编译成什么？shader_feature 与 multi_compile的区别？
	- 为什么要做Linear到Gamma的转换
	- 多次DrawMesh和DrawMeshInstance有什么区别，主要优化了哪里？
	- 渲染管线层面有没有做什么优化？
	- 描述一下渲染流水线；曲面细分着色器在移动端支持不？
	- c#、lua、python这几种语言在使用的过程中有什么不同？
	- 讲一下项目里的网络层的收发包过程，你们网络层是写在c++的，那通过pb反序列化后，是怎么转成lua table的？
	- 讲一下你了解的光照模型
- 米哈游 崩坏ip在研 xgf
	- 40分钟项目, 20分钟八股,30小时手撸代码:循环队列