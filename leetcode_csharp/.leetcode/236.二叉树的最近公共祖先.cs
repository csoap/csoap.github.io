/*
 * @lc app=leetcode.cn id=236 lang=csharp
 *
 * [236] 二叉树的最近公共祖先
 */

// @lc code=start
/**
 * Definition for a binary tree node.
 * public class TreeNode {
 *     public int val;
 *     public TreeNode left;
 *     public TreeNode right;
 *     public TreeNode(int x) { val = x; }
 * }
 */
 //最近公共祖先条件：1.left,right 都是root的子孙，
//  2.root 是left和right两节点的祖先节点中离根节点最远的节点
//定义子问题:定义 f(x)表示 x 节点的子树中是否包含 p 节点或 q 节点，如果包含为 true，否则为 false
//其中 lson 和 rson 分别代表 x 节点的左孩子和右孩子。
// ( (lson && rson) || ((root.val == p.val || root.val == q.val) && (lson || rson)) ) 解读为
// 1.lson && rson说明左子树和右子树均包含 pp 节点或 qq 节点
// 2.x 恰好是 p 节点或 q 节点且它的左子树或右子树有一个包含了另一个节点的情况
public class Solution {
    TreeNode ans;
    public TreeNode LowestCommonAncestor(TreeNode root, TreeNode p, TreeNode q) {
        dfs(root, p, q);
        return ans;
    }

    public bool dfs(TreeNode root, TreeNode p, TreeNode q) {
        if (root == null) return false;
        bool lson = dfs(root.left, p, q);
        bool rson = dfs(root.right, p, q);
        if ( (lson && rson) || ((root.val == p.val || root.val == q.val) && (lson || rson)) )
        {
            ans = root;
        }
        return lson || rson || (root.val == p.val || root.val == q.val);
    }
}
// @lc code=end

