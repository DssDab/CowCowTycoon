using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject CowPrefab {  get; private set; }

    public event Action<CowController> OnSpawned;

    private Dictionary<int,GameObject> _cows;

    private int _maxCowStock;

    public void Initialize(GameObject cowPrefab, int maxCowStock)
    {
        CowPrefab = cowPrefab;
        _cows = new Dictionary<int, GameObject>();
        _maxCowStock = maxCowStock;

    }
    
    public string TrySpawnCow(CowData cowData)
    {
        GameObject cow = null;

        if (cowData == null)
            return "Fail : CowData is null..";

        if (_cows.Count < _maxCowStock)
        {
            // 스폰 위치 지정
            Vector3 randomViewportPos = new Vector3(
            UnityEngine.Random.Range(0.3f, 0.7f), // X: 30%~70%
            UnityEngine.Random.Range(0.3f, 0.7f), // Y: 30%~70%
            0f);

            if (Camera.main == null)
                return "Fail : MainCamera Is Missing..";
            Vector3 worldPos = Camera.main.ViewportToWorldPoint(randomViewportPos);
            worldPos.z = 0; // 2D라서 z=0 고정

            if (CowPrefab == null)
                return "Fail : CowPrefab is null";
            // 소 게임오브젝트 생성
            cow = Instantiate(CowPrefab, worldPos, Quaternion.identity);
            CowController cowController = cow.GetComponent<CowController>();
            cowController.SetData(cowData);
            if (_cows.ContainsKey(cowController.Data.ID))
                return "Fail : Cow alread spawned";

            _cows.Add(cowController.Data.ID, cow);
            OnSpawned?.Invoke(cowController);
            return "";
            
        }
        return "Fail : ";
      
        
    }
    public void DeSpawnCow(int id)
    {
        if (_cows.TryGetValue(id, out var cow))
        {
            Destroy(cow);
            _cows.Remove(id);
        }            

    }

}
