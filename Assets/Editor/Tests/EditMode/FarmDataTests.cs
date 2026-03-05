using NUnit.Framework;
using UnityEngine;

public class FarmDataTests
{
    [Test]
    public void AddCow_IncreasesCount_And_NextCowId()
    {
        var farm = new FarmData();
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        farm.Init(balance);

        int beforeCount = farm.Cows.Count;
        int beforeNextId = farm.CowSpawnKey;

        int spawnKey = farm.AddCow();

        Assert.AreEqual(beforeCount + 1, farm.Cows.Count);
        Assert.AreEqual(beforeNextId, spawnKey);
        Assert.AreEqual(beforeNextId + 1, farm.CowSpawnKey);
    }
    [Test]
    public void SelectCowID_Updates_SelectedId()
    {
        var farm = new FarmData();
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        farm.Init(balance);
        int id = farm.AddCow();

        var cow = farm.Cows[id];
        farm.SelectCowID(cow);

        Assert.AreEqual(id, farm.SelectedID);
    }
    [Test]
    public void FeedingCows_Fails_WhenNoSelectedCow()
    {
        var farm = new FarmData();
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        farm.Init(balance);
        farm.AddCow();  // SelectedID는 0 상태

        bool ok = farm.FeedingCow();

        Assert.IsFalse(ok);
    }
    [Test]
    public void FeedingCows_Succeeds_ConsumesFeed_And_IncreasesEatCount()
    {
        var farm = new FarmData();
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        farm.Init(balance);

        int id = farm.AddCow();
        var cow = farm.Cows[id];
        farm.SelectCowID(cow);

        int beforeFeed = farm.FeedStock;
        int beforeEat = cow.EatCount;

        bool ok = farm.FeedingCow();

        Assert.IsTrue(ok);
        Assert.AreEqual(beforeFeed - 1, farm.FeedStock);
        Assert.AreEqual(beforeEat + 1, cow.EatCount);
    }
    [Test]
    public void ClearCowData_RemovesCow_And_ResetsSelected()
    {
        var farm = new FarmData();
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        farm.Init(balance);

        int id = farm.AddCow();

        farm.SelectCowID(farm.Cows[id]);
        Assert.AreEqual(id, farm.SelectedID);

        farm.ClearCowData(id);

        Assert.IsFalse(farm.Cows.ContainsKey(id));
        Assert.AreEqual(0, farm.SelectedID);
    }
    [Test]
    public void NextDay_ResetsEatCount_And_IncrementsPeriod()
    {
        var farm = new FarmData();
        var balance = ScriptableObject.CreateInstance<BalanceConfig>();
        farm.Init(balance);

        int id = farm.AddCow();
        var cow = farm.Cows[id];

        farm.SelectCowID(cow);
        farm.FeedingCow(); //  EatCount 증가

        int beforePeriod = cow.GetPeriod();
        Assert.Greater(cow.EatCount, 0);

        farm.NextDay();

        Assert.AreEqual(0, cow.EatCount);
        Assert.AreEqual(beforePeriod + 1, cow.GetPeriod());

    }
}
