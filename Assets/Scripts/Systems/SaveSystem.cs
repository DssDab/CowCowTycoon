using Assets.Scripts.Datas.SaveData;
using Assets.Scripts.Utility;

using Game.System;

public class SaveSystem
{
    private readonly SaveMapper _mapper;
    private readonly SaveDataRepository _repository;

    public SaveSystem(SaveMapper mapper, SaveDataRepository repo)
    {
        _mapper = mapper;
        _repository = repo;
    }

    public void Save(TrainingSession s, PlayerData p, FarmData f, MarketData m) => _repository.Save(_mapper.Build(s, p, f, m));

    public bool TryLoad(TrainingSession s, PlayerData p, FarmData f, MarketData m)
    {
        if (_repository.TryLoad(out GameSaveData dto) == false)
            return false;

        _mapper.Apply(dto, s, p, f, m);
        return true;
    }

    public void DeleteSaveData(string _)=>_repository.DeleteSaveData();

}

