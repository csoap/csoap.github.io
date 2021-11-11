/*
 * @lc app=leetcode.cn id=100 lang=csharp
 *
 * [100] 相同的树
 */

// @lc code=start
/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int val=0, TreeNode left=null, TreeNode right=null) {
 *         this.val = val;
 *         this.left = left;
 *         this.right = right;
 *     }
 * }
 */
public class Solution {
    // 深度优先搜索
    // public bool IsSameTree(TreeNode p, TreeNode q) {
    //     if (p == null && q == null) return true;
    //     if (p == null || q == null) return false;
    //     if (p.val != q.val) return false;
    //     return IsSameTree(p.left, q.left) && IsSameTree(p.right, q.right);
    // }
    // 广度优先搜索
    public bool IsSameTree(TreeNode p, TreeNode q) {
        if (p == null && q == null) return true;
        if (p == null || q == null) return false;
        if (p.val != q.val) return false;
        Queue<TreeNode> queue1 = new Queue<TreeNode>();
        Queue<TreeNode> queue2 = new Queue<TreeNode>();
        queue1.Enqueue(p);
        queue2.Enqueue(q);
        while (queue1.Count != 0 && queue2.Count != 0)
        {
            TreeNode node1 = queue1.Dequeue();
            TreeNode node2 = queue2.Dequeue();
            if (node1.val != node2.val) return false;
            TreeNode left1 = node1.left, right1 = node1.right, left2 = node2.left, right2 = node2.right;
            if ((left1 == null) ^ (left2 == null)) return false; // 仅一个true 才为true
            if ((right1 == null) ^ (right2 == null)) return false;
            if (left1 != null)
            {
                queue1.Enqueue(left1);
            }
            if (right1 != null)
            {
                queue1.Enqueue(right1);
            }
            if (left2 != null)
            {
                queue2.Enqueue(left2);
            }
            if (right2 != null)
            {
                queue2.Enqueue(right2);
            }
        }
        return queue1.Count == 0 && queue1.Count == 0;
    }
}
// @lc code=end

