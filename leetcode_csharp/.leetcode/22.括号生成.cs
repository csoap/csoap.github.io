/*
 * @lc app=leetcode.cn id=22 lang=csharp
 *
 * [22] 括号生成
 */

// @lc code=start
public class Solution {
    // 回溯法
    public IList<string> GenerateParenthesis(int n) {
        IList<string> res = new List<string>();
        StringBuilder sb = new StringBuilder();
        Helper(res, sb, 0, 0, n);
        return res;
    }

    public void Helper(IList<string> res, StringBuilder sb, int open, int close, int n){
        if (sb.Length == n * 2){
            res.Add(sb.ToString());
            return;
        }
        if (open < n) {
            sb.Append("(");
            Helper(res, sb, open + 1, close, n);
            sb.Remove(sb.Length - 1, 1);
        }
        if (close < open){
            sb.Append(")");
            Helper(res, sb, open, close + 1, n);
            sb.Remove(sb.Length - 1, 1);
        }
    }
}
// @lc code=end

