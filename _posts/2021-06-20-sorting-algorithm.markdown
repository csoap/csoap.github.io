---
layout:     post
title:      "常用排序,查找算法汇总"
subtitle:   ""
date:       2021-06-20
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 算法
---

- 环境
    - 语言:C#

- 20 个最常用的、最基础数据结构与算法
    - 10 个数据结构：数组、链表、栈、队列、散列表、二叉树、堆、跳表、图、Trie 树
    - 10个算法：递归、排序、二分查找、搜索、哈希算法、贪心算法、分治算法、回溯算法、动态规划、字符串匹配算法

- 栈,用数组实现的栈叫做顺序栈,用链表实现的栈叫做链式栈
    ```csharp
    //数组实现的栈
    class ArrayStack
    {
        private string[] items; // 数组
        private int count; // 栈中元素个数
        private int max;
        public ArrayStack(int n)
        {
            items = new string[n];
            count = 0;
            max = n;
        }

        public bool Push(string item)
        {
            // 空间不够,入栈失败,可以做拓容处理
            if (count == max) return false;
            items[count] = item;
            ++count;
            return true;
        }

        public string Pop()
        {
            if (count == 0) return null;
            string tmp = items[count - 1];
            --count;
            return tmp;
        }
    }
    ```
- 队列,用数组实现的
    ```csharp
    // 用数组实现的队列
    public class ArrayQueue {
        // 数组：items，数组大小：n
        private String[] items;
        private int n = 0;
        // head 表示队头下标，tail 表示队尾下标
        private int head = 0;
        private int tail = 0;
        // 申请一个大小为 capacity 的数组
        public ArrayQueue(int capacity) {
            items = new String[capacity];
            n = capacity;
        }

        // 入队操作，将 item 放入队尾
        // 当队列的 tail 指针移动到数组的最右边后，如果有新的数据入队，我们可以将 head 到 tail 之间的数据，整体搬移到数组中 0 到 tail-head 的位置
        // 避免数据搬移还可以用到循环队列,循环队列判断队列满的公式:，(tail+1)%max=head
        public boolean enqueue(String item) {
            // tail == n 表示队列末尾没有空间了
            if (tail == n) {
                // tail ==n && head==0，表示整个队列都占满了
                if (head == 0) return false;
                // 数据搬移
                for (int i = head; i < tail; ++i) {
                    items[i-head] = items[i];
                }
                // 搬移完之后重新更新 head 和 tail
                tail -= head;
                head = 0;
            }
            items[tail] = item;
            ++tail;
            return true;
        }
        // 出队
        public String dequeue() {
            // 如果 head == tail 表示队列为空
            if (head == tail) return null;
            String ret = items[head];
            ++head;
            return ret;
        }
    }
    ```
    - 循环队列
        - 首尾相连，扳成了一个环
    - 阻塞队列
        - 基于阻塞队列实现的"生产者 - 消费者模型"
        - 队列基础上增加了阻塞操作。简单来说，就是在队列为空的时候，从队头取数据会被阻塞。因为此时还没有数据可取，直到队列中有了数据才能返回；如果队列已经满了，那么插入数据的操作就会被阻塞，直到队列中有空闲位置后再插入数据，然后再返回
    - 并发队列
        - 线程安全的队列
        - 直接在 enqueue()、dequeue()方法上加锁，但是锁粒度大并发度会比较低，同一时刻仅允许一个存或者取操作

- 跳表
    ![排序时空复杂度](/img/in-post/post-js-version/sort/skiplist.png)
    - what?
        - 链表加多级索引的结构
        - 基于单链表实现二分查找
        - 空间换时间
    - 规律
        - 第一级索引的结点个数大约就是 n/2，第二级索引的结点个数大约就是 n/4,第 k级索引结点的个数就是 n/(2^k)
        - 从而求得 h=log2n-1。如果包含原始链表这一层，整个跳表的高度就是 **log2n**
        - 时间复杂度就是O(m*logn),其中m = 3,每一级索引都最多只需要遍历 3 个结点,所以**时间复杂度是O(logn)**
        - 跳表这个动态数据结构，不仅支持查找操作，还支持动态的插入、删除操作，而且插入、删除操作的时间复杂度也是 O(logn)
    - 当链表的长度 n 比较大时，比如 1000、10000 的时候，在构建索引之后，查找效率的提升就会非常明显
    - 应用环境
        - redis的有序集合
            - 核心操作
                - 插入一个数据,删除一个数据,查找一个数据,按照区间查找数据,迭代输出有序序列
    - 空间复杂度
        - 结点总和就是 n/2+n/4+n/8…+8+4+2=n-2, 空间复杂度是O(n)
    - 如何降低索引占用空间
        - 前面都是每两个结点抽一个结点到上级索引，如果我们每三个结点或五个结点，抽一个结点到上级索引,就不用那么多索引结点了
    - 跳表索引动态更新
        - what?
            - 当我们不停地往跳表中**插入**数据时，如果我们不更新索引，就有可能出现某 2 个索引结点之间数据非常多的情况。极端情况下，跳表还会退化成单链表
                - 跳表是通过随机函数来维护“平衡性
                    - 随机函数的选择从概率上来讲，能够保证跳表的索引大小和数据大小平衡性，不至于性能过度退化
                    - 随机函数 怎么实现?具体要看阿奎那代码
    - 对比 红黑树(两者都是动态数据结构)
        - 很多编程语言中的Map 类型都是通过红黑树来实现的
        - 插入、删除、查找以及迭代输出有序序列这几个操作，红黑树也可以完成，时间复杂度跟跳表是一样的。但是，按照区间来查找数据这个操作，红黑树的效率没有跳表高
        - 为了代码的简单、易读，比起红黑树，我们更倾向用跳表
- 散列表(hash table)
    - 应用场景
        - Word 文档中单词拼写检查功能是如何实现的？
            - 常用的英文单词有 20 万个左右，假设单词的平均长度是 10 个字母，平均一个单词占用 10 个字节的内存空间，那 20 万英文单词大约占 2MB 的存储空间，就算放大 10 倍也就是 20MB.这个大小完全可以放在内存里面。所以我们可以用散列表来存储整个英文单词词典
            - 当用户输入某个英文单词时，我们拿用户输入的单词去散列表中查找。如果查到，则说明拼写正确；如果没有查到，则说明拼写可能有误，给予提示
        - 假设我们有 10 万条 URL 访问日志，如何按照访问次数给 URL 排序？
            - 遍历 10 万条数据，以 URL 为 key，访问次数为 value，存入散列表，同时记录下访问次数的最大值 K，时间复杂度 O(N)。
            - 如果 K 不是很大，可以使用桶排序，时间复杂度 O(N)。如果 K 非常大（比如大于 10 万），就使用快速排序，复杂度 O(NlogN)。
        - 有两个字符串数组，每个数组大约有 10 万条字符串，如何快速找出两个数组中相同的字符
            - 以第一个字符串数组构建散列表，key 为字符串，value 为出现次数。再遍历第二个字符串数组，以字符串为 key 在散列表中查找，如果 value 大于零，说明存在相同字符串。时间复杂度 O(N)
