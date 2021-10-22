---
layout:     post
title:      "常用查找算法汇总"
subtitle:   ""
date:       2019-11-29
author:     "CSoap"
header-img: "img/home-bg-o.jpg"
tags:
    - 算法
---
- 环境
    - 语言:C#
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

