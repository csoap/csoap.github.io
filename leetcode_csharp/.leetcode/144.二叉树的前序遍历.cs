/*
 * @lc app=leetcode.cn id=144 lang=csharp
 *
 * [144] 二叉树的前序遍历
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
    //栈
    public IList<int> PreorderTraversal(TreeNode root) {
        List<int> res = new List<int>();
        if (root == null) return res;
        Stack<TreeNode> stack = new Stack<TreeNode>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            TreeNode node = stack.Pop();
            res.Add(node.val);
            //先入后出
            if (node.right != null)
            {
                stack.Push(node.right);
            }
            if (node.left != null)
            {
                stack.Push(node.left);
            }
        }
        return res;
    }
    //递归
    // IList<int> res;
    // public IList<int> PreorderTraversal(TreeNode root) {
    //     res = new List<int>();
    //     if (root == null) return res;
    //     Helper(root);
    //     return res;
    // }

    // private void Helper(TreeNode root) {
    //     if(root != null){
    //         res.Add(root.val);
    //         if (root.left != null) Helper(root.left);
    //         if (root.right != null) Helper(root.right);
    //     }
    // }

}
// @lc code=end

