using Game.System;
using Assets.Scripts.Datas.SaveData;

namespace Assets.Scripts.Utility
{
    public class SaveMapper
    {
        // 런타임 상태 -> 저장 DTO로
        public GameSaveData Build(TrainingSession session, PlayerData player, FarmData farm, MarketData market)
        {
            return new GameSaveData()
            {
                sessionData = session.Export(),
                playerData = player.Export(),
                farmData = farm.Export(),
                marketData = market.Export()
            };
        }
        public void Apply(GameSaveData data, TrainingSession session, PlayerData player, FarmData farm, MarketData market)
        {
            if (data == null)
                return;

            // 적용 순서 : player/farm/market -> Session
            // (Session을 먼저 로드해도 되지만 UI 갱신 타이밍 때문에 데이터 먼저 로드)
            if (data.playerData != null)
                player.ApplyState(data.playerData);

            if(data.farmData != null)
                farm.ApplyState(data.farmData);

            if(data.marketData != null)
                market.ApplyState(data.marketData);

            if(data.sessionData != null)
                session.ApplyState(data.sessionData);

        }
    }
}
