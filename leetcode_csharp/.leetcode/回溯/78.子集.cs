/*
 * @lc app=leetcode.cn id=78 lang=csharp
 *
 * [78] 子集
 */

// @lc code=start
public class Solution {
    private IList<IList<int>> ans;

    public IList<IList<int>> Subsets(int[] nums) {
        ans = new List<IList<int>>();
        if(nums == null || nums.Length == 0) {
            return ans;
        }
        Backtrack(nums, new List<int>(), 0);
        return ans;
    }

    public void Backtrack(int[] nums, List<int> path, int start) {
        ans.Add(new List<int>(path));
        // 选择列表
        for(int i = start; i < nums.Length; i++) {
            path.Add(nums[i]);
            Backtrack(nums, path, i+1);
            path.Remove(nums[i]);
        }
    }
}
// @lc code=end

