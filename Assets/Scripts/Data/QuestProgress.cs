using System;

[Serializable]
public class QuestProgress
{
    public string questId;
    public int currentAmount;
    public bool isCompleted;
    public bool isClaimed;
}
