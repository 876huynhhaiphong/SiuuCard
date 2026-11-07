public class PlayerCombat : CombatUnit
{
    public void Initialize(int maxHP)
    {
        this.maxHP = maxHP;
        currentHP = maxHP;
        currentBlock = 0;
    }
}
