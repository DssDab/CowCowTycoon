using Assets.Scripts.Datas.SaveData;
using System;
using Unity.VisualScripting;

public enum ItemType
{
    Cow,
    Feed
}
public class MarketData 
{
    public const int FEED_BUNDLE = 5;
    public int FeedPrice { get; set; } = 100;

    public int CowPrice { get; set; } = 2000;

    public MarketSaveData Export()
    {
        return new MarketSaveData() { cowPrice = CowPrice, feedPrice = FeedPrice };
    }
    public void ApplyState(MarketSaveData md)
    {
        CowPrice = md.cowPrice;
        FeedPrice = md.feedPrice;
    }
    
}
