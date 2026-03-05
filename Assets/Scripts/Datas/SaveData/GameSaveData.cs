using System;
namespace Assets.Scripts.Datas.SaveData
{
    [Serializable]
    public class GameSaveData 
    {
        public SessionSaveData sessionData;
        public PlayerSaveData playerData;
        public FarmSaveData farmData;
        public MarketSaveData marketData;
    }

}
