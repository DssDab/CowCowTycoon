using System;

namespace Assets.Scripts.Datas.SaveData
{
    [Serializable]
    public class SessionSaveData
    {
        public int day;
        public DayPhase dayPhase;
        public bool isFinished;
    }
}
