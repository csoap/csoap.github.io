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
- 不错的文章链接
    - https://leetcode-cn.com/problems/sort-an-array/solution/fu-xi-ji-chu-pai-xu-suan-fa-java-by-liweiwei1419/
    - https://blog.csdn.net/weixin_41190227/article/details/86600821
    - https://www.lfzxb.top/summary-of-common-sorting-algorithms/
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
        - 先将待排序的序列建成大根堆，使得每个父节点的元素大于等于它的子节点。此时整个序列最大值即为堆顶元素，我们将其与末尾元素交换，使末尾元素为最大值，然后再调整堆顶元素使得剩下的 n-1n−1 个元素仍为大根堆，再重复执行以上操作我们即能得到一个有序的序列
