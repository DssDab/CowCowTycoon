
using Assets.Scripts.Datas.SaveData;

public class PlayerData 
{
    public int PlayerHp { get; private set; }
    
    public int PlayerMoney { get; private set; }

    public void Init()
    {
        PlayerHp = 7;
        PlayerMoney = 2500;

    }

    public void ChangeHp(int value)=>PlayerHp += value;
    public void ChangeMoney(int price)=> PlayerMoney += price;

    public PlayerSaveData Export()
    {
        return new PlayerSaveData() { hp = PlayerHp, money = PlayerMoney };
    }

    public void ApplyState(PlayerSaveData pd)
    {
        PlayerHp = pd.hp;
        PlayerMoney = pd.money;
    }

}