串？
    - what
        - 散列表用的是数组支持按照下标随机访问数据的特性
        - 散列函数可以把它定义成hash(key)，其中 key 表示元素的键值，hash(key) 的值表示经过散列函数计算得到的散列值
    - 散列冲突
        - 开放寻址法
            - 如果出现了散列冲突，我们就重新探测一个空闲位置，将其插入
                - 线性探测
                    - 插入
                        - x 经过 Hash 算法之后，被散列到位置下标为 7 的位置，但是这个位置已经有数据了，所以就产生了冲突。于是就顺序地往后一个一个找，看有没有空闲的位置，遍历到尾部都没有找到空闲的位置，于是我们再从表头开始找，直到找到空闲位置插入
                    - 查找元素的过程类似插入过程
                        - 如果相等，则说明就是我们要找的元素；否则就顺序往后依次查找。如果遍历到数组中的第一个出现的空闲位置，还没有找到，就说明要查找的元素并没有在散列表中
                    - 删除
                        - 不能单纯地把要删除的元素设置为空,会导致查找失败
                        - 将删除的元素，特殊标记为 deleted。当线性探测查找的时候，遇到标记为 deleted的空间，并不是停下来，而是继续往下探测
                - 二次探测,跟线性探测很像,它探测的下标序列就是 hash(key)+0，hash(key)+1^2 ，hash(key)+2^2……
                - 双重散列
                    - 使用一组散列函数 hash1(key)，hash2(key)，hash3(key)……我们先用第一个散列函数，如果计算得到的存储位置已经被占用，再用第二个散列函数，依次类推，直到找到空闲的存储位置
                - 问题:散列表中插入的数据越来越多时，散列冲突发生的可能性就会越来越大，空闲位置会越来越少，线性探测的时间就会越来越久.最坏情况下,查找和删除操作时间复杂度O(n)
                    - 如何解决?
                        - 散列表的装载因子 = 填入表中的元素个数 / 散列表的长度
                        - 装载因子越大，说明空闲位置越少，冲突越多，散列表的性能会下降
        - 链表法

        - 开放寻址法 和 链表法 对比
            - 开放寻址法
                - 开放寻址法,数据都在数组,有效利用CPU 缓存加快查询速度
                - 当数据量比较小、装载因子小的时候，适合采用开放寻址法
            - 链表法
                - 链表法对内存的利用率比开放寻址法要高, 对大装载因子的容忍度更高
                - 链表因为要存储指针，所以对于比较小的对象的存储，是比较消耗内存的，还有可能会让内存的消耗翻倍
                - 链表中的结点是零散分布在内存中的，不是连续的，所以对 CPU 缓存是不友好
                - 基于链表的散列冲突处理方法比较适合存储大对象、大数据量的散列表，而且，比起开放寻址法，它更加灵活，支持更多的优化策略，比如用红黑树代替链表。
                    - 在 JDK1.8 版本中，为了对 HashMap 做进一步优化，我们引入了红黑树。而当链表长度太长（默认超过 8）时，链表就转换为红黑树。我们可以利用红黑树快速增删改查的特点O(logn)，来避免散列表时间复杂度退化成 O(n),提高HashMap 的性能
    - 如何设计一个可以应对各种异常情况的工业级散列表，来避免在散列冲突的情况下，散列表性能的急剧下降，并且能抵抗散列碰撞攻击？
        - 散列函数
            - 散列函数的设计不能太复杂,过于复杂的散列函数，势必会消耗很多计算时间
            - 散列函数生成的值要尽可能随机并且均匀分布
        - 装载因子过大
            - 动态扩容
                - 重新申请一个更大的散列表，将数据搬移到这个新散列表中,需要通过散列函数重新计算每个数据的存储位置
                - 随着数据的删除，散列表中的数据会越来越少，空闲空间会越来越多。如果我们对空间消耗非常敏感，我们可以在装载因子小于某个值之后，启动动态缩容。如果更加在意执行效率，能够容忍多消耗一点内存空间，那就可以不用费劲来缩容
            - 如果散列表当前大小为 1GB，要想扩容为原来的两倍大小，那就需要对1GB 的数据重新计算哈希值，并且从原来的散列表搬移到新的散列表，听起来就很耗时
                - 我们可以将扩容操作穿插在插入操作的过程中，分批完成。当装载因子触达阈值之后，我们只申请新空间，但并不将老的数据搬移到新散列表中。
                - 当有新数据要插入时，我们将新数据插入新散列表中，并且从老的散列表中拿出一个数据放入到新散列表。每次插入一个数据到散列表，我们都重复上面的过程。经过多次插入操作之后，老的散列表中的数据就一点一点全部搬移到新散列表中了
                - 搬迁期间的查询操作,为了兼容了新、老散列表中的数据，我们先从新散列表中查找，如果没有找到，再去老的散列表中查找


    - 散列表和链表结合
        - why?
            - 因为散列表是动态数据结构，不停地有数据的插入、删除，所以每当我们希望按顺序遍历散列表中的数据的时候，都需要先排序，那效率势必会很低。为了解决这个问题，我们将散列表和链表（或者跳表）结合在一起使用

        ![排序时空复杂度](/img/in-post/post-js-version/sort/linkedHashMap.png)

        - 假设猎聘网有 10 万名猎头，每个猎头都可以通过做任务（比如发布职位）来积累积分，然后通过积分来下载简历。假设你是猎聘网的一名工程师，如何在内存中存储这 10 万个猎头ID 和积分信息，让它能够支持这样几个操作
            - 根据猎头的 ID 快速查找、删除、更新这个猎头的积分信息
            - 查找积分在某个区间的猎头 ID 列表

            - Answer:以猎头 ID 构建一个散列表,再以积分排序构建一个跳表
                - ID 在散列表中所以可以 O(1) 查找到这个猎头
                - 跳表支持区间查询
