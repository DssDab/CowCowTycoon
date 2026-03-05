using System;
using UnityEngine;

[CreateAssetMenu(menuName = "CowCowTycoon/Config/BalanceConfig")]
public class BalanceConfig : ScriptableObject
{
    [Header("Session Rules")]
    [Min(1)] public int maxDay = 7;

    [Header("Farm Rules")]
    [Min(0)] public int initialFeedStock = 5;  // FarmData.Init()
    [Min(1)] public int maxCowStock = 5;       // FarmData.Init()

    // 실제 비육전기 기간이 5개월인 점을 감안해서
    // 게임용 스케일을 적용
    [Header("Growth Tuning (CowData)")]
    public float realAdgKg = 1.0f;
    public float gameScale = 20.0f;
    public float maxGainKg = 25.0f; // 하루 최대 증체량 상한 (값이 너무 커지는걸 막기 위해서)

    [Range(0f, 5f)] public float feedCurve = 0.8f; 
    [Min(1f)] public float stressScale = 100f;      // Stress/100
    [Range(0f, 1f)] public float stressMinFactor = 0.6f; // lerp(1, 0.6, t)
    [Min(0f)] public float hungerStress = 5f;       // EatCount==0
    [Min(0f)] public float baseStress = 1f;         // else
    [Min(0)] public float starveScale = 1.5f;
    [Min(0f)] public float starveLossKg = 2f;
    [Min(0f)] public float starveLossFatShare = 0.85f;

    // 근육/지방 지표 성장 : 체중 증가량에 비례, 비육전기에서 근육+지방이 같이 붙음.
    // (지표 스케일은 게임 밸런스용) - ratio가 1.2 근처로 유지되기 쉬운 분배
    public float indexScale = 0.2f;                 // 체중 1kg 증가에 대해 지표 총합이 2 정도 증가
    [Range(0f, 1f)] public float muscleShare = 0.55f;
    [Range(0f, 1f)] public float fatShare = 0.6f;

    [Header("Market Tuning")]
    [Range(0f, 1f)] public float morningVolatility = 0.10f;
    [Min(0)] public int fallbackCowPrice = 2000;
    [Min(1)] public int feedBundleSize = 5;

    [Header("Sell Price - Weight Class")]
    public WeightClassTuning weightClass = new WeightClassTuning();

    [Header("Sell Price - Grade")]
    public GradeTuning grade = new GradeTuning();

    [Serializable]
    public class WeightClassTuning
    {
        public float denomLight = 310f;
        public float denomMid = 320f;
        public float denomHeavy = 330f;
        public float denomMax = 350f;

        public float minFactor = 0.7f;
        public float maxFactor = 1.6f;
    }

    [Serializable]
    public class GradeTuning
    {
        // 비육전기에서 근육/지방이 고르게 붙게 한다.

        [Range(0f, 1f)] public float targetFatPct = 0.4737f;
        public float devA = 0.03f;
        public float devB = 0.08f;

        public float mulA = 1.2f;
        public float mulB = 0.9f;
        public float mulC = 0.7f;
    }
}