using Assets.Scripts.Datas.SaveData;
using Assets.Scripts.Utility;
using Game.System;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MarketSystemTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    }

    private static CowData MakeCow(float weight, float muscle, float fat, int period = 2, int id = 1)
    {
        var cow = new CowData();
        cow.ApplyState(new CowSaveData()
        {
            id = id,
            weight = weight,
            muscle = muscle,
            fat = fat,
            stress = 0,
            eatCount = 0,
            period = period,
        });
        return cow;

    }

    [Test]
    public void TryBuy_Fails_WhenMoneyInsufficient()
    {
        // Arrange
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();

        var ms = new MarketSystem(new FakeClock(), balance, network);

        // Act
        bool ok = ms.TryBuy(money: 0, curCowCap: 0, maxCowCap: 5, type: ItemType.Cow, out int cost, out string reason);

        //Assert
        Assert.IsFalse(ok);
        Assert.AreEqual(0, cost);
        Assert.IsNotEmpty(reason);
    }
    [Test]
    public void TryBuy_Fails_WhenBarnFull_ForCow()
    {
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();

        var ms = new MarketSystem(new FakeClock(), balance, network);

        bool ok = ms.TryBuy(money: 99999, curCowCap: 5, maxCowCap: 5, type: ItemType.Cow, out int cost, out string reason);

        Assert.IsFalse(ok);
        Assert.AreEqual(0, cost);
        Assert.IsNotEmpty(reason);
    }
    [Test]
    public void TryBuy_Succeeds_WhenEnoghMoney()
    {
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();

        var ms = new MarketSystem(new FakeClock(), balance, network);

        bool ok = ms.TryBuy(money: 99999, curCowCap: 0, maxCowCap: 5, type: ItemType.Feed, out int cost, out string reason);

        Assert.IsTrue(ok);
        Assert.Less(cost, 0);
        Assert.IsTrue(string.IsNullOrEmpty(reason));
    }
    [Test]
    public void TrySell_Fails_WhenPeriodLessThan_2()
    {
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();

        var ms = new MarketSystem(new FakeClock(), balance, network);
        var cow = MakeCow(id: 1, weight: 200f, muscle: 80f, fat: 100f, period: 1);

        bool ok = ms.TrySell(type: ItemType.Cow, cow, period: cow.GetPeriod(), phase: DayPhase.Morning, out int money, out string reason);

        Assert.IsFalse(ok);
        Assert.AreEqual(0, money);
        Assert.IsNotEmpty(reason);
    }
    [Test]
    public void TrySell_WhenMorning()
    {
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();

        var ms = new MarketSystem(new FakeClock(), balance, network);
        var cow = MakeCow(id: 1, weight: 200f, muscle: 80f, fat: 100, period: 2);

        bool ok = ms.TrySell(type: ItemType.Cow, cow, period: cow.GetPeriod(), phase: DayPhase.Afternoon, out int money, out string reason);

        Assert.IsFalse(ok);
        Assert.AreEqual(0, money);
        Assert.IsNotEmpty(reason);
    }
    [Test]
    public void TrySell_GradeMultiplier_A_ShouldBeGreaterThan_C()
    {
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();

        var ms = new MarketSystem(new FakeClock(), balance, network);

        // A : score = (muscle / fat) == 0.8 => 1.2배
        var cowA = MakeCow(id: 1, weight: 200, muscle: 80, fat: 100, period: 2);

        // C : (score >= 1.5) => 0.7배
        var cowC = MakeCow(id: 1, weight: 250, muscle: 150, fat: 100, period: 2);

        bool isA = ms.TrySell(type: ItemType.Cow, cowA, period: cowA.GetPeriod(), phase: DayPhase.Morning, out int moneyA, out string reasonA);
        bool isC = ms.TrySell(type: ItemType.Cow, cowC, period: cowC.GetPeriod(), phase: DayPhase.Morning, out int moneyC, out string reasonC);

        Assert.IsTrue(isA);
        Assert.IsTrue(isC);

        Assert.Greater(moneyA, moneyC);
    }
    [Test]
    public void EndSessionReport_Contains_SummaryKeywards()
    {
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();
        var ms = new MarketSystem(new FakeClock(), balance, network);

        CowData cow1 = MakeCow(id: 1, weight: 200, muscle: 80, fat: 100, period: 2);
        CowData cow2 = MakeCow(id: 2, weight: 250, muscle: 150, fat: 100, period: 2);

        // 최소 1마리라도 넣어야 등급/분포가 의미가 있음
        var cows = new Dictionary<int, CowData>()
        {
            {cow1.ID, cow1 },
            {cow2.ID, cow2 }
        };

        string report = ms.EndSessionReport(cows, money: 2500, cowCount: cows.Count, day: 7);

        Assert.IsNotEmpty(report);
        Assert.IsTrue(report.Contains("총 자산") || report.Contains("자산"));
    }
    
    
}