- 二叉树

    ![树术语](/img/in-post/post-js-version/sort/tree.png)

    - 树 二叉树
    - 平衡二叉查找树 红黑树
    - 递归树

    - 思考 二叉树有哪几种存储方式？什么样的二叉树适合用数组来存储？
    - 如何存储一棵二叉树
        - 基于指针或者引用的二叉链式存储法
        - 基于数组的顺序存储法(数组顺序存储的方式比较适合完全二叉树)
            - 下标为i的节点,左子节点存储在下标 2 * i ,右子节点存储在 2 * i + 1,下标为 i/2 是它的父节点
            - **完全二叉树仅仅“浪费”了一个下标为 0 的存储位置**。如果是非完全二叉树，会浪费比较多的数组存储空间
            - 如果从0开始存储,下标为i的节点,左子节点存储在下标 2 * i + 1 ,右子节点存储在 2 * i + 2,下标为 (i-1)/2 是它的父节点,多了计算,为了节约计算时间从1开始比较好
                - 堆其实就是一种完全二叉树，最常用的存储方式就是数组
            ![树的数组存储](/img/in-post/post-js-version/sort/tree_array.png)
    - 二叉树遍历
        - 前序遍历
        - 中序遍历
            - 中序遍历二叉查找树，可以输出有序的数据序列，时间复杂度是 O(n)，非常高效
        - 后续遍历

        ```csharp
        // 前序
        public void PreOrder(Node root) {
            if (root == null) return;
            Debug.Log(root); //伪代码
            PreOrder(root.left);
            PreOder(root.right);
        }

        ```

        - 广度优先 深度优先遍历
    - 二叉查找树
        - 二叉查找树要求，在树中的任意一个节点，其左子树中的每个节点的值，都要小于这个节点的值，而右子树节点的值都大于这个节点的值
        - 操作是插入、删除还是查找，**时间复杂度**其实都跟树的高度成正比，也就是 O(height), 树的高度就等于最大层数减一
            - 在二叉查找树中，查找、插入、删除等很多操作的时间复杂度都跟树的高度成正比。两个极端情况的时间复杂度分别是 O(n) 和 O(logn)，分别对应二叉树**退化成链表的情况和完全二叉树**
            - 极端情况下,根节点的左右子树极度不平衡，已经退化成了链表，所以查找的时间复杂度就变成了 O(n)
            - 如何解决?
                - 平衡二叉查找树,插入、删除、查找操作的时间复杂度比较稳定，是 O(logn)
        ```csharp
        public class BinarySearchTree
        {
            private Node tree;

            public class Node
            {
                public int data;
                public Node left;
                public Node right;
                public Node(int data)
                {
                    this.data = data;
                }
            }

            public Node Find(int data)
            {
                Node p = tree;
                while (p != null)
                {
                    if (data < p.data) p = p.left;
                    else if (data > p.data) p = p.right;
                    else return p;
                }
                return null;
            }

            public void Insert(int data)
            {
                if (tree == null)
                {
                    tree = new Node(data);
                    return;
                }
                Node p = tree;
                while (p != null)
                {
                    if (data > p.data)
                    {
                        // 为空才插入
                        if (p.right == null) {
                            p.right = new Node(data);
                            return;
                        }
                        p = p.right;
                    }
                    else
                    {
                        if (p.left == null)
                        {
                            p.left = new Node(data);
                            return;
                        }
                        p = p.left;
                    }
                }
            }

            //针对要删除节点的子节点个数的不同，我们需要分三种情况来处理
            // 1.删除的节点没有子节点,只需要直接将父节点中，指向要删除节点的指针置为 null
            // 2.删除的节点只有一个子节点（只有左子节点或者右子节点），我们只需要更新父节点中，指向要删除节点的指针，让它指向要删除节点的子节点就可以了
            // 3.删除的节点有两个子节点,需要找到这个节点的右子树中的最小节点，把它替换到要删除的节点上。然后再删除掉这个最小节点
            public void Delete(int data)
            {
                Node p = tree; //p 指向要删除的节点，初始化指向根节点
                Node pp = null; //pp 记录的是p的父节点
                while (p != null && p.data != data)
                {
                    pp = p;
                    if (data > p.data) p = p.right;
                    else p = p.left;
                }
                if (p == null) return; //没找到
                //要删除的节点有两个子节点
                if (p.left != null && p.right != null)
                {
                    Node minP = p.right;
                    Node minPP = p; // minPP 表示 minP 的父节点
                    while (minP.left != null)
                    {
                        minPP = minP;
                        minP = minP.left;
                    }
                    p.data = minP.data;
                    p = minP;
                    pp = minPP;
                }

                // 删除节点是叶子节点或是仅有一个子节点
                Node child; // p的子节点
                if (p.left != null) child = p.left;
                else if (p.right != null) child = p.right;
                else child = null;

                if (pp == null) tree = child; // 删除的是根节点
                else if (pp.left == p) pp.left = child;
                else pp.right = child;
            }
        }
        ```

        ![二叉查找树删除节点](/img/in-post/post-js-version/sort/tree_delete.png)

        - 前面讲的二叉查找树的操作，针对的都是不存在键值相同的情况。那如果存储的两个对象键值相同,如何解决
            - 通过链表和支持动态扩容的数组等数据结构，把值相同的数据都存储在同一个节点上
            - 把这个新插入的数据当作大于这个节点的值来处理
                - 查找数据的时候，遇到值相同的节点，我们并不停止查找操作，而是继续在右子树中查找，直到遇到叶子节点，才停止。这样就可以把键值等于要查找值的所有节点都找出来.删除同样操作
    -  散列表和二叉查找树的对比
        - 散列表的插入、删除、查找操作的时间复杂度可以做到常量级的O(1)
        - 二叉查找树在比较平衡的情况下，插入、删除、查找操作时间复杂度才是O(logn)
        - 散列表中的数据是无序存储的，如果要输出有序的数据，需要先进行排序。对于二叉查找树，只需要中序遍历，就可以在 O(n) 的时间复杂度内，输出有序的数据序列
        - 散列表扩容耗时很多，而且当遇到散列冲突时，性能不稳定，尽管二叉查找树的性能不稳定，但是在工程中，我们最常用的平衡二叉查找树的性能非常稳定，时间复杂度稳定在O(logn)
        - 散列表的查找等操作的时间复杂度是常量级的，但因为哈希冲突的存在，这个常量不一定比 logn 小，所以实际的查找速度可能不一定比 O(logn) 快。加上哈希函数的耗时，也不一定就比平衡二叉查找树的效率高
        - 散列表的构造比二叉查找树要复杂，需要考虑的东西很多。比如散列函数的设计、冲突解决办法、扩容、缩容等。平衡二叉查找树只需要考虑平衡性这一个问题
        - 为了避免过多的散列冲突，散列表装载因子不能太大，特别是基于开放寻址法解决冲突的散列表，不然会浪费一定的存储空间
    
    - avl 就是平衡二叉树
        - 视频:https://www.bilibili.com/video/BV1xE411h7dd
        - 平衡调整步骤
            - 找平衡因子 = 2
            - 找插入新节点后失去平衡的最小子树
                - 要求
                    - 距离插入节点最近
                    - 平衡因子绝对值大于1的结点作为根
            - 平衡调整
                - 四种情况 -对应调整方法
                    - LL型 -R 右旋(中为支,高右转)
                    - RR型 -L 左旋(中为支,高左转)
                    - LR型 -LR (下二整天先左转, 后与LL同)
                    - RL型 -RL 下二整体先右转,后与RR同)

    - 平衡二叉查找树
        - what?
            - 满足二叉查找树的前提下
            - 还满足**任意一个节点的左右子树的高度相差不能大于1**,
        - why?
            - 发明平衡二叉查找树这类数据结构的初衷是，解决普通二叉查找树在频繁的插入、删除等动态更新的情况下，出现时间复杂度退化的问题
            - 平衡二叉查找树中“平衡”的意思，其实就是让整棵树左右看起来比较“平衡”，这样就能让整棵树的高度相对来说低一些，相应的插入、删除、查找等操作的效率高一些
    - 红黑树
        - https://www.cnblogs.com/tiancai/p/9072813.html
        - 一种**不严格**的平衡二叉查找树,高度差不一定
        - 红黑树是“近似平衡”的,近似平衡”就等价为性能不会退化的太严重,
            - 如何证明平衡?红黑树的高度是否比较稳定地趋近log n 
        - what
            - 红黑树中的节点，一类被标记为黑色，一类被标记为红色
            - 满足要求
                - 根节点是黑色的
                - 每个叶子节点都是黑色的空节点（NIL），也就是说，叶子节点不存储数据
                - 任何相邻的节点都不能同时为红色，也就是说，红色节点是被黑色节点隔开的
                - 每个节点，从该节点到达其可达叶子节点的所有路径，都包含相同数目的黑色节点
    - 散列表 跳表 红黑树 对比
        - 散列表：插入删除查找都是O(1), 是最常用的，但其缺点是不能顺序遍历以及扩容缩容的性能损耗。适用于那些不需要顺序遍历，数据更新不那么频繁的。
        - 跳表：插入删除查找都是O(logn), 并且能顺序遍历。缺点是空间复杂度O(n)。适用于不那么在意内存空间的，其顺序遍历和区间查找非常方便。
        - 红黑树：插入删除查找都是O(logn), 中序遍历即是顺序遍历，稳定。缺点是难以实现，去查找不方便。其实跳表更佳，但红黑树已经用于很多地方了

