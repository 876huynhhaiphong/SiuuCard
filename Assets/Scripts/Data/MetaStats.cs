using System;

[Serializable]
public class MetaStats
{
    public int bonusMaxHP = 0;        // +HP gốc khi bắt đầu run
    public int bonusMaxMana = 0;      // +Mana tối đa mỗi turn
    public int bonusDraw = 0;         // +Số bài rút thêm mỗi turn
    public int bonusStartingGold = 0;
    public int bonusBookSlots = 0;    // +Sức chứa Quyển sách (mới so với doc gốc)
}
