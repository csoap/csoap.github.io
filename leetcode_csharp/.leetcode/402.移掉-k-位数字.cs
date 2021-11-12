/*
 * @lc app=leetcode.cn id=402 lang=csharp
 *
 * [402] 移掉 K 位数字
 */

// @lc code=start
public class Solution {
    //贪心算法 + 双端队列
    public string RemoveKdigits(string num, int k) {
        var stack = new LinkedList<char>(); // 双端队列
        foreach (var digit in num)
        {
            //当且仅当K>0 并且队尾元素大于要入队的元素的时候就把队尾元素移除掉
            while (stack.Count > 0 && k > 0 && stack.Last.Value > digit)
            {
                stack.RemoveLast();
                k--;
            }
            //如果不大于则直接入队
            stack.AddLast(digit);
        }
        //此时如果K还大于0 队列里面的元素已经为单调不降了。则最后依次移除队列尾部剩余的k数次即可，
        for (int i = 0; i < k; i++) stack.RemoveLast();
        bool isLeadingZero = true;
        StringBuilder ans = new StringBuilder();
        //从队列头部取出所有元素
        foreach (var digit in stack)
        {
            //防止前导0 也就是队头第一个元素==0 则需要跳过。
            if (isLeadingZero && digit == '0') continue;
            isLeadingZero = false;
            ans.Append (digit);
        }
        if (ans.Length == 0) return "0";
        return ans.ToString();
    }
}
// @lc code=end