- 堆
    - what
        - 完全二叉堆
        - 堆中每一个节点的值都必须大于等于（或小于等于）其子树中每个节点的值
    - 如何存储一个堆
        - 完全二叉树比较适合用数组来存储。用数组来存储完全二叉树是非常节省存储空间的。因为我们不需要存储左右子节点的指针，单纯地通过数组的下标，就可以找到一个节点的左右子节点和父节点
    - 堆支持的常见操作
        - 插入一个元素
        - 删除堆顶元素

        ```csharp
        public class Heap
        {
            private int[] a; // 数组, 从下标1开始存储数据
            private int n; // 堆可以存储的最大数据个数
            private int count; // 堆已存储的数据个数

            // 假设此堆是大顶堆
            public Heap(int capiticy)
            {
                a = new int[capiticy + 1];
                n = capiticy;
                count = 0;
            }

            public void Insert(int data)
            {
                if (count >= n) return; //堆满了
                ++count;
                a[count] = data;
                int i = count;
                while (i /2 > 0 && a[i] > a[i / 2]) // 自下往上堆化
                {
                    Swap(a, i, i / 2);
                    i = i / 2;
                }
            }
            // 删除堆顶元素
            //把最后一个节点放到堆顶，然后利用同样的父子节点对比方法。对于不满足父子节点大小关系的，互换两
            //个节点，并且重复进行这个过程，直到父子节点之间满足大小关系为止。这就是从上往下的堆化方法
            public void RemoveMax()
            {
                if (count == 0) return ;
                a[1] = a[count];
                --count;
                Heapify(a, count, 1);
            }

            public void Heapify(int[] a, int n, int i)
            {
                // 自上往下堆化
                while (true)
                {
                    int maxPos = i;
                    if (i * 2 <= n && a[i] < a[i * 2]) maxPos = i * 2;
                    if (i * 2 + 1 <= n && a[maxPos] < a[i * 2 + 1]) maxPos = i * 2 + 1;
                    if (maxPos == i) break;
                    Swap(a, i, maxPos);
                    i = maxPos;
                }

            }
        }
        ```
    - 堆的应用
        - 优先级队列
            - 合并有序小文件
                - 假设我们有 100 个小文件，每个文件的大小是 100MB，每个文件中存储的都是有序的字符串。我们希望将这些 100 个小文件合并成一个有序的大文件
                - 将从小文件中取出来的字符串放入到小顶堆中，那堆顶的元素，也就是优先级队列队首的元素，就是最小的字符串。我们将这个字符串放入到大文件中，并将其从堆中删除。然后再从小文件中取出下一个字符串，放入到堆中。循环这个过程，就可以将 100 个小文件中的数据依次放入到大文件中
            - 高性能定时器
                - 假设我们有一个定时器，定时器中维护了很多定时任务，每个任务都设定了一个要触发执行的时间点。定时器每过一个很小的单位时间（比如 1 秒），就扫描一遍任务，看是否有任务到达设
                - 效率低两个原因
                    - 任务的约定执行时间离当前时间可能还有很久,前面的多次扫描是多余的
                    - 每次都要扫描整个任务列表，如果任务列表很大的话,耗时
                - 解决
                    - 用优先级队列来解决。我们按照任务设定的执行时间，将这些任务存储在优先级队列中，队列首部（也就是小顶堆的堆顶）存储的是最先执行的任务
                    - 拿队首任务的执行时间点，与当前时间点相减，得到一个时间间隔 T,从当前时间点到（T-1）秒这段时间里，定时器都不需要做任何事情
        - 求 Top K 和求中位数
            - TOPK
                - 静态数据(数据集合事先确定，不会再变)
                    - 维护一个大小为 K 的小顶堆，顺序遍历数组，从数组中取出取数据与堆顶元素比较。如果比堆顶元素大，就把堆顶元素删除，并且将这个元素插入到堆中；如果比堆顶元素小，则不做处理，继续遍历数组。这样等数组中的数据都遍历完之后，堆中的数据就是前 K 大数据了
                    - 时间复杂度O(nlogK),遍历数组需要 O(n) 的时间复杂度，一次堆化操作需要 O(logK) 的时间复杂度
                - 动态数据(数据集合事先并不确定，有数据动态地加入到集合中)
                    - 维护一个 K 大小的小顶堆，当有数据被添加到集合中时，我们就拿它与堆顶的元素对比。如果比堆顶元素大，我们就把堆顶元素删除，并且将这个元素插入到堆中；如果比堆顶元素小，则不做处理。这样，无论任何时候需要查询当前的前 K 大数据，我们都可以里立刻返回给他
            - 求中位数(允许重复数据)
                - 维护两个堆，一个大顶堆，一个小顶堆。大顶堆中存储前半部分数据，小顶堆中存储后半部分数据，且小顶堆中的数据都大于大顶堆中的数据
                - 如果新加入的数据小于等于大顶堆的堆顶元素，我们就将这个新数据插入到大顶堆；如果新加入的数据大于等于小顶堆的堆顶元素，我们就将这个新数据插入到小顶堆
                - 插入或删除的过程为了满足两个堆的数据个数约定,从一个堆中不停地将堆顶元素移动到另一个堆
                - 中位数我们只需要返回大顶堆的堆顶元素就可以了，所以时间复杂度就是 O(1)
                - 变形问题:99 百分位数据, 处理的思路都是一样,即利用两个堆，一个大顶堆，一个小顶堆，随着数据的动态添加，动态调整两个堆中的数据，最后大顶堆的堆顶元素就是要求的数据
        - 有一个包含 10 亿个搜索关键词的日志文件，如何能快速获取到热门榜 Top 10 的搜索关键词呢？
            - 环境限制:处理的场景限定为单机，可以使用的内存为 1GB
            - 因为用户搜索的关键词，有很多可能都是重复的
            - 选用散列表。我们就顺序扫描这 10 亿个搜索关键词。当扫描到某个关键词时，我们去
            散列表中查询。如果存在，我们就将对应的次数加一；如果不存在，我们就将它插入到散列表，并记录数为 1。以此类推，等遍历完这 10 亿个搜索关键词之后，散列表中就存储了不重复的搜索关键词以及出现的次数
            - 堆求 Top K 的方法，建立一个大小为 10 的小顶堆，遍历散列表，依次取出每个搜索关键词及对应出现的次数，然后与堆顶的搜索关键词对比
            - 漏洞:
                - 我们假设 10亿条搜索关键词中不重复的有 1 亿条，如果每个搜索关键词的平均长度是 50 个字节，那存储 1亿个关键词起码需要 5GB 的内存空间
            - 解决:我们创建 10 个空文件 00，01，02，……，09。我们遍历这 10 亿个关键词，并且通过某个哈希算法对其求哈希值，然后哈希值同 10 取模，得到的结果就是这个搜索关键词应该被分到的文件编号
            - 对这 10 亿个关键词分片之后，每个文件都只有 1 亿的关键词，去除掉重复的，可能就只有1000 万个，每个关键词平均 50 个字节，所以总的大小就是 500MB。1GB 的内存完全可以放得下
            - 我们针对每个包含 1 亿条搜索关键词的文件，利用散列表和堆，分别求出 Top 10，然后把这个10 个 Top 10 放在一块，然后取这 100 个关键词中，出现次数最多的 10 个关键词

- 图
    - 应用环境:微博微信等社交网络中的好友关系
        - 微博,有关注功能,引入方向概念,称为有向图
    - 涉及算法
        - 图的搜索,最短路径,最小生成树,二分图
    - 概念
        - 元素我们就叫作顶点
        - 建立的关系叫作边
            - 边带权重的图成为 带权图,如qq好友,通过这个权重来表示 QQ 好友间的亲密度
        - 每个用户有多少个好友，对应到图中，就叫作顶点的度
            - 顶点的入度，表示有多少条边指向这个顶点
            - 顶点的出度，表示有多少条边是以这个顶点为起点指向其他顶点
    - 存储方法
        - 邻接矩阵存储方法,相当于二维数组
            ![图存储](/img/in-post/post-js-version/sort/map_1.png)
            - 缺点
                - 稀疏图(顶点很多,每个顶点的边并不多)的情况下,浪费空间,比如微信有好几亿的用户，对应到图上就是好几亿的顶点。但是每个用户的好友并不会很多，一般也就三五百个而已
            - 优点
                - 邻接矩阵的存储方式简单、直接，因为基于数组，所以在获取两个顶点的关系时，就非常高效
        - 邻接表存储方法,相当于散列表
                ![图存储](/img/in-post/post-js-version/sort/map_2.png)
                - 邻接表存储起来比较节省空间，但是使用起来就比较耗时间
                - 是否存在一条从顶点 2 到顶点 4 的边，那我们就要遍历顶点 2 对应的那条链表，看链表中是否存在顶点 4
                - 基于链表法解决冲突的散列表,如果链表太长可以将链表换成其他数据结构,如平衡二叉树等(实际开发中用的是红黑树)
    - 针对微博用户关系, 如何设计
        - 社交网络是一张稀疏图, 采用邻接表存储
        - 用一个邻接表来存储这种有向图是不够的。我们去查找某个用户关注了哪些用户非常容易
        - 一个**逆邻接表**。邻接表中存储了用户的关注关系，逆邻接表中存储的是用户的被关注关系
        - 优化
            - 为了快速判断两个用户之间是否是关注与被关注的关系
            - 需要按照用户名称的首字母排序，分页来获取用户的粉丝列表或者关注列表
        - 怎么做
            - 选择用跳表代替链表,跳表插入、删除、查找都是O(logn),且跳表本身有序
        - 分片
            - 对于小规模的数据, 可以将这个社交关系存储在内存中,如果是上亿用户就需要分片
                - 哈希算法数据分片.机器 1 上存储顶点 1，2，3 的邻接表，在机器 2 上，存储顶点 4，5 的邻接表

# 排序算法

- 不错的文章链接
    - https://www.jianshu.com/p/a5bc98500cec
    - https://leetcode-cn.com/problems/sort-an-array/solution/fu-xi-ji-chu-pai-xu-suan-fa-java-by-liweiwei1419/
    - https://blog.csdn.net/weixin_41190227/article/details/86600821
    - https://www.lfzxb.top/summary-of-common-sorting-algorithms/
    - https://leetcode-cn.com/problems/sort-an-array/solution/dong-hua-mo-ni-yi-ge-po-dui-pai-wo-gao-l-i6mt/
- 前言
    - 记录常用排序算法,日后方便回顾

    - 定义测试环境代码

        ```csharp
        static void Main(string[] args)
        {
            int[] nums = { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
            Sort(nums); // 这里替换为排序方法
            PrintArr(nums);
            Console.ReadKey();
        }

        public static void PrintArr(int[] nums)
        {
            for (int i = 0; i < nums.Length; i++)
            {
                Console.WriteLine(nums[i]);
            }
        }

        public static void Swap(int[] nums, int i, int j)
        {
            int temp = nums[i];
            nums[i] = nums[j];
            nums[j] = temp;
        }
        ```

- 各种排序时空复杂度
    - n: 数据规模
    - k: “桶”的个数
    - In-place: 占用常数内存，不占用额外内存
    - Out-place: 占用额外内存

    ![排序时空复杂度](/img/in-post/post-js-version/sort/sort.png)

