using System;

namespace Assets.Scripts.Datas.SaveData
{
    [Serializable]
    public class CowSaveData
    {
        public int id;
        public float weight, muscle, fat;
        public float stress;
        public int eatCount;
        public int period;
    }
}
