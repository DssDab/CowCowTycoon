### 2D 프로젝트 CowCowTycoon : 턴제 목장 경영 시뮬레이션 포트폴리오 

### 프로젝트 소개
Unity 6.1 / C#로 제작한 **턴제 목장 경영 시뮬레이션**입니다.  
플레이어는 제한된 자원(체력/자금/사료)과 **시장 시세**, **판매 가능(Morning/오전)** 제약을 고려하여 행동(급여/휴식/구매/판매)을 선택하고, **세션 종료 시 총 자산(현금 + 보유 소 청산가)** 을 극대화하는 것을 목표로 설계했습니다.

- **Session**: 기본 7일  
- **Turn**: 1일 × 4 Phase (`Morning → Afternoon → Evening → Night`)  
- **Action**: `Feed / Rest / BuyCow / BuyFeed / SellCow`  

* * *

### 개발 기간
- `2025.09.24 ~ 2026.03.06` *(총 6개월)*  

* * *

### 사용 기술 / 적용 패턴
- **Unity 6.1 / C#**
- **옵저버(Observer) 패턴 / Event-driven**
  - `TrainingSession`이 이벤트 발행 → `GameManager`가 씬에서 구독하여 `UI/Spawner/Save/Market/CowRegistry`를 연결
- **상태 머신(State Machine)**
  - `DayPhase(Morning/Afternoon/Evening/Night)` 기반으로 턴/일 진행을 제어
- **Repository 패턴**
  - `SaveDataRepository`: 저장 파일 I/O 전담 (`GameSaveData.json`)
- **Mapper(매핑 계층) 패턴**
  - `SaveMapper`: 런타임 상태 ↔ 저장 DTO 변환(저장 포맷 변경 영향 최소화)
- **Registry 패턴**
  - `CowRegistry`: `CowController`를 ID 기반으로 등록/선택/팝업 이벤트 중계
- **Composition Root(조립 지점 집중)**
  - `GameManager`가 초기화/이벤트 wiring을 한 곳에서 수행(의존 방향 통제)

* * *

### 주요 기능 및 기술적 특징
#### 1) 턴 기반 루프(4 Phase) + 제약 기반 의사결정
- HP/사료/판매 시간 제약을 통해 **계획 플레이**가 되도록 구성
- `Night → NextDay`에서 성장/시세 갱신/저장을 묶어 “하루 단위 체크포인트”로 정리

#### 2) 외부 시세 연동: 세션 시작 1회 조회 + 실패/시간 외 fallback
- 외부 시세는 **초기 기준점(앵커)** 으로만 사용
- 조회 실패/경매 시간 외에도 게임 진행이 멈추지 않도록 **fallback** 적용
- **검증(직접 확인)**: Unity Editor(KST)에서  
  - 09:00~11:30 내 실행 시 `PRICE_FETCH_SUCCESS` 로그 확인  
  - 시간 경과 후 재실행 시 `INIT_FALLBACK : outside auction window` + `PRICE_INIT_FALLBACK` 적용 확인  

#### 3) 판매 Morning 제한(도메인 룰 반영)
- 실제 경매 시간이 오전이라는 전제 반영 → **Morning에만 판매 가능**
- **검증(직접 확인)**: Morning 외 Phase에서 판매 시 실패 메시지 출력 및 금액 미변동 확인
- 판매 후에는 OnSold 이벤트 체인으로 데이터 제거, Registry 해제, 오브젝트 제거, 팝업 종료를 묶어 후처리 누락 가능성을 줄임

#### 4) 자동 저장: Night → NextDay 1회 저장
- 매 행동 저장 대신 “하루 종료”에만 저장하여 저장 비용을 통제
- **검증(직접 확인)**: `Night → NextDay`에서만 `GameSaveData.json` 필드값 변경 확인

* * *

### 개발 과정에서 배운 점
- **이벤트 기반 설계의 중요성** :  `TrainingSession`이 게임 규칙/상태 변화를 이벤트로만 발행하고, `GameManager`가 씬에서 구독하도록 분리하니 UI/스폰/저장 로직을 바꿔도 코어 규칙 수정이 최소화되어 디버깅 범위가 좁아진다는 것 체감했습니다.

  ---
- **실패 경로를 정상 플로우에 포함 시키는 설계** : 외부 시세 조회는 경매 시간 조건 + 1회 시도 + 실패 시 fallback을 기본 흐름으로 두고, 네트워크 실패/타임아웃이 나도 게임 진행이 멈추지 않게 만드는 것이 사용자 경험과 안정성에 핵심이라는걸 배웠습니다.

  ---
- **저장/로드 경계 분리의 효과** : 런타임 상태를 SaveMapper로 DTO(player->farm->market->session)순서로 변환하고, SaveDataRepository가 JSON 직렬화와 파일 I/O만 담당하도록 분리했습니다. 또한 저장 데이터 복원은 player->farm->market->session 순서로 적용해 UI 갱신 시점과 실제 데이터 상태가 어긋나는 문제를 줄였고, 이를 통해 저장 포맷 변경 시 수정 범위를 저장 계층으로 한정할 수 있었습니다.

  ---
- **상태머신 기반 턴 루프의 명확성** : `DayPhase` 전환을 `NextPhase()`로 통제하고, `Night -> NextDay`에서 성장/시세 갱신/저장 을 한 지점에 묶으니까 게임 진행 흐름이 예측 가능해지고 버그 재현이 쉬워진다는걸 배웠습니다.
  

* * *

### 실행 방법 (Unity Editor)
1. Unity Hub에서 프로젝트를 열고 **Unity 6.1**로 실행합니다.
2. 시작 씬(예: `SampleScene` 또는 프로젝트의 메인 씬)을 엽니다.
3. Hierarchy에서 `GameManager`가 존재하는지 확인합니다.
4. 상단 **Play** 버튼을 눌러 실행합니다.
5. 인게임 UI에서 `Feed/Rest/Buy/Sell`을 선택하며 턴을 진행합니다.

- 저장 파일 위치: `Application.persistentDataPath/GameSaveData.json`