- 冒泡排序

    ![冒泡排序](/img/in-post/post-js-version/sort/bubble_sort.gif)

    - 要排序的数组的初始状态是 4，5，6，3，2，1，其中，有序元素对有 (4，5) (4，6) 两个，所以有序度是 2。n=6，所以排序完成之后终态的满有序度为 n*(n-1)/2=15
    - 逆序度 = 满有序度 - 有序度, 逆序度表示要进行交换的次数
    ```csharp
    public static void BubbleSort(int[] nums)
    {
        if (nums == null) return;
        int len = nums.Length;
        if (len == 0) return;
        bool flag = false; //提前退出冒泡循环的标志位
        for (int i = 0; i < len - 1; i++)
        {
            for (int j = 0; j < len - 1 - i; j++)
            {
                if (nums[j] > nums[j + 1])
                {
                    Swap(nums, j, j + 1);
                    flag = true;// 表示有数据交换
                }
            }
            if (!flag) break; // 没有数据交换，提前退出
        }
    }
    ```
    - 算法分析
        - 最佳情况 O(n),在代码中使用一个**标志位**来判断是否已经排序好
        - 最差情况 O(n^2)
        - 平均情况 O(n)
- 选择排序
    - 选择排序算法的实现思路有点类似插入排序，也分已排序区间和未排序区间。但是选择排序每次会从未排序区间中找到最小的元素，将其放到已排序区间的末尾。

    ![选择排序](/img/in-post/post-js-version/sort/select_sort.gif)

    ```csharp
    public static void SelectSort(int[] nums)
    {
        if (nums == null) return;
        int count = nums.Length;
        if (count == 1) return;
        int min;
        for (int i = 0; i < count - 1;  i++)
        {
            min = i; // 寻找后面最小的索引
            for (int j = i + 1; j < count; j++)
            {
                if (nums[min] > nums[j])
                {
                    min = j;
                }
            }
            if (min != i)
            {
                Swap(nums, i, min);
            }
        }
    }
    ```

    - 算法分析
        - 最佳情况 O(n^2)
        - 最差 O(n^2)
        - 平均 O(n^2)
        - 不稳定. 比如 5，8，5，2，9 这样一组数据，使用选择排序算法来排序的话，第一次找到最小元素 2，
与第一个 5 交换位置，那第一个 5 和中间的 5 顺序就变了，所以就不稳定了

- 插入排序
    - 联想:斗地主抓牌时候插入牌
    - 我们将数组中的数据分为两个区间，已排序区间和未排序区间。初始已排序区间只有一个
元素，就是数组的第一个元素。插入算法的核心思想是取未排序区间中的元素，在已排序区间中
找到合适的插入位置将其插入，并保证已排序区间数据一直有序。重复这个过程，直到未排序区
间中元素为空，算法结束
    ![插入排序](/img/in-post/post-js-version/sort/insert_sort.gif)

    ```csharp
    public static void InsertSort(int[] nums)
    {
        if (nums == null) return;
        int count = nums.Length;
        if (count == 1) return;
        for (int i = 1; i < count; i++)
        {
            for (int j = i; j > 0 && nums[j] < nums[j-1]; j--)
            {
                Swap(nums, j, j - 1);
            }
        }
    }

    /// 优化写法 用临时变量记录标记项,去掉Swap方法

    ```
    
    - 算法分析
        - 最佳情况 O(n), 只有外循环
        - 最差 O(n^2)
        - 平均 O(n^2)

- 希尔排序

    ![插入排序](/img/in-post/post-js-version/sort/shell_sort.png)

    ```csharp
    public static void Sort(int[] nums)
    {
        if (nums == null) return;
        int count = nums.Length;
        if (count == 1) return;
        int gap = count >> 1;
        while (1 <= gap)
        {
            // 把距离为 gap 的元素编为一个组，扫描所有组
            for (int i = gap; i < count; i++)
            {
                int preIndex;
                int temp = nums[i];
                // 对距离为 gap 的元素组进行排序
                for (preIndex = i - gap; preIndex >= 0 && temp < nums[preIndex]; preIndex -= gap)
                {
                    nums[preIndex + gap] = nums[preIndex];
                }
                nums[preIndex + gap] = temp;
            }
            gap /= 2;
        }
    }
    ```
    
    - 算法分析
        - 最佳 O(nLogn)
        - 最差 O(nLogn)
        - 平均 O(nLogn)

- 归并排序
    - https://leetcode-cn.com/problems/sort-an-array/solution/pai-xu-shu-zu-by-leetcode-solution/
    - 思路 归并排序利用了分治的思想来对序列进行排序。对一个长为 n 的待排序的序列，我们将其分解成两个长度n/2的子序列,每次先递归调用函数使两个子序列有序，然后我们再线性合并两个有序的子序列使整个序列有序。

    ![归并排序](/img/in-post/post-js-version/sort/merge_sort.png)

    ```csharp
    public static void SortArray(int[] nums)
    {
        if (nums == null || nums.Length == 1) return;
        int count = nums.Length;
        int[] tmp = new int[count];
        MergeSort(nums, 0, count - 1, tmp);
    }

    public static void MergeSort(int[] nums, int l, int r, int[] tmp)
    {
        if (l >= r) return;
        int mid = l + ((r - l) >> 1);
        MergeSort(nums, l, mid, tmp); // 左边归并排序,使得左子序列有序
        MergeSort(nums, mid + 1, r, tmp);
        // 将两个有序子数组合并
        int i = l, j = mid +1;//左序列指针,//右序列指针
        int cnt = 0;//临时数组指针
        while (i <= mid && j <= r)
        {
            if (nums[i] < nums[j])
            {
                tmp[cnt++] = nums[i++];
            }
            else
            {
                tmp[cnt++] = nums[j++];
            }
        }
        while (i <= mid)
        {
            tmp[cnt++] = nums[i++];
        }
        while (j <= r)
        {
            tmp[cnt++] = nums[j++];
        }
        cnt = 0;
        while (l <= r)
        {
            nums[l++] = tmp[cnt++];
        }
    }
    ```

    - 算法分析
        - 最佳 O(n)
        - 最差 O(nlogn)
        - 平均 O(nlogn)

- 快速排序
    - https://www.bilibili.com/video/BV1at411T75o
    - 基本思想
        - 选定Pivot中心轴
        - 将大于Pivot的数字放在Pivot的右边,将小于Pivotde的数字放在Pivot的左边
        - 分别对左右子序列重复前三步操作

    ```csharp
    public static void QuickSort(int[] nums, int low, int high)
    {
        if (low >= high) return;
        int index = SortUnit(nums, low, high); // 找寻基准数据的正确索引
        QuickSort(nums, low, index - 1);
        QuickSort(nums, index + 1, high);
    }

    public static int SortUnit(int[] array, int low, int high)
    {
        int key = array[low];//基准数
        while (low < high)
        {
            //从high往前找小于或等于key的值
            while (low < high && array[high] >= key)
                high--;
            //比key小等的放左边
            array[low] = array[high];
            //从low往后找大于key的值
            while (low < high && array[low] <= key)
                low++;
            //比key大的放右边
            array[high] = array[low];
        }
        //结束循环时，此时low等于high，左边都小于或等于key，右边都大于key。将key放在游标当前位置。
        array[low] = key;
        return high;
    }
    ```

    - 算法分析
        - 最佳 O(nlogn)
        - 最差 O(n^2)
        - 平均 O(nlogn)

    - 归并排序 和快速排序 区别
        - 归并排序的处理过程是由下到上的，先处理子问题，然后再合并。而快排正好相反，它的处理过程是由上到下的，先分区，然后再处理子问题。归并排序虽然是稳定的、时间复杂度为 O(nlogn) 的排序算法，但是它是非原地排序算法。快速排序通过设计巧妙的原地分区函数，可以实现原地排序，解决了归并排序占用太多内存的问题
        - 快排,原地,不稳定
        - 归并, 非原地,稳定
    - 快排缺点
        - 如果数据原本就是有序或接近有序,每次分区选择最后一个数据,情况时间复杂度退化为O(n^2),主要问题是选分区点不合理
    - 如何优化
        - 三数取中法
            - 从区间的首、尾、中间，分别取出一个数，然后对比大小，取这 3 个数的中间值作为分区点。这样每间隔某个固定的长度，取数据出来比较，将中间值作为分区点的分区算法，肯定要比单纯取某一个数据更好。但是，如果要排序的数组比较大，那“三数取中”可能就不够了，可能要“五数取中”或者“十数取中
        - 随机法
            - 随机法就是每次从要排序的区间中，随机选择一个元素作为分区点
            - 从概率的角度来看，也不大可能会出现每次分区点都选的很差的情况，所以平均情况下，这样选的分区点是比较好的
    -  C 语言中 qsort() 的底层实现原理
        - qsort() 会优先使用归并排序来排序输入数据，因为归并排序的空间复杂度是 O(n)，所以对于小数据量的排序，比如 1KB、2KB 等，归并排序额外需要 1KB、2KB 的内存空间，这个问题不大
        - 要排序的数据量比较大的时候，qsort() 会改为用快速排序算法来排序
        - 在快速排序的过程中，当要排序的区间中，元素的个数小于等于 4 时，qsort() 就退化为插入排序
            - 因为:在小规模数据面前，O(n ) 时间复杂度的算法并不一定比 O(nlogn) 的算法执行时间长
