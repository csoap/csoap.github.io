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

