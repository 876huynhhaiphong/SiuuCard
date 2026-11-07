using System.Collections.Generic;

public class RunResult
{
    public bool victory;
    public int combatsWon;
    public List<string> newlyUnlockedCards = new List<string>();

    /// <summary>
    /// Rule hiện tại: THẮNG run thì mọi card nhận được trong run vào kho vĩnh viễn; THUA thì mất hết.
    /// (Đổi rule ở đây nếu muốn, ví dụ chỉ giữ ngẫu nhiên 1 vài card.)
    /// </summary>
    public static RunResult FromRun(RunData run, bool victory)
    {
        return new RunResult
        {
            victory = victory,
            combatsWon = run.combatsWon,
            newlyUnlockedCards = victory ? new List<string>(run.cardsGainedThisRun) : new List<string>()
        };
    }
}
