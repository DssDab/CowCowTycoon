using System.IO;
using System.Xml.Serialization;
using CowCowTycoon.Datas.AuctionData;
using UnityEngine;

namespace CowCowTycoon.Utils
{
    public static class ConvertDataType
    {

        public static Response DeserializeXml(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Response));
            using (StringReader reader = new StringReader(xml))
            {
                return (Response)serializer.Deserialize(reader);
            }
        }
        public static string ConvertXmlToJson(string xml)
        {
            Response response = DeserializeXml(xml);
            string json = JsonUtility.ToJson(response, true);
            return json;
        }
    }

}
