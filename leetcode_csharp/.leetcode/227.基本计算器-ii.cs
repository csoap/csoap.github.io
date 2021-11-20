/*
 * @lc app=leetcode.cn id=227 lang=csharp
 *
 * [227] 基本计算器 II
 */

// @lc code=start
public class Solution {
    public int Calculate(string s) {
        s = s.Replace(" ", ""); // 将" "字符替换为""
        char[] s1 = s.ToCharArray(); //字符串转为字符数组
        Stack ts = new Stack();
        int len = s1.Length;
        int ans = 0;
        int num = 0;
        char opt = '+';  //初始化opt为'+',是为了把第一个数放入堆栈
        char ch;
        for (int i = 0; i < len; i++)
        {
            num = 0;
            // 将字符转为数字，并且在循环里取完数字
            while (char.IsDigit(s1[i]))
            {
                num = num * 10 + s1[i] - '0';
                i++;
                if (i >= len)
                {
                    i--;
                    break;
                }
            }
            ch = s1[i];
            switch (opt)
            {
                case '+':
                    ts.Push(num); //遇到'+',将num放入堆栈，注意这里的num指的都是运算符后的数字
                    break;
                case '-':
                    ts.Push(-num); //遇到'-',将-num放入堆栈
                    break;
                case '*':
                    ts.Push((int)ts.Pop() * num);
                    break;
                case '/':
                    ts.Push((int)ts.Pop() / num);
                    break;
                default:
                    break;
            }
            opt = ch;
        }
        //将堆栈中的数字相加，所得的和就是答案
        while (ts.Count > 0)
        {
            ans += (int)ts.Pop();
        }
        return ans;
    }
}
// @lc code=end

