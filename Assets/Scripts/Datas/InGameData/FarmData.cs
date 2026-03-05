using Assets.Scripts.Datas.SaveData;
using System.Collections.Generic;

public class FarmData 
{
    public int CowSpawnKey { get; private set; }

    private Dictionary<int,CowData> m_Cows = new Dictionary<int, CowData>();
    public int FeedStock { get; private set; }
    public int MaxCowStock { get; private set; }
    public int SelectedID { get; private set; }

    public IReadOnlyDictionary<int,CowData> Cows => m_Cows;

    private BalanceConfig _balance;

    public void Init(BalanceConfig balance)
    {
        _balance = balance;
        FeedStock = balance != null ? balance.initialFeedStock : 5;
        MaxCowStock = balance != null ? balance.maxCowStock : 5;

        SelectedID = 0;
        CowSpawnKey = 1;
    }

    public bool FeedingCow()
    {
        if(FeedStock <= 0)
            return false;

        if (m_Cows.TryGetValue(SelectedID, out CowData cow) == false)
            return false;

        cow.Eat();
        FeedStock -= 1;

        return true;
    }

    public int AddCow()
    {
        CowData cow = new CowData();
        
        cow.Init(CowSpawnKey);
        m_Cows.Add(cow.ID,cow);
        return CowSpawnKey++;
    }
    public void AddFeed(int feedStock)
    {
        FeedStock += feedStock;
    }

    public void NextDay()
    {
        foreach (var cow in m_Cows)
        {
            cow.Value.Grow(_balance);
            cow.Value.UpdateLifeCycle();
            cow.Value.ResetEatCount();
        }
    }

    public void SelectCowID(CowData data)
    {
        SelectedID = data.ID;
    }

    public void ClearCowData(int index)
    {
        if(Cows.ContainsKey(index))
        {
            m_Cows.Remove(index);
            SelectedID = 0;
        }

    }
    public CowData GetSelectedCowData()
    {
        bool hasKey = m_Cows.TryGetValue(SelectedID, out CowData data) == true;
        if (hasKey)
            return data;

        return null;
    }
    public FarmSaveData Export()
    {
        List<CowSaveData> cows = new List<CowSaveData>();
        foreach(var cow in m_Cows)
        {
            cows.Add(cow.Value.Export());
        }
        FarmSaveData data = new FarmSaveData(FeedStock,MaxCowStock,CowSpawnKey, cows);

        return data;
    }
    public void ApplyState(FarmSaveData fd)
    {
        FeedStock = fd.feedStock;
        MaxCowStock = fd.maxCowStock;
        CowSpawnKey = fd.nextCowId;
        SelectedID = 0;

        m_Cows.Clear();
        foreach(var data in fd.cows)
        {
            var cow = new CowData();
            cow.ApplyState(data);
            m_Cows.Add(cow.ID, cow);
        }
    }
   
}
