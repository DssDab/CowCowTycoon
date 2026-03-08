using Assets.Scripts.Datas.SaveData;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Game.System
{
    public class TrainingSession 
    {

        public int _curDay { get; private set; }
        public int _maxDay { get; private set; }
        public DayPhase _currentPhase {  get; private set; }    

        private FarmData farm;
        private MarketSystem marketSystem;
        private PlayerData player;

        public event Action<string> OnActionFeedback;

        public event Func<bool> OnFed;
        public event Func<CowData, string> OnCowSpawned;
        public event Action<CowData> OnDayChanged;
        public event Action<int> OnSold;

        public event Action<DayPhase, int, int, int,int>OnSessionUpdated;
        public event Action<TrainingSession, PlayerData, FarmData, MarketData> OnSaved;
        public event Action<string> OnSessionEnded;

        private CancellationTokenSource _cts;

        private readonly NetworkConfig _network;
     
        public bool isFinished { get; private set; }

        public TrainingSession(FarmData farm,
                               MarketSystem marketSystem, PlayerData player, BalanceConfig balance, NetworkConfig network)
        {
            this.farm = farm;
            this.marketSystem = marketSystem;
            this.player = player;

            _network = network;
            _maxDay = balance != null ? balance.maxDay : 7;
        }

        public async Task StartSessionAsync()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();

            _curDay = 1;
            _currentPhase = DayPhase.Morning;
            isFinished = false;

           

            int index = farm.AddCow();
            if(farm.Cows.TryGetValue(index, out CowData cowData))
            {
                OnCowSpawned?.Invoke(cowData);
            }

            if(_network != null && string.IsNullOrEmpty(_network.serverUrl) == false)
            {
                await marketSystem.InitPriceAsync(_cts.Token);
            }

            OnSessionUpdated?.Invoke(_currentPhase,
                                    _curDay,
                                    player.PlayerHp,
                                    player.PlayerMoney,
                                    farm.FeedStock);
        }


        public void ExecuteAction(TrainingAction action)
        {
            if (isFinished)
                return;

            string feedback = "";

            int hp = 0;
           
            if (player.PlayerHp <= 0 &&
            action != TrainingAction.Rest)
            {
                feedback = "플레이어의 체력 낮음.";
                OnActionFeedback?.Invoke(feedback);
                return;
            }
            switch (action)
            {
                case TrainingAction.Feed:
                {
                    // 먹이가 부족하여 먹이를 줄 수 없음 이벤트
                    // 행동 실패 시 실패사유를 string 형식으로 반환해서
                    // 화면에 출력하기 위해서 OnFed의 반환값을 사용
                    if(OnFed == null ||
                       OnFed.Invoke() == false)
                    {
                        feedback = "사료를 구매하세요..";
                        OnActionFeedback?.Invoke(feedback);
                        return;
                    }
                    // 먹이를 먹임
                    hp = -1;
                    break;
                }
                case TrainingAction.Rest:
                {
                    // 휴식을 취함. 이벤트
                    // 플레이어 체력 회복
                    hp = 2;
                    break;
                }
                case TrainingAction.BuyCow:
                {
                    if (marketSystem.TryBuy(player.PlayerMoney, farm.Cows.Count, farm.MaxCowStock, ItemType.Cow, out int cost, out feedback) == false)
                    {
                        // 현재 축사레벨이 낮아서 구매가 불가능 이벤트 발생
                        OnActionFeedback?.Invoke(feedback);
                        return;
                    }
                    // 구매 성공!!
                    hp = -1;
                    int id = farm.AddCow();
                    feedback = OnCowSpawned?.Invoke(farm.Cows[id]);
                    player.ChangeMoney(cost);
                    break;
                }
                case TrainingAction.BuyFeed:
                {
                    if (marketSystem.TryBuy(player.PlayerMoney, farm.Cows.Count, farm.MaxCowStock, ItemType.Feed, out int cost, out feedback) == false)
                    {
                        // 현재 소지금이 부족해서 구매가 불가능 이벤트 발생
                        OnActionFeedback?.Invoke(feedback);
                        return;
                    }
                        // 구매 성공!!
                    int feed = marketSystem._marketData.FEED_BUNDLE;
                    farm.AddFeed(feed);
                    player.ChangeMoney(cost);
                    hp = -1;
                    break;
                }
                case TrainingAction.SellCow:
                {
                    CowData cowData = farm.GetSelectedCowData();
                    
                    if(cowData == null)
                    {
                        feedback = "현재 선택된 소가 없습니다.";
                        OnActionFeedback?.Invoke(feedback);
                        return;
                    }
                    int lifeCycle = cowData.GetPeriod();
                    if (marketSystem.TrySell(ItemType.Cow, cowData, lifeCycle, _currentPhase, out int price ,out feedback) == false)
                    {
                        OnActionFeedback?.Invoke(feedback);
                        return;
                    }
                    // 판매 성공
                    hp = -1;
                    OnSold?.Invoke(farm.SelectedID);
                        player.ChangeHp(hp);
                        player.ChangeMoney(price);
                        OnSessionUpdated?.Invoke(_currentPhase,
                                      _curDay,
                                      player.PlayerHp,
                                      player.PlayerMoney,
                                      farm.FeedStock);
                        return;
                }
            }
           
            player.ChangeHp(hp);
            
            NextPhase();

        }
        public void NextPhase()
        {
            switch (_currentPhase)
            {
                case DayPhase.Morning:
                    _currentPhase = DayPhase.Afternoon;
                    break;
                case DayPhase.Afternoon:
                    _currentPhase = DayPhase.Evening;
                    break;
                case DayPhase.Evening:
                    _currentPhase = DayPhase.Night;
                    break;
                case DayPhase.Night:
                    {
                        NextDay();
                        break;
                    }
            }
            OnSessionUpdated?.Invoke(_currentPhase,
                                      _curDay,
                                      player.PlayerHp,
                                      player.PlayerMoney,
                                      farm.FeedStock);

        }
      
        private void NextDay()
        {
          
            if (_curDay >= _maxDay)
            {
                EndSession();
                return;
            }
            _curDay++;
            _currentPhase = DayPhase.Morning;
            // 소 상태 업데이트
            farm.NextDay();
            if(farm.Cows.TryGetValue(farm.SelectedID, out CowData cow))
            {
                OnDayChanged?.Invoke(cow);
            }
          
            marketSystem.ApplyMorningSimulatedPrice();
            OnSaved?.Invoke(this, player, farm, marketSystem._marketData);

        }
        public SessionSaveData Export()
        {
            return new SessionSaveData() { day = _curDay, dayPhase = _currentPhase, isFinished = this.isFinished };
        }
        public void ApplyState(SessionSaveData sd)
        {
            _curDay = sd.day;
            _currentPhase = sd.dayPhase;
            this.isFinished = sd.isFinished;
        }
        public void SessionLoad()
        {
            OnSessionUpdated?.Invoke(_currentPhase, _curDay, player.PlayerHp, player.PlayerMoney, farm.FeedStock);
            marketSystem.PublishCurrentPrice();

            foreach (var id in farm.Cows)
            {
                OnCowSpawned?.Invoke(id.Value);
            }
          
        }
        private void EndSession()
        {
            isFinished = true;

            int money = player.PlayerMoney;
            int cowCount = farm.Cows.Count;

            string result = marketSystem.EndSessionReport(farm.Cows, money, cowCount, _curDay);
            
            OnSessionEnded?.Invoke(result);

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }


      
    }

}
