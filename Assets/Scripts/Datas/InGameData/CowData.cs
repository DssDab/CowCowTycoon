using Assets.Scripts.Datas;
using Assets.Scripts.Datas.SaveData;
using System;

[Serializable]
public class CowData 
{
    public int ID { get; private set; }
    
    public float Weight { get; private set; }
    public float Muscle { get; private set; }
    public float Fat { get; private set; }
    public float Stress { get; private set; }

    public int EatCount { get; private set; }

    private int m_period;

    public event Action OnAte;
    public void Init(int id)
    {
        ID = id;
        Weight = UnityEngine.Random.Range(300f, 310.1f);
        Fat = UnityEngine.Random.Range(95f, 120.1f);
        Muscle = UnityEngine.Random.Range(Fat * 1.2f, Fat * 1.23f);
        Stress = 0f;
        m_period = 1;
    }

 
    public void Grow(BalanceConfig balance)
    {
        float hungerStress = balance != null ? balance.hungerStress : 5f;
        float baseStress = balance != null ? balance.baseStress : 1f;
        // 굶었을 떄
        if (EatCount == 0)
        {
            Stress += hungerStress;

            float lossKg = balance != null ? balance.starveLossKg : 2f;
            Weight = Math.Max(0, Weight - lossKg);

            float starveScale = balance != null ? balance.starveScale : 1.5f;
            float indexLoss = lossKg * starveScale;

            float fatLossShare = balance != null ? balance.starveLossFatShare : 0.85f;
            fatLossShare = Math.Clamp(fatLossShare, 0f, 1f);
            float muscleLossShare = 1f - fatLossShare;

            Muscle = Math.Max(0f, Muscle - indexLoss * muscleLossShare);
            Fat = Math.Max(0f, Fat - indexLoss * fatLossShare);
            return;
        }

        float realAdgKg = balance != null ? balance.realAdgKg : 1f;
        float gameScale = balance != null ? balance.gameScale : 20f;
        float maxGainKg = balance != null ? balance.maxGainKg : 25f;
        float feedCurve = balance != null ? balance.feedCurve : 0.8f;

        // 비육전기 일당증체량) 0.9 ~ 1.0kg
        // EatCount가 성장에 영향을 줌
        float feedFactor = 1f - UnityEngine.Mathf.Exp(-feedCurve * EatCount);
        float dailyGainKg = realAdgKg * gameScale * feedFactor;
        if (dailyGainKg > maxGainKg)
            dailyGainKg = maxGainKg;
        // 스트레스가 높을수록 성장이 둔화 (최저 0.6배 까지만)

        float stressScale = balance != null ? balance.stressScale : 100f;
        float stressMinFactor = balance != null ? balance.stressMinFactor : 0.6f;
        float stressT = UnityEngine.Mathf.Clamp01(Stress / stressScale);
        float stressFactor = UnityEngine.Mathf.Lerp(1f, stressMinFactor, stressT);

        // 체중 성장(kg)
        float gain = dailyGainKg * stressFactor;
        Weight += gain;


        float indexScale = balance != null ? balance.indexScale : 0.2f;  
        float totalIndexGain = gain * indexScale;

        float muscleShare = balance != null ? balance.muscleShare : 0.55f;
        float fatShare = balance != null ? balance.fatShare : 0.6f;
        Muscle += totalIndexGain * muscleShare;
        Fat += totalIndexGain * fatShare;


     
       

    }

    
    public void Eat()
    {
        EatCount++;
        OnAte?.Invoke();
    }
    public void ResetEatCount() => EatCount = 0;

    public void UpdateLifeCycle() => m_period++;
    public int GetPeriod() => m_period;

    public CowSaveData Export()
    {
        return new CowSaveData() { id = ID, weight = Weight, muscle = Muscle, fat = Fat, stress = Stress, eatCount = EatCount, period = m_period };
    }
    public void ApplyState(CowSaveData cd)
    {
        ID = cd.id;
        Weight = cd.weight;
        Muscle = cd.muscle;
        Fat = cd.fat;
        Stress = cd.stress;
        EatCount = cd.eatCount;
        m_period = cd.period;
    }

  
}
