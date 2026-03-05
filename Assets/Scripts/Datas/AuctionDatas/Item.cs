using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace CowCowTycoon.Datas.AuctionData
{
    [Serializable]
    public class Item
    {
        public int auctAmt;
        public int auctCnt;
        public string baseYmd;
        public string beforeYmd;
        public int diffAuctAmt;
        public string judgeBreedCd;
        public string judgeKindCd;
        public string localCode;
        public string localNm;
    }
    [Serializable]
    public class Header
    {
        public string resultCode;
        public string resultMsg;
    }
    [Serializable]
    public class Notice
    {
        public string rows;
    }
    [Serializable]
    public class Body
    {
        [XmlArray("items")]
        [XmlArrayItem("item")]
        public List<Item> items;
    }
    [XmlRoot("response")]
    public class Response
    {
        public Header header;
        public Notice notice;
        public Body body;
    }
}
