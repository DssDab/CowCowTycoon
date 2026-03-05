using Assets.Scripts.Datas.SaveData;
using Assets.Scripts.Utility;
using Game.System;
using NUnit.Framework;
using System;
using UnityEngine;

public class SaveMapperTests
{
    private sealed class FakeClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = DateTime.UtcNow; }

    private static void SetDeterministicCow(FarmData farm, int id, float weight, float muscle, float fat, int period)
    {
        var fd = farm.Export();
        foreach(var c in fd.cows)
        {
            if(c.id == id)
            {
                c.weight = weight;
                c.muscle = muscle;
                c.fat = fat;
                c.period = period;
                c.eatCount = 0;
                c.stress = 0;
            }
        }
        farm.ApplyState(fd);

    }
    [Test]
    public void Build_Returns_GameSaveData_With_All_SubData()
    {
        var mapper = new SaveMapper();

        var player = new PlayerData();
        player.Init();

        var farm = new FarmData();
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();
        farm.Init(balance);

        int cowId = farm.AddCow();
        farm.SelectCowID(farm.Cows[cowId]);

        var market = new MarketData();

        var ms = new MarketSystem(new FakeClock(), balance, network);
        var session = new TrainingSession(farm, ms, player, balance, network);

        // 세션 값도 최소한 결정적으로 만들어줌
        session.ApplyState(new SessionSaveData() { day = 3, dayPhase = DayPhase.Morning, isFinished = false });

        var save = mapper.Build(session, player, farm, market);

        Assert.IsNotNull(save);
        Assert.IsNotNull(save.playerData);
        Assert.IsNotNull(save.farmData);
        Assert.IsNotNull(save.marketData);
        Assert.IsNotNull(save.sessionData);
    }
    [Test]
    public void RoundTrip_Apply_Reproduces_Core_State()
    {
        var mapper = new SaveMapper();

        // --- A : 원본 상태 ---
        var playerA = new PlayerData();
        playerA.Init();
        playerA.ChangeMoney(1234);
        playerA.ChangeHp(-2);

        var farmA = new FarmData();
        var balanceA = ScriptableObject.CreateInstance<BalanceConfig>();
        var network = ScriptableObject.CreateInstance<NetworkConfig>();

        farmA.Init(balanceA);
        farmA.AddFeed(10);
        int idA = farmA.AddCow();
        farmA.SelectCowID(farmA.Cows[idA]);
        SetDeterministicCow(farmA, idA, weight: 200, muscle: 80, fat: 100, period: 2);

        var marketA = new MarketData();
        marketA.CowPrice = 3456;
        marketA.FeedPrice = 222;

        var msA = new MarketSystem(new FakeClock(), balanceA, network);
        var sessionA = new TrainingSession(farmA, msA, playerA, balanceA, network);
        sessionA.ApplyState(new SessionSaveData() { day = 5, dayPhase = DayPhase.Evening, isFinished = false });

        // Build
        GameSaveData save = mapper.Build(sessionA, playerA, farmA, marketA);

        // --- B : 빈 상태 + Apply ---
        var playerB = new PlayerData();
        playerB.Init();
        var farmB = new FarmData();
        var balanceB = ScriptableObject.CreateInstance<BalanceConfig>();
        farmB.Init(balanceB);
        var marketB = new MarketData();
        var msB = new MarketSystem(new FakeClock(), balanceB, network);
        var sessionB = new TrainingSession(farmB, msB, playerB, balanceB, network);

        mapper.Apply(save, sessionB, playerB, farmB, marketB);

        // --- Assert : 핵심 필드 동일 ---
        Assert.AreEqual(playerB.PlayerMoney, playerB.PlayerMoney);
        Assert.AreEqual(playerB.PlayerHp, playerB.PlayerHp);

        Assert.AreEqual(marketA.CowPrice, marketB.CowPrice);
        Assert.AreEqual(marketA.FeedPrice, marketB.FeedPrice);

        Assert.AreEqual(farmA.FeedStock, farmB.FeedStock);
        Assert.AreEqual(farmA.MaxCowStock, farmB.MaxCowStock);
        Assert.AreEqual(farmA.CowSpawnKey, farmB.CowSpawnKey);
        Assert.AreEqual(farmA.Cows.Count, farmB.Cows.Count);

        // Cow 1마리 핵심 스탯 비교
        var cowA = farmA.Cows[idA];
        var cowB = farmB.Cows[idA];
        Assert.AreEqual(cowA.Weight, cowB.Weight);
        Assert.AreEqual(cowA.Muscle, cowB.Muscle);
        Assert.AreEqual(cowA.Fat, cowB.Fat);
        Assert.AreEqual(cowA.GetPeriod(), cowB.GetPeriod());

        Assert.AreEqual(sessionA._curDay, sessionB._curDay);
        Assert.AreEqual(sessionA._currentPhase, sessionB._currentPhase);
        Assert.AreEqual(sessionA.isFinished, sessionB.isFinished);

    }
    
}
