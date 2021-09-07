/*
 * @lc app=leetcode.cn id=1 lang=csharp
 *
 * https://leetcode-cn.com/problems/ti-huan-kong-ge-lcof/
 * 剑指 Offer 05. 替换空格
 */

// @lc code=start
public class Solution {
    public string ReplaceSpace(string s) {
        if(s == null || s.Length <= 0){
            return s;
        }
        int curLength = s.Length;
        int numberOfBlank = 0;
        //创建一个字符数组，长度等与原字符串长度加上空格数 * 2
        for (int i = 0; i < curLength; i++){
            if (s[i] == ' '){
                ++numberOfBlank;
            }
        }
        int newLength = curLength + numberOfBlank * 2;
        int point1 = curLength - 1;
        int point2 = newLength - 1;
        char[] str = new char[newLength];
        //两个指针，一个指向原字符串最后一个，另一个指向字符数组最后一个，然后在字符数组的后面注入字符遇到空格就替换
        while(point1 >= 0){
            if (s[point1] == ' '){
                str[point2--] = '0';
                str[point2--] = '2';
                str[point2--] = '%';
            }
            else
            {
                str[point2--] = s[point1];
            }
            --point1;
        }
        string newStr = new string(str);
        return newStr;

    }
}
// @lc code=end

