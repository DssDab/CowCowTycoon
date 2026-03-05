using Assets.Scripts.Datas.SaveData;
using System.IO;
using UnityEngine;
namespace Assets.Scripts.Utility
{
    public class SaveDataRepository
    {
        private string GetPath() => Path.Combine(Application.persistentDataPath, "GameSaveData.json");

     
        public void Save(GameSaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            string path = GetPath();
            
            File.WriteAllText(path, json);
            
        }

        public bool TryLoad(out GameSaveData data)
        {
            data = null;
            string path = GetPath();    
           if(File.Exists(path) == false)
                return false;

            try
            {
                string text = File.ReadAllText(path);
                data = JsonUtility.FromJson<GameSaveData>(text);
                return data != null;
            }
            catch
            {
                data = null;
                return false;
            }
        }
        public void DeleteSaveData()
        {
            string path = GetPath();
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