- 堆排序
    - https://www.bilibili.com/video/BV1K4411X7fq
    - 思路
        - 建堆(堆化),时间复杂度O(n)
            - 上浮,不断插入元素进行建堆(插入数组最后)
            - 下沉,通过下沉操作建堆效率更高，具体过程是，找到最后一个**非叶子节点**，(与堆顶交换递归),然后从后往前遍历执行下沉操作
        - 排序,时间复杂度O(nlogn)
            - 将堆顶元素（代表最大元素,数组中第一个元素）与最后一个元素交换，然后新的堆顶元素进行下沉操作，遍历执行上诉操作，则可以完成排序
    - 节点公式(假设是从数组下标为1的卫视开始存储)
        - parent(i) = i/2
        - leftChild(i) = 2*i
        - rightChild(i) = 2*i+1
    - 时间复杂度
        - 因为我们建堆的时间复杂度为 O(n），排序过程的时间复杂度为 O(nlogn),所以总的空间复杂度为 O(nlogn)
    - 空间复杂度  O(1)
    - 快速排序和堆排序对比
        - 对于快速排序来说，数据是顺序访问的。而对于堆排序来说，数据是跳着访问的。这样对 CPU 缓存是不友好的
        - 相同的的数据，排序过程中，堆排序的数据交换次数要多于快速排序

    ```csharp
        //// n 表示数据的个数，数组 a 中的数据从下标 1 到 n 的位置
        public static void SortArray(int[] a, int n)
        {
            BuildHeap(a, n);
            int k = n;
            while(k > 1) // O(nLogn)
            {
                Swap(a, 1, k);
                --k;
                Heapify(a, k, 1);
            }
        }
        //  O(logn)
        public static void BuildHeap(int[] a, int n)
        {
            // 对下标从n/2开始到1的数据进行堆化,下标是n/2+1 到n的节点是叶子节点,不需要堆化
            for (int i = n / 2; i >= 1; --i)
            {
                Heapify(a, n, i);
            }
        }

        public static void Heapify(int[] a, int n, int i)
        {
            while (true)
            {
                int maxPos = i;
                if (i * 2 <= n && a[i] < a[i * 2]) maxPos = i * 2;
                if (i * 2 + 1 <= n && a[maxPos] < a[i * 2 + 1]) maxPos = i * 2 + 1;
                if (maxPos == i) break;
                Swap(a, i, maxPos);
                i = maxPos;
            }
        }
    ```

- 计数排序
    - 核心在于将输入的数据值转化为键存储在额外开辟的数组空间中。 作为一种线性时间复杂度的排序，计数排序要求输入的数据必须是有确定范围的整数
    - 适用场景
        - 量大但是范围小
            > 计数排序只能用在数据范围不大的场景中，如果数据范围 k 比要排序的数据 n 大很多，就不适合用计数排序了。而且，计数排序只能给非负整数排序，如果要排序的数据是其他类型的，要将其在不改变相对大小的情况下，转化为非负整数

            > 拿考生这个例子。如果考生成绩精确到小数后一位，我们就需要将所有的分数都先乘以 10，转化成整数，然后再放到 9010 个桶内。再比如，如果要排序的数据中有负数，数据的范围是 [-1000, 1000]，那我们就需要先对每个数据都加 1000，转化成非负整数
            - 某大型企业数万员工年龄排序
            - 如何得到高考名次
    - 步骤
        - 步骤1：找出待排序的数组中最大和最小的元素；
        - 步骤2：统计数组中每个值为i的元素出现的次数，存入数组C的第i项；
        - 步骤3：对所有的计数累加（从C中的第一个元素开始，每一项和前一项相加）；
        - 步骤4：反向填充目标数组：将每个元素i放在新数组的第C(i)项，每放一个元素就将C(i)减去1。

    ![计数排序](/img/in-post/post-js-version/sort/count_sort.gif)

    ```csharp
    public static void CountSort(int[] nums)
    {
        int len = nums.Length;
        if (len < 1) return;
        int min = int.MaxValue, max = 0, index = 0;
        for (int i = 0; i < len; i++)
        {
            if (nums[i] > max) max = nums[i];
            if (nums[i] < min) min = nums[i];
        }
        int[] c = new int[max - min + 1];
        for (int i = 0; i < len; i++)
        {
            c[nums[i] - min]++;
        }
        for (int i = 0; i < c.Length; i++)
        {
            for (int j = 0; j < c[i]; j++)
            {
                nums[index++] = i + min;
            }
        }
    }
    ```

    - 空间复杂度 N+K(N:原数组,K计数数组)
    - 时间复杂度 N+K

- 桶排序
    - 桶排序 是计数排序的升级版。它利用了函数的映射关系，高效与否的关键就在于这个映射函数的确定
    - 原理
        - 假设输入数据服从均匀分布，将数据分到有限数量的桶里，每个桶再分别排序（有可能再使用别的排序算法,如快排

    ![桶排序](/img/in-post/post-js-version/sort/bucket_sort.png)

    ```csharp
    public static void BucketSort(int[] arr)
    {
        int len = arr.Length;
        if (len < 1) return;
        int length = arr.Length;
        // 计算最大值与最小值
        int max = int.MinValue;
        int min = int.MaxValue;
        for (int i = 0; i < length; i++)
        {
            max = Math.Max(max, arr[i]);
            min = Math.Min(min, arr[i]);
        }

        // 计算桶的数量
        int bucketNum = (max - min) / length + 1;
        List<List<int>> bucketArr = new List<List<int>>(bucketNum);
        for (int i = 0; i < bucketNum; i++)
        {
            bucketArr.Add(new List<int>());
        }

        // 将每个元素放入桶
        for (int i = 0; i < length; i++)
        {
            int num = (arr[i] - min) / (length);
            bucketArr[num].Add(arr[i]);
        }

        // 对每个桶进行排序
        for (int i = 0; i < bucketArr.Count; i++)
        {
            bucketArr[i].Sort();
        }

        // 将桶中的元素赋值到原序列
        int index = 0;
        for (int i = 0; i < bucketArr.Count; i++)
        {
            for (int j = 0; j < bucketArr[i].Count; j++)
            {
                arr[index++] = bucketArr[i][j];
            }
        }
    }
    ```
    - 算法分析
    - 最佳情况：T(n) = O(n+k)
    - 最差情况：T(n) = O(n+k)
    - 平均情况：T(n) = O(n2)

- 基数排序
    - 基本思想
        - 基数排序是按照低位先排序，然后收集；再按照高位排序，然后再收集；依次类推，直到最高位。有时候有些属性是有优先级顺序的，先按低优先级排序，再按高优先级排序。最后的次序就是高优先级高的在前，高优先级相同的低优先级高的在前。
        - 有时候要排序的数据并不都是等长的, 可以把所有的单词补齐到相同长度,如单词比较,位数不够的可以在后面补 a，因为字母中最小的是 a
    - 适用范围
        - 每一位的数据范围不能太大
        - 需要可以分割出独立的“位”来比较，而且位之间有递进的关系，如果 a 数据的高位比 b 数据大，那剩下的低位就不用比较了
        - 如:假设我们有 10 万个手机号码，希望将这 10 万个手机号码从小到大排序
        > 规律:假设要比较两个手机号码 a，b 的大小，如果在前面几位中，a手机号码已经比 b 手机号码大了，那后面的几位就不用看了
    - 步骤
        - 取得数组中的最大数，并取得位数
        - arr为原始数组，从最低位开始取每个位组成radix数组
        - 对radix进行计数排序（利用计数排序适用于小范围数的特点）
    
    ![基数排序](/img/in-post/post-js-version/sort/radix_sort.gif)

    ```csharp
    public static void Sort(int[] arr)
    {
        int length = arr.Length;
        //待排序列最大值
        int max = arr[0];
        int exp;//指数

        //计算最大值
        for (int i = 0; i < length; i++)
        {
            if (arr[i] > max)
            {
                max = arr[i];
            }
        }

        //从个位开始，对数组进行排序
        for (exp = 1; max / exp > 0; exp *= 10)
        {
            //存储待排元素的临时数组
            int[] temp = new int[length];
            //分桶个数
            int[] buckets = new int[10]; // 0~10

            //将数据出现的次数存储在buckets中
            for (int i = 0; i < length; i++)
            {
                //(value / exp) % 10 :value的最底位(个位)
                buckets[(arr[i] / exp) % 10]++;
            }

            //更改buckets[i]，
            for (int i = 1; i < 10; i++)
            {
                buckets[i] += buckets[i - 1];
            }

            //将数据存储到临时数组temp中
            for (int i = length - 1; i >= 0; i--)
            {
                temp[buckets[(arr[i] / exp) % 10] - 1] = arr[i];
                buckets[(arr[i] / exp) % 10]--;
            }

            //将有序元素temp赋给arr
            Array.Copy(temp, 0, arr, 0, length);
        }
    }
    ```
    - 算法分析
        - 时间复杂度O(n*k)
    - 基数排序 vs 计数排序 vs 桶排序
        - 三种排序算法都利用了桶的概念，但对桶的使用方法上
            - 基数排序： 根据键值的每位数字来分配桶
            - 计数排序： 每个桶只存储单一键值
            - 桶排序： 每个桶存储一定范围的数值

# 查找算法

- 不错的文章链接
    - https://cloud.tencent.com/developer/article/1407004
    - https://xie.infoq.cn/article/996cf8899930ae467cc790035
    - https://www.cnblogs.com/maybe2030/p/4715035.html
- 前言
    - 记录常用查找算法,日后方便回顾

- 查找算法分类
    - 静态查找 动态查找
        - 静态或者动态都是针对查找表而言的。动态表指查找表中有删除和插入操作的表。
    - 无需查找 有序查找

- 顺序查找
    - 时间复杂度为O(n)。
    ```csharp
    public static int OrderSearch(int[] array, int value)
    {
        if (array == null || array.Length < 1)
            return -1;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == value)
                return i;
        }
        return -1;
    }
    ```

- 二分查找
    - 应用场景
        - 如何用最**省内存**的方式实现快速查找
        - 二分查找只能用在插入、删除操作不频繁，一次排序多次查找的场景中。针对动态变化的数据集合，二分查找将不再适用,推荐使用**二叉树**
        - 假设我们有 1000 万个整数数据，每个数据占 8 个字节，如何设计数据结构和算法，快速判断某个整数是否出现在这 1000 万数据中？ 我们希望这个功能不要占用太多的内存空间，最多不要超过 100MB
            - 先排序,然后直接二分查找
    - 前提
        - 元素必须是有序,如果无序则要先进行排序 O(nlogn)
    - 注意要点
        - 记得考虑有没有**重复数据**
        - 变形问题
            - 查找**第一个等于**给定值的元素,最后一个等于,第一个大于等于,最后一个小于,这些变形都要能熟练手写

    - 基本思想
        - 用给定值k先与中间结点的关键字比较，中间结点把线形表分成两个子表，若相等则查找成功；若不相等，再根据k与该中间结点关键字的比较结果确定下一步查找哪个子表，这样递归进行，直到查找到或查找结束发现表中没有这样的结点。
        > 二分查找的前提条件是需要有序表顺序存储，对于静态查找表，一次排序后不再变化，折半查找能得到不错的效率。但对于需要**频繁执行插入或删除操作的数据集**来说，维护有序的排序会带来不小的工作量，那就**不建议使用**。
    - 时间复杂度 O(log2n)

    ```csharp
    // 非递归,没有重复数据
    public static int BinarySearch(int[] array, int value)
    {
        if (array == null || array.Length < 1)
            return -1;
        int low = 0;
        int high = array.Length - 1;
        int mid;
        while (low <= high) // 循环退出条件注意是 low<=high，而不是 low<high
        {
            mid = low +( (high - low)  >> 1); //low+(high-low)/2, 为什么(low +high) / 2会溢出啊？答：两个很大的int相加的话超出 Integer.MAX_VALUE 了
            if (array[mid] == value)
            {
                return mid;
            }
            if (array[mid] > value)
            {
                high = mid - 1; // 如果写成high = mid,可能造成无限循环
            }
            else
            {
                low = mid + 1; // low = mid,可能造成无限循环
            }
        }
        return -1;
    }

    //递归版本,没有重复数据
    public static int BinarySearch(int[] array, int value, int low, int high)
    {
        if (array == null || array.Length < 1)
            return -1;
        if (low > high) return -1;
        int mid = low + ((high - low) >> 1);
        if (array[mid] == value)
        {
            return mid;
        }else if(array[mid] > value)
        {
            return BinarySearch(array, value, low, mid - 1);
        }
        else
        {
            return BinarySearch(array, value, mid + 1, high);
        }
    }
    // 非递归,有重复,求解的是第一个值等于给定值的元素
    public static int BinarySearch(int[] a ,int n, int value){
        int low = 0;
        int high = n - 1;
        while (low <= high) {
            int mid = low + ((high - low) >> 1);
            if (a[mid] > value) {
                high = mid - 1;
            } else if (a[mid] < value) {
                low = mid + 1;
            } else {
                if ((mid == 0) || (a[mid - 1] != value)){
                    return mid;
                }else {
                    high = mid - 1;
                }
            }
        }
        return -1;
    }

    // 非递归,有重复,查找最后一个值等于给定值的元素
    public int bsearch(int[] a, int n, int value) {
        int low = 0;
        int high = n - 1;
        while (low <= high) {
            int mid = low + ((high - low) >> 1);
            if (a[mid] > value) {
                high = mid - 1;
            } else if (a[mid] < value) {
                low = mid + 1;
            } else {
                if ((mid == n - 1) || (a[mid + 1] != value)){
                    return mid;
                }
                else {
                    low = mid + 1;
                }
            }
        }
        return -1;
    }

    // 查找第一个大于等于给定值的元素
    public int bsearch(int[] a, int n, int value) {
        int low = 0;
        int high = n - 1;
        while (low <= high) {
            int mid = low + ((high - low) >> 1);
            if (a[mid] >= value) {
                if ((mid == 0) || (a[mid - 1] < value)){
                    return mid;
                }else{
                    high = mid - 1;
                }
            } else {
                low = mid + 1;
            }
        }
        return -1;
    }

    // 查找最后一个小于等于给定值的元素
    public int bsearch(int[] a, int n, int value) {
    int low = 0;
    int high = n - 1;
    while (low <= high) {
        int mid = low + ((high - low) >> 1);
        if (a[mid] > value) {
            high = mid - 1;
        } else {
            if ((mid == n - 1) || (a[mid + 1] > value)) return mid;
            else low = mid + 1;
        }
    }
    return -1;
    }
    ```

    - 局限性
        - 二分查找依赖的是顺序表结构，简单点说就是数组,链表不可以,数组**按照下标随机访问数据**的时间复杂度是 O(1)，而链表随机访问的时间复杂度是 O(n)
        - 二分查找针对的是有序数据
        - 数据量太小不适合二分查找
            > 有个例外,数据之间的比较操作非常耗时，不管数据量大小，都推荐使用二分查找.比如，数组中存储的都是长度超过 300 的字符串，如此长的两个字符串之间比对大小，就会非常耗时。我们需要尽可能地减少比较次数，而比较次数的减少会大大提高性能，这个时候二分查找就比顺序遍历更有优势
        - 数据量太大也不适合二分查找
            - 二分查找的底层需要依赖数组这种数据结构，而数组为了支持随机访问的特性，要求内存空间连续。比如，我们有 1GB 大小的数据，如果希望用数组来存储，那就需要 1GB 的**连续**内存空间,如果这剩余的 1GB内存空间都是零散的，没有连续的 1GB 大小的内存空间，那照样无法申请一个 1GB 大小的数
组

- 插值运算
    - 场景: 在英文字典里面查“apple”，你下意识翻开字典是翻前面的书页还是后面的书页呢？如果再让你查“zoo”，你又怎么查？很显然，这里你绝对不会是从中间开始查起，而是有一定目的的往前或往后翻
    - 基本思想
        - 基于二分查找算法，将查找点的选择改进为自适应选择，让mid值的变化更靠近关键字key，这样也就间接地减少了比较次数
        - mid = (low+high)/2 ,  即 mid = low + 1/2*(high-low); 改进为 mid = low+(key-a[low]) / (a[high]-a[low])*(high-low);
    - 前提条件
        - 对于表长较大，而关键字分布又比较均匀的查找表来说，插值查找算法的平均性能比折半查找要好的多
    - 时间复杂度均为O(log2(log2n))

    ```csharp
    public static int BinarySearch(int[] array, int value, int low, int high)
    {
        if (array == null || array.Length < 1)
            return -1;
        int mid = low+(value-a[low])/(a[high]-a[low])*(high-low);
        if (array[mid] == value)
        {
            return mid;
        }else if(array[mid] > value)
        {
            return BinarySearch(array, value, low, mid - 1);
        }
        else
        {
            return BinarySearch(array, value, mid + 1, high);
        }
    }
    ```

- 二叉查找树(二叉搜索树, BST)
    - 基本思想
        - 二叉查找树是先对待查找的数据进行生成树，确保树的左分支的值小于右分支的值，然后在就行和每个节点的父节点比较大小，查找最适合的范围。
    - 时间复杂度 插入和查找均为O(logn)
    - 树
        - 二叉搜索树
        - 查找树
        - 红黑树
        - B树和B+树

- 哈希查找
    - 基本思想
        - 如果所有的键都是整数，那么就可以使用一个简单的无序数组来实现：将键作为索引，值即为其对应的值，这样就可以快速访问任意键的值。
    - 步骤
        - 用给定的哈希函数构造哈希表
        - 根据选择的冲突处理方法解决地址冲突,如拉链法
        - 在哈希表的基础上执行哈希查找



# 其他常见算法

- 字符串匹配
    - BF算法 RK算法
        - 应用:如何借助哈希算法实现高效字符串匹配
        - 特点:都是单模式串匹配,也就是一个串跟一个穿匹配
        - 概念:字符串 A 中查找字符串 B，那字符串 A 就是**主串**，字符串 B 就是**模式串**.把主串的长度记作 n，模式串的长度记作 m。因为我们是在主串中查找模式串，所以 n>m
        - BF ( Brute Force),暴力匹配算法
            - what?
                - 我们在主串中，检查起始位置分别是 0、1、2…n-m 且长度为 m 的 n-m+1 个子串，看有没有跟模式串匹配的,O(n * m)
        - RK算法,相当于把主串和模式串通过哈希算法求哈希值,直接比较哈希值大小(排除哈希冲突,数字比较效率高),本质上算法整体效率没提高
            - 哈希算法怎么设计?
                - 假设要匹配的字符串的字符集中只包含 K 个字符，我们可以用一个 K 进制数来表示一个子串，这个 K 进制数转化成十进制数，作为子串的哈希值
                    - 如,把 a～z 这 26 个字符映射到 0～25 这 26 个数字，a 就表示 0，b 就表示 1，以此类推，z 表示 25
                        - 如: "cba" = "c" * 26 * 26 + "b" * 26 + "a" * 1 = 2*26*26 + 1*26 + 0*1
            - 哈希冲突怎么解决?
                - 发现一个子串的哈希值跟模式串的哈希值相等的时候，我们只需要再对比一下子串和模式串本身就好
    - BM算法 KMP算法
        - BM算法
            - 应用:如何实现文本编辑器中的查找功能？
            - "abcacabdc",查找"abd",主串中的 c，在模式串中是不存在的，所以，模式串向后滑动的时候，只要 c 与模式串有重合，肯定无法匹配。所以，我们可以一次性把模式串往后多**滑动几位**，以此来减少不必要的字符比较，提高匹配的效率
        - KMP
            - KMP 算法和BM 算法的本质非常类似，都是根据规律在遇到坏字符的时候，把模式串往后多滑动几位
            - BM 算法有两个规则，坏字符和好后缀。KMP 算法借鉴 BM 算法的思想，可以总结成好前缀规则
    - Trie数和AC自动机,多模式匹配,在一个主串中同时查找多个模式串
        - Trie树
            - 本质
                - 利用字符串之间的公共前缀，将重复的前缀合并在一起,避免重复存储一组字符串的相同前缀子串
                - 空间换时间
            - 前提要求
                - 字符串包含的字符集不能天大
                - 要求字符串的前缀重合比较多
                - 指针串起来的数据不连续,对缓存不友好
            - 适合场景
                - 查找前缀匹配的字符串,如搜索引擎的搜索关键词提醒,输入法自动补全
            - 根节点到红色节点的一条路径表示一个字符串
                ![排序时空复杂度](/img/in-post/post-js-version/sort/trie_tree.png)
            - 缺点
                - 如果字符串中包含从 a 到 z 这 26 个字符，那每个节点都要存储一个长度为 26 的数组，并且每个数组存储一个 8 字节指针(或4字节)
                - 如果字符串中不仅包含小写字母，还包含大写字母、数字、甚至是中文,在重复的前缀并不多的情况下，Trie 树不但不能节省内存，还有可能会浪费更多的内存
            - 如何解决上述问题
                - 可以牺牲一点查询的效率，将每个节点中的数组换成其他数据结构,如有序数组
                    - 有序数组,通过二分查找的方法，快速查找到某个字符应该匹配的子节点的指针,但是插入一个字符串的时候，我们为了维护数组中数据的有序性，就会稍微慢
        - AC自动机
            - 适合场景
                - 敏感词过滤系统
            - 本质
                - AC 自动机实际上就是在 Trie 树之上，加了类似 KMP 的 next 数组，只不过此处的next 数组是构建在树上罢了

- 贪心算法
    - 使用场景
        - huffman 霍夫曼编码,利用贪心算法来实现对数据压缩编码，有效节省数据存储空间的,广泛用于数据压缩中,压缩率通常在 20%～90% 之间
    - 实战例子
        - 分糖果
            -  m 个糖果和 n 个孩子。我们现在要把糖果分给这些孩子吃，但是糖果少，孩子多（m<n），所以糖果只能分配给一部分孩子
            - 每个糖果的大小不等，这 m 个糖果的大小分别是 s1，s2，s3，……，sm。除此之外，每个孩子对糖果大小的需求也是不一样的，只有糖果的大小大于等于孩子的对糖果大小的需求的时候，孩
            - 如何分配糖果，能尽可能满足最多数量的孩子？
            - 思路:抽象成从 n 个孩子中，抽取一部分孩子分配糖果，让满足的孩子的个数（期望值）是最大的
            - 答案:每次从剩下的孩子中，找出对糖果大小需求最小的，然后发给他剩下的糖果中能满足他的最小的糖果
        -  区间覆盖
            - 有 n 个区间，区间的起始端点和结束端点分别是 [l1, r1]，[l2, r2]，[l3, r3]，……，[ln, rn]。我们从这 n 个区间中选出一部分区间，这部分区间满足两两不相交（端点相交的情况不算相交），最多能选出多少个区间呢
            - 类似问题:任务调度,教师排课等
            - 解决思路:
                - 我们假设这 n 个区间中最左端点是 lmin，最右端点是 rmax。这个问题就相当于，我们选择几个不相交的区间，从左到右将 [lmin, rmax] 覆盖上。我们按照起始端点从小到大的顺序对这 n 个区间排序
                - 每次选择的时候，左端点跟前面的已经覆盖的区间不重合的，右端点又尽量小的，这样可以让剩下的未覆盖区间尽可能的大，就可以放置更多的区间
- 分治算法
    - 运用场景
        - 大规模计算框架MapReduce中的分治思想
    - 三个步骤
        - 将原问题分解成一系列子问题
        - 递归地求解各个子问题，若子问题足够小，则直接求解(这一点与动态规划明显区别,**子问题可以独立求解,子问题之间没有相关性**)
        - 子问题的结果合并成原答案
- 回溯算法
    - 场景
        - 深度优先搜索
        - 八皇后问题
        - 0-1背包
        - 图的着色、旅行商问题、全排列
        - 数独
    - 思想
        - 采用试错思想,尝试分布的解决一个问题.在分布解决问题的过程中,当它通过尝试发现现有的分布答案不能得到有效的正确的解答的时候,它将取消上一步甚至是上几步的计算,再通过其它的可能的分布解答再次尝试寻找问题的答案
    - 最坏的情况下,回溯法会导致一次**复杂度为指数时间**的计算
- 动态规划


- 递归代码模板

    ```

    ```