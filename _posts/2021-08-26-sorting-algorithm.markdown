---
layout:     post
title:      "常用排序,查找算法汇总"
subtitle:   ""
date:       2019-11-29
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
- C#基础常用API
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
    - Array
        - Array.IndexOf(arr,"d") != -1
        - arr.Length
        - Array.Clear(arr)
        - Array.Sort(arr)
        - Array.Reverse()
    - List
        - List<Wife> list1 = new List<Wife>();
        - list1.Insert(1, wf1); // 在指定位置插入
        - list1.Remove(wf1);// 删除指定对象的第一个匹配项
        - list1.Add(new Wife("aa")); // 将元素添加到末尾
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

    ![插入排序](/img/in-post/post-js-version/sort/merge_sort.png)

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
        - 建堆
            - 上浮,不断插入元素进行建堆
            - 下沉,通过下沉操作建堆效率更高，具体过程是，找到最后一个**非叶子节点**，然后从后往前遍历执行下沉操作
        - 排序
            - 将堆顶元素（代表最大元素）与最后一个元素交换，然后新的堆顶元素进行下沉操作，遍历执行上诉操作，则可以完成排序
        > 先将待排序的序列建成大根堆，使得每个父节点的元素大于等于它的子节点。此时整个序列最大值即为堆顶元素，我们将其与末尾元素交换，使末尾元素为最大值，然后再调整堆顶元素使得剩下的 n-1n−1 个元素仍为大根堆，再重复执行以上操作我们即能得到一个有序的序列
    - 节点公式
        - parent(i) = (i-1)/2
        - leftChild(i) = 2*i+1
        - rightChild(i) = 2*i+2
    - 时间复杂度
        - 因为我们建堆的时间复杂度为 O(n），排序过程的时间复杂度为 O(nlogn),所以总的空间复杂度为 O(nlogn)
    - 空间复杂度  O(1)
    - 快速排序和堆排序对比
        - 对于快速排序来说，数据是顺序访问的。而对于堆排序来说，数据是跳着访问的。这样对 CPU 缓存是不友好的
        - 相同的的数据，排序过程中，堆排序的数据交换次数要多于快速排序

    ```csharp
    public static void HeapSort(int[] nums)
    {
        int len = nums.Length;
        if (len < 1) return;
        //1.利用上浮操作,构建一个最大堆
        BuildMaxHeap(nums);
        //2.循环将堆首位（最大值）与末位交换，然后在重新调整最大堆
        for (int i = len - 1; i > 0; i--)
        {
            Swap(nums, 0, i);
            len--;
            AdjustHeap(nums, 0, len);
        }
    }

    public static void BuildMaxHeap(int[] nums)
    {
        //从第一个非叶子结点从下至上，从右至左调整结构
        int len = nums.Length;
        for (int i = (len / 2) - 1; i >= 0; i--)
        {
            AdjustHeap(nums, i, len);
        }
    }

    public static void AdjustHeap(int[] array, int i, int len)
    {
        int left = 2 * i + 1;
        int right = 2 * i + 2;
        int largest = i;
        //如果有左子树，且左子树大于父节点，则将最大指针指向左子树
        if (left < len && array[left] > array[largest])
        {
            largest = left;
        }
        //如果有右子树，且右子树大于父节点，则将最大指针指向右子树
        if (right < len && array[right] > array[largest])
        {
            largest = right;
        }
        //如果父节点不是最大值，则将父节点与最大值交换，并且递归调整与父节点交换的位置
        if (largest != i)
        {
            Swap(array, i, largest);
            AdjustHeap(array, largest, len);
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
    public int bsearch7(int[] a, int n, int value) {
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