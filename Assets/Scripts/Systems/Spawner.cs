using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public event Action<CowController> OnSpawned;
    private GameObject CowPrefab { get; set; }

    private Dictionary<int, GameObject> _cows;

    private int _maxCowStock;

    private Queue<GameObject> _cowPools;
    private List<Transform> _spawnPos;
    private List<Transform> _spawnablePos;
    // 캐싱용
    private Dictionary<int, Transform> _spawnPosCache;

    public void Initialize(GameObject cowPrefab, List<Transform> spawnPos, int maxCowStock)
    {
        CowPrefab = cowPrefab;
        _cows = new Dictionary<int, GameObject>();
        _maxCowStock = maxCowStock;

        _cowPools = new Queue<GameObject>();
        _spawnPos = new List<Transform>();
        _spawnablePos = new List<Transform>();
        _spawnPosCache = new Dictionary<int, Transform>();

        SetSpawnPosition(spawnPos);
        InitCowPools();

    }

    public string TrySpawnCow(CowData cowData)
    {

        if (cowData == null)
            return "Fail : CowData is null..";

        if (_spawnablePos == null || _spawnablePos.Count == 0)
            return "Fail : NoSpawnable position";

        if (_cows.Count < _maxCowStock)
        {
            if (_cows.ContainsKey(cowData.ID))
                return "Fail : Cow alread spawned";

            // 스폰 위치 지정
            int index = UnityEngine.Random.Range(0, _spawnablePos.Count);
            Vector3 spawnPos = _spawnablePos[index].position;

            if (CowPrefab == null)
                return "Fail : CowPrefab is null";
            // 소를 풀에서 가져옴
            GameObject cow = GetCowPool();
            cow.transform.position = spawnPos;

            CowController cowController = cow.GetComponent<CowController>();
            cowController.SetData(cowData);

            _cows.Add(cowController.Data.ID, cow);
            _spawnPosCache.Add(cowController.Data.ID, _spawnablePos[index]);
            _spawnablePos.RemoveAt(index);
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
            cow.SetActive(false);
            _cowPools.Enqueue(cow);
        }
        if (_spawnPosCache.TryGetValue(id, out Transform pos))
        {
            _spawnablePos.Add(pos);
        }
        _spawnPosCache.Remove(id);

    }
    private GameObject GetCowPool()
    {
        if (_cowPools.Count > 0)
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
