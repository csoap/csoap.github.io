---
layout:     post
title:      "常用排序算法汇总"
subtitle:   "十大排序算法"
date:       2019-11-29
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 算法
---

- 环境
    - 语言:C#

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

    ```csharp
    public static void BubbleSort(int[] nums)
    {
        if (nums == null) return;
        int len = nums.Length;
        if (len == 0) return;
        for (int i = 0; i < len - 1; i++)
        {
            for (int j = 0; j < len - 1 - i; j++)
            {
                if (nums[j] > nums[j + 1])
                {
                    Swap(nums, j, j + 1);
                }
            }
        }
    }
    ```
    - 算法分析
        - 最佳情况 O(n),在代码中使用一个**标志位**来判断是否已经排序好
        - 最差情况 O(n^2)
        - 平均情况 O(n)
- 选择排序

    ![选择排序](/img/in-post/post-js-version/sort/select_sort.gif)

    ```csharp
    public static void SelectSort(int[] nums)
    {
        if (nums == null) return;
        int count = nums.Length;
        if (count == 0) return;
        int min;
        for (int i = 0; i < count - 1;  i++)
        {
            min = i;
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

- 插入排序
    - 联想:斗地主抓牌时候插入牌

    ![插入排序](/img/in-post/post-js-version/sort/insert_sort.gif)

    ```csharp
    public static void InsertSort(int[] nums)
    {
        if (nums == null) return;
        int count = nums.Length;
        if (count == 0) return;
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
        if (count == 0) return;
        int gap = count / 2;
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
        if (nums == null) return;
        int count = nums.Length;
        if (count == 0) return;
        tmp = new int[count];
        MergeSort(nums, 0, count - 1);
    }

    public static void MergeSort(int[] nums, int l, int r)
    {
        if (l >= r) return;
        int mid = (l + r) >> 1;
        MergeSort(nums, l, mid);
        MergeSort(nums, mid + 1, r);
        int i = l, j = mid + 1;
        int cnt = 0;
        while (i <= mid && j <= r)
        {
            if (nums[i] <= nums[j])
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
        for (int k = 0; k < r - l + 1; k++)
        {
            nums[k + l] = tmp[k];
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
        int index = SortUnit(nums, low, high);
        QuickSort(nums, low, index - 1);
        QuickSort(nums, index + 1, high);
    }

    public static int SortUnit(int[] array, int low, int high)
    {
        int key = array[low];//基准数
        while (low < high)
        {
            //从high往前找小于或等于key的值
            while (low < high && array[high] > key)
                high--;
            //比key小开等的放左边
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
        - 假设输入数据服从均匀分布，将数据分到有限数量的桶里，每个桶再分别排序（有可能再使用别的排序算法或是以递归方式继续使用桶排序进行排序

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
    - 元素必须是有序,如果无序则要先进行排序
    - 基本思想
        - 用给定值k先与中间结点的关键字比较，中间结点把线形表分成两个子表，若相等则查找成功；若不相等，再根据k与该中间结点关键字的比较结果确定下一步查找哪个子表，这样递归进行，直到查找到或查找结束发现表中没有这样的结点。
        > 二分查找的前提条件是需要有序表顺序存储，对于静态查找表，一次排序后不再变化，折半查找能得到不错的效率。但对于需要**频繁执行插入或删除操作的数据集**来说，维护有序的排序会带来不小的工作量，那就**不建议使用**。
    - 时间复杂度 O(log2n)

    ```csharp
    // 非递归
    public static int BinarySearch(int[] array, int value)
    {
        if (array == null || array.Length < 1)
            return -1;
        int low = 0;
        int high = array.Length - 1;
        int mid;
        while (low <= high)
        {
            mid = low +( (high - low)  >> 1); //low+(high-low)/2, 为什么(low +high) / 2会溢出啊？答：两个很大的int相加的话超出 Integer.MAX_VALUE 了
            if (array[mid] == value)
            {
                return mid;
            }
            if (array[mid] > value)
            {
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }
        return low;
    }

    //递归版本
    public static int BinarySearch(int[] array, int value, int low, int high)
    {
        if (array == null || array.Length < 1)
            return -1;
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
    ```

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