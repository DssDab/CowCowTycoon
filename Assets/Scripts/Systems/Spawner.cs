using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public event Action<CowController> OnSpawned;
    private GameObject CowPrefab {  get; set; }

    private Dictionary<int,GameObject> _cows;

    private int _maxCowStock;

    private Queue<GameObject> _cowPools;
    private List<Transform> _spawnPos;
    private List<Transform> _spawnablePos;


    public void Initialize(GameObject cowPrefab, List<Transform> spawnPos, int maxCowStock)
    {
        CowPrefab = cowPrefab;
        _cows = new Dictionary<int, GameObject>();
        _maxCowStock = maxCowStock;

        _cowPools = new Queue<GameObject>();
        _spawnPos = new List<Transform>();
        _spawnablePos = new List<Transform>();

        SetSpawnPosition(spawnPos);
        InitCowPools();

    }
    
    public string TrySpawnCow(CowData cowData)
    {
        GameObject cow = null;

        if (cowData == null)
            return "Fail : CowData is null..";

        if (_cows.Count < _maxCowStock)
        {
            // 스폰 위치 지정
            int index = UnityEngine.Random.Range(0, _spawnablePos.Count);
            Vector3 spawnPos = _spawnablePos[index].position;
            _spawnablePos.RemoveAt(index);
            

            if (CowPrefab == null)
                return "Fail : CowPrefab is null";
            // 소를 풀에서 가져옴
            cow = GetCowPool();
            cow.transform.position = spawnPos;

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
            _cows.Remove(id);
        }
        cow.SetActive(false);
        _cowPools.Enqueue(cow);
        _spawnablePos.Add(cow.transform);
    }
    private GameObject GetCowPool()
    {
        if(_cowPools.Count > 0)
        {
            GameObject go = _cowPools.Dequeue();
            go.SetActive(true);
            return go;
        }
        
        GameObject cow = Instantiate(CowPrefab);
        return cow;

    }
    private void InitCowPools() 
    {
        for (int i = 0; i < _maxCowStock; i++)
        {
            GameObject go = Instantiate(CowPrefab);
            go.SetActive(false);
            _cowPools.Enqueue(go);
        }
    }
    private void SetSpawnPosition(List<Transform> spawnPos)
    {
        if (spawnPos == null)
            return;

        _spawnPos.Clear();
        foreach (var pos in spawnPos)
        {
            _spawnPos.Add(pos);
        }
        _spawnablePos.Clear();
        foreach (var pos in _spawnPos)
        {
            _spawnablePos.Add(pos);
        }
    }

}
