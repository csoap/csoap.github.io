---
layout:     post
title:      "C#常用容器的使用"
subtitle:   ""
date:       2021-11-16
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - csharp
---

# C# 常用容器大纲

- 容器
    - 线性
        - 直接存取
            - Array 数组
            - ArrayList 数组列表
            - List<T> 数组列表的泛型化
            - 字符串
            - 结构 Struct
        - 顺序存取
            - 栈 Stack<T>
            - 队列 Queue<T>
            - 索引
                - 散列表 HashTable
                - 字典 Dictionary<TKey,TValue>
                - 链表 LinkedList<T>
    - 非线性
        - 树
        - 集: HastSet<T>
        - 图

# 容器特性
- 数组
    - 缺点
        - 数组的两个数据间插入数据很麻烦,会有数据搬移操作
        - 声明数组必须指定数组长度
    - API
        - Array.IndexOf(arr,"d") != -1
        - arr.Length
        - Array.Clear(arr)
        - Array.Sort(arr)
        - Array.Reverse()
- ArrayList
    - 优点
        - 不必指定数组的长度,支持动态拓容
    - 缺点
        - 可以存储不同的类型数据,它会把所有元素当作object处理,会有装箱消耗,类型不安全
    - API
        ```csharp
        ArrayList list = new ArrayList();
        list.Add("abc");//新增
        list.Add(123);
        list[2] = 35;//修改
        list.RemoveAt(0); //移除
        list.Insert(0, "qwe");//插入
        ```
- List<T>,泛型集合
    - 它的大部分用法都与ArrayList相似
    - 关键的区别:声明List集合时，需要为其声明List集合内数据的对象类型
    - API
        - List<Wife> list1 = new List<Wife>();
        - list1.Insert(1, wf1); // 在指定位置插入
        - list1.Remove(wf1);// 删除指定对象的第一个匹配项
        - list1.Add(new Wife("aa")); // 将元素添加到末尾
- LinkedList<T> 双链表
    - 结构:LinkedListNode< T >被LinkedList类包含，用LinkedListNode类，可以获得元素的上一个与下一个元素的引用
    - 优点
        - 插入删除快
            - 只需要修改上一个元素的Next与下一个元素的Previous的引用, 像ArrayList列表中，如果插入，需要移动其后的所有元素
    - 缺点
        - 查找慢,链表只能是一个接着一个的访问
    - 与List 对比
        - List 是数组列表,LinkedList 是双向链表
        - List读取速度块O(1), 增删O(n),List可以直接通过索引值定位到元素
        - LinkedList 读取O(n),增删O(1)
    - API
        ```csharp
        LinkedList<int> list = new LinkedList<int>();
        list.AddFirst(1);
        list.AddLast(2);
        print(list.Count);
        bool have = list.Contains(1);

        LinkedListNode<T> first = list.First;
        LinkedListNode<T> last = list.Last;
        // AddAfter()在某个节点之后添加,AddBefore,Remove() 删除指定的一个匹配对象, RemoveFirst(), RemoveLast()
        // Find() 从链表头开始找一个元素，并返回他的节点类，LinkedListNode<T>,FindLast()是从尾部来搜
        // Clear() 清除所有元素
    ```

- HashSet, 表示值的集
    - 与Dictionary几乎一样,差别仅在于HashSet没有key，只有value
    - 表示值的集
    - 不包含任何重复的元素，元素顺序不分先后
    - 用O(n)的空间换取O(1)的查找时间
    - API
        ```csharp
        HashSet<int> evenNumbers = new HashSet<int>();
        evenNumbers.Add(1);
        //UnionWith - Union 或将其设置的添加
        //IntersectWith - 交集
        //ExceptWith - Set 减法
        //SymmetricExceptWith - 余集
        ```
- HashTable
    - key/value键值对均为object类型
    - 内部维护很多对Key-Value键值对，其还有一个类似索引的值叫做散列值(HashCode)，它是根据GetHashCode方法对Key通过一定算法获取得到的
    - 填充因子:HashTable中的被占用空间达到一个百分比的时候就将该空间自动扩容,扩容的大小是当前空间大小的两倍最接近的素数
    - 缺点,装箱拆箱的消耗,所以就有了Dictionary<T>
    - 对比Dictionary
        - 单线程使用Dictionary, 多线程使用HashTable,HashTable可以通过Hashtable tab = Hashtable.Synchronized(new Hashtable());获得线程安全的对象
    ```csharp
    Hashtable table = new Hashtable();
    table.Add("名字", "张三");
    print(table["名字"]);

    foreach(DictionEntry entry in table){
        print(entry.Key + ":");
        print(entry.Value);
    }
    ```
- Dictionary
    - 相对于HashTable，类似于List和ArrayList的关系。它是类型安全的
    ```csharp
    Dictionary<string, Person> dic = new Dictionary<string, Person>();
    Person person1 = new Person(){name= "张三"};
    dic.Add("学生1", person1);
    foreach (string key in dic.Keys) //只取key
    {
        Console.WriteLine(key);
    }
    foreach (Person person in dic.Values)    //只取value
    {
        Console.WriteLine(person);
    }

    ```
- HashSet/HashTable/Dictionary
    - 容器的底层都是Hash表
    - 操作元素
        - 定义int[] m_buckets 数组来保存元素在实际容器Slot[] m_slots 位置
        - 即 Value的保存在 m_slots[m_buckets[value.GetHashCode()%m_buckets.Length]].value
    - 容器长度为质数
        - 质数只能被1和自身整除
        - 减少位置冲突
    - 冲突解决
        - 当位置冲突时使用Slot.next保存数据
    - 数据已满时添加数据
        - 新建一个2倍大小的容器
        - 数据拷贝过去 重新计算位置

- Queue
    - 先进先出
    ```csharp
    Queue q = new Queue();
    q.Enque("a");
    string str = q.Dequeue();
    ```
- Stack
    - 后进先出
    ```csharp
    Stack st = new Stack();
    st.Push("a");
    string str= st.Pop();
    ```


# 常用方法API

- Random
    ```csharp
    //public virtual int Next(int minValue, int maxValue);
    Random ran = new Random();
    int rand1 = ran.Next(100,999); //100至999的随机数
    int rand2 = ran.Next(); 返回一个大于或等于零而小于2,147,483,647的数
    ran.NextDouble()返回一个介于 0.0 和 1.0 之间的随机数
    ```
- String
    - IndexOf(char value, int startIndex);//查找字符索引 startIndex 默认0
    - Substring(int startIndex, int length)
    - Insert(int startIndex, string value)
    - Remove(int startIndex, int count)
    - ToArray() char[] str1 = str.ToArray();
    - Contains,ToLower,ToUpper
- StringBuilder
    - sb.ToString();// 转字符串
    - sb.Append("c"); //新增字符
    - sb.Remove(); // 移除字符