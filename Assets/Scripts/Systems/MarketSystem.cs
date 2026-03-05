using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game.System
{

    public class MarketSystem 
    {
        private struct Price_Cache
        {
            public int cowPrice;        // 마지막 적용 가격(초기 성공 or fallback 이후 적용값)
            public DateTime UTC;        // 마지막 성공 갱신 시각 성공했을 때만 갱신 권장.
            public int fail;            // 가격 갱신을 실패한 횟수
            public string failReason;   // 실패사유
        }

        private NetworkUtility _networkUtility;

        public MarketData _marketData { get; private set; }
        private Price_Cache _priceCache; // 가격 캐싱용

        private BalanceConfig _balance;
        private NetworkConfig _network;

        private bool _initDone;         // 초기 가격 셋업이 끝났는지 확인용
        private bool _initNetTried;     // 네트워크 초기 가격을 시도했는지 확인용


        private readonly TimeSpan _kstOffset = TimeSpan.FromHours(9);
        private readonly TimeSpan _auctionOpen = new TimeSpan(9, 0, 0);
        private readonly TimeSpan _auctionClose = new TimeSpan(11, 30, 0);

        private readonly IClock _time;

        public event Action<int> OnUpdatedPrice;
       
        public MarketSystem(IClock clock, BalanceConfig balance, NetworkConfig network)
        {
            _balance = balance;
            _network = network;
            _networkUtility = new NetworkUtility(_network);
            _marketData = new MarketData();

            _time = clock ?? new SystemClock();
            int fallback = _balance != null ? balance.fallbackCowPrice : 2000;
            _priceCache.cowPrice = _marketData.CowPrice > 0 ? _marketData.CowPrice : fallback;
            _priceCache.UTC = DateTime.MinValue;
            _priceCache.fail = 0;

            _initDone = false;
            _initNetTried = false;
        }

        // 게임 시작 시 1회만 호출하도록 의도된 초기화 메서드
        public async Task<bool> InitPriceAsync(CancellationToken ct)
        {
            // 초기화 완료(성공이든 실패든)면 no-op
            if (_initDone)
            {
                _priceCache.failReason = "INIT_SKIP : already initialized";
                UnityEngine.Debug.LogWarning(_priceCache.failReason);
                return true;
            }

            // 네트워크 초기화 시도는 딱 1회만
            if(_initNetTried)
            {
                // 이미 시도했는데 아직 _initDone이 false인 경우는
                // 동시 호출(race) 가능성. 여기서는 안전하기 no-op 처리
                _priceCache.failReason = "INIT_SKIP : already tried";
                UnityEngine.Debug.LogWarning(_priceCache.failReason);
                return false;
            }
            _initNetTried = true;

            if(IsAuctionWindowKst() == false)
            {
                _priceCache.failReason = "INIT_FALLBACK : outside auction window";
                UnityEngine.Debug.LogWarning(_priceCache.failReason);
                _priceCache.UTC = DateTime.UtcNow;

                ApplyFallbackInitialPrice();
                _initDone = true;

                return false;
            }

            bool ok = await TryFetchAndApplyAsync(ct);
            if (ok == false)
            {
                // 초기 실패 : 규칙 기반 fallback 적용
                ApplyFallbackInitialPrice();
            }
            
            // 초기 가격 셋업은 여기서 완료로 간주(성공/실패 포함)
            _initDone = true;
           

            return ok;
        }

        // Morning에 호출할 로컬 변동(±10%) 적용
        public void ApplyMorningSimulatedPrice()
        {
            EnsureInitializedLocally();

            int cur = _marketData.CowPrice;
            float vol = _balance != null ? _balance.morningVolatility : 0.1f;
            int next = ApplyPercentBand (cur, vol);

            _marketData.CowPrice = next;
            _priceCache.cowPrice = next;

            PublishCurrentPrice();
        }

        private async Task<bool> TryFetchAndApplyAsync(CancellationToken ct)
        {
            try
            {
                int timeoutSec = _network != null ? _network.timeoutSeconds : 5;
                int auctAmt = await _networkUtility.GetTodayAuctAmtAsync(ct, timeoutSec);
                int price = auctAmt / 10;

                _marketData.CowPrice = price;
                _priceCache.cowPrice = price;
                _priceCache.UTC = _time.UtcNow.UtcDateTime;
                _priceCache.fail = 0;

                _priceCache.failReason = null;
                UnityEngine.Debug.Log($"PRICE_FETCH_SUCCESS price={_marketData.CowPrice}, time{_priceCache.UTC}");
                PublishCurrentPrice();
                return true;
            }
            catch (OperationCanceledException)
            {
                // 취소는 실패 카운트 정책을 왜곡할 수 있으니 분리
                throw;
            }
            catch ( Exception ex)
            {
                _priceCache.fail++;
                _priceCache.failReason = ex.Message;
                _priceCache.UTC = DateTime.UtcNow;
                UnityEngine.Debug.LogWarning($"PRICE_FETCH_FAIL fail={_priceCache.fail} : reason={ex.Message}");
                return false;
            }
        }
        private void ApplyFallbackInitialPrice()
        {
            int fallback = _balance != null ? _balance.fallbackCowPrice : 2000;
            int price = _priceCache.cowPrice > 0 ? _priceCache.cowPrice : fallback;
            _marketData.CowPrice = price;
            _priceCache.cowPrice = price;

            UnityEngine.Debug.LogWarning($"PRICE_INIT_FALLBACK price{_marketData.CowPrice} reason={_priceCache.failReason} utc={_priceCache.UTC}");
            PublishCurrentPrice();

        }
        private void EnsureInitializedLocally()
        {
            if(_marketData.CowPrice <= 0)
            {
                ApplyFallbackInitialPrice();
                _initDone = true;   // 로컬에서 했지만 초기화는 진행했으니 true
            }
        }
        private int ApplyPercentBand(int cur, float maxPct)
        {
             int fallback = _balance != null ? _balance.fallbackCowPrice : 2000;
            int basePrice = cur > 0 ? cur : fallback;
            float delta = (UnityEngine.Random.value * 2f - 1f) * maxPct;    // -maxPct ~ +maxPct
            int next = (int)(basePrice * (1f + delta));
            return Math.Max(1, next);
        }
        public void PublishCurrentPrice()
        {
            if (_marketData.CowPrice <= 0)
                ApplyFallbackInitialPrice();

            OnUpdatedPrice?.Invoke(_marketData.CowPrice);
        }
        public bool TryBuy(int money, int curCowCap, int maxCowCap, ItemType type, out int cost, out string failReason)
        {

            failReason = "";
            cost = 0;
            if (CanBuy(money, type) == false)
            {
                failReason = "소지금이 부족하여 구매를 할 수 없습니다.";
                return false;
            }
            
            if(type == ItemType.Cow)
            {
                if(curCowCap >= (maxCowCap))
                {
                    failReason = "축사가 가득 차 구매가 안됩니다.";
                    return false;
                }
            }
            cost = -GetPrice(type);
            return true;

        }
        
        public bool TrySell(ItemType type, CowData cowData, int period, DayPhase phase, out int money, out string failReason)
        {
            
            failReason = "";
            money = 0;

            if (period < 2)
            {
                failReason = "소가 들어온지 얼마 지나지 않아서 판매가 불가합니다.";
                return false;
            }
            if(DayPhase.Morning < phase)
            {
                failReason = "경매는 오전에만 진행됩니다.";
                return false;
            }
          
            int price = EvaluateSellPrice(cowData).price;
            money = price;
            return true;
        }

      
        public string EndSessionReport(IReadOnlyDictionary<int, CowData> cows, int money, int cowCount, int day)
        {
            int cowPrice = GetPrice(ItemType.Cow);

            // 남은 소 청산가(예상) 합
            int liquidatationSum = 0;
            int baseLiquidatation = cowPrice * cowCount;      // 등급 반영 전 가격

            int a = 0, b = 0, c = 0;

            // 남아있는 소 중에 가장 비싼 소
            int topId = -1;
            char topGrade = 'F';
            int topPrice = 0;

            foreach(var kv in cows)
            {
                var cow = kv.Value;
                var v = EvaluateSellPrice(cow);

                liquidatationSum += v.price;

                if (v.grade == 'A')
                    a++;
                else if (v.grade == 'B')
                    b++;
                else
                    c++;

                if(v.price > topPrice)
                {
                    topId = cow.ID;
                    topGrade = v.grade;
                    topPrice = v.price;
                }
            }
            int premium = liquidatationSum - baseLiquidatation;
            int total = money + liquidatationSum;

            StringBuilder sb = new StringBuilder(256);
            sb.AppendLine($"{day}일 정산");
            sb.AppendLine($"현금 : {money}");
            sb.AppendLine($"남은 소 청산가(예상) : {liquidatationSum}");
            sb.AppendLine($"총 자산 : {total}");
            sb.AppendLine($"등급 프리미엄 : {(premium >= 0 ? "+" : "")}{premium} (기준 :{baseLiquidatation})");
            sb.AppendLine($"등급 분포 : A : {a} / B : {b} / C : {c}");
            sb.AppendLine($"남은 소 : {cowCount}");
            if (topId != -1)
                sb.AppendLine($"최고 가치 소 : ID{topId} / {topGrade} / {topPrice}");
            sb.AppendLine($"마지막 소 시세 : {cowPrice}");

            return (sb.ToString());
        }
       
       private (int price, char grade) EvaluateSellPrice(CowData cow)
       {
            // 체중 배율 : 임시로 현재 소의 몸무게와 비슷한 체급으로 배율을 책정
            // 330 이상이면 350으로 고정
            float weight = cow.Weight;
            float denom;

            var wc = _balance != null ? _balance.weightClass : null;
            float dLight = wc != null ? wc.denomLight : 310f;
            float dMid = wc != null ? wc.denomMid : 320f;
            float dHeavy = wc != null ? wc.denomHeavy : 330f;
            float dMax = wc != null ? wc.denomMax : 350f;

            if (weight < 310f)
                denom = dLight;
            else if (weight < 330f)
                denom = dMid;
            else if (weight < 350f)
                denom = dHeavy;
            else
                denom = dMax;

            float minF = wc != null ? wc.minFactor : 0.7f;
            float maxF = wc != null ? wc.maxFactor : 1.6f;
            float weightFactor = Math.Clamp(weight / denom, minF, maxF);

            // 등급 결정(보정) : 절대값이 아닌 구성비로 판정
            // fatPct = Fat / ( Muscle + Fat)
            float sum = Math.Max(1f, cow.Muscle + cow.Fat);
            float fatPct = cow.Fat / sum;


            var gt = _balance != null ? _balance.grade : null;
            float target = gt != null ? gt.targetFatPct : 0.4737f;
            float devA = gt != null ? gt.devA : 0.01f;
            float devB = gt != null ? gt.devB : 0.03f;
            float dev = Math.Abs(fatPct - target);

            char grade;
            float gradeMul;
            if(dev <= devA)
            {
                grade = 'A';
                gradeMul = gt != null ? gt.mulA : 1.2f;
            }
            else if(dev <= devB)
            {
                grade = 'B';
                gradeMul = gt != null ? gt.mulB : 0.9f;
            }
            else
            {
                grade = 'C';
                gradeMul = gt != null ? gt.mulC : 0.7f;
            }
            int basePrice = GetPrice(ItemType.Cow);
            int finalPrice = (int)(basePrice * weightFactor * gradeMul);

            return (finalPrice, grade);

       }
        private bool IsAuctionWindowKst()
        {
            DateTimeOffset kst = _time.UtcNow.ToOffset(_kstOffset);

            // 월 ~ 금
            var dow = kst.DayOfWeek;
            bool weekday = dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday;

            TimeSpan time = kst.TimeOfDay;
            bool isAution = time >= _auctionOpen && time <= _auctionClose;

            return weekday && isAution;
        }
        private bool CanBuy(int money, ItemType type)
        {
            int price = GetPrice(type);
            return money >= price;
        }
        private int GetPrice(ItemType type)
        {
            return type == ItemType.Cow ?
                _marketData.CowPrice :
                _marketData.FeedPrice;
        }
    
    }
}
