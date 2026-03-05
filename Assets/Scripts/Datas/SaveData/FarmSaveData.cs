using System;
using System.Collections.Generic;

namespace Assets.Scripts.Datas.SaveData
{
    [Serializable]
    public class FarmSaveData
    {
        public int feedStock;
        public int maxCowStock;
        public int nextCowId;
        public List<CowSaveData> cows;

        public FarmSaveData(int feedStock, int maxCowStock, int nextCowId, List<CowSaveData> cows) 
        {
            this.feedStock = feedStock;
            this.maxCowStock = maxCowStock;
            this.nextCowId = nextCowId;
            this.cows = cows;
        }
    }
}
