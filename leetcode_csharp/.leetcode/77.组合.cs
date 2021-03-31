/*
 * @lc app=leetcode.cn id=77 lang=csharp
 *
 * [77] 组合
 */

// @lc code=start
public class Solution {
    //回溯
    IList<IList<int>> res;
    public IList<IList<int>> Combine(int n, int k) {
        res = new List<IList<int>>();
        if (k > n || k <= 0) return res;
        List<int> temp = new List<int>();
        Backtrack(temp, n, k, 1);
        return res;
    }
    private void Backtrack(List<int> temp, int n, int k, int start) {
        //临界
        if (temp.Count == k)
        {
            res.Add(new List<int>(temp));
            return;
        }
        for (; start <= n; start++)
        {
            temp.Add(start);
            Backtrack(temp, n, k, start + 1);
            temp.RemoveAt(temp.Count - 1);
        }
    }

}
// @lc code=end

