/*
 * @lc app=leetcode.cn id=50 lang=csharp
 *
 * [50] Pow(x, n)
 */

// @lc code=start
public class Solution {

    //分治法: pow(x, n)
    // subProblem: subResult = pow(x, n / 2)
    public double MyPow(double x, int n) {
        if (n < 0)
        {
            x = 1 / x;
            n = -n;
        }
        return Helper(x, n);
    }

    private double Helper(double x, int n){
        if (n == 0) return 1.0;
        if (n == 1) return x;
        double half = Helper(x, n / 2); // 子问题
        // 子问题合并
        return n % 2 == 0 ? half * half : half * half * x;
    }
}
// @lc code=end

