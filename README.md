# CowCowTycoon

Unity 기반으로 제작한 **턴 기반 목장 경영 시뮬레이션 프로젝트**입니다.  
플레이어는 소를 관리하고 성장 상태와 시장 가격을 고려하여 판매 전략을 세우며 수익을 극대화하는 것을 목표로 합니다.

본 프로젝트는 게임 루프 설계, 데이터 구조 분리, 이벤트 기반 시스템 구성과 같은 **게임 클라이언트 개발의 아키텍처 설계 역량을 보여주는 것을 목표로 제작되었습니다.**

---

# 프로젝트 개요

- **장르** : 경영 시뮬레이션
- **개발 환경** : Unity / C#
- **플랫폼** : Windows
- **진행 방식** : 턴 기반 시스템

### 주요 특징

- 30일 세션 기반 게임 진행
- 하루 4 Phase 구조의 턴 시스템
- 소 성장 상태 관리 (체중 / 지방 / 근육 / 스트레스)
- 외부 API 기반 시장 가격 시스템
- 네트워크 오류 대비 fallback 정책 적용
- JSON 기반 Save / Load 시스템

---

# 시스템 아키텍처

본 프로젝트는 **Event 기반 구조**를 중심으로 설계되었습니다.  
핵심 게임 로직은 `TrainingSession`에서 관리하며 각 시스템은 이벤트를 통해 연결됩니다.
```
InputManager
↓
GameManager
↓
TrainingSession
↓
┌───────────────┐
MarketSystem SaveSystem
Spawner CowRegistry
UIManager
```


핵심 흐름


Input → TrainingSession → Event → Systems


이 구조를 통해 **게임 로직과 시스템을 분리하고 결합도를 낮추는 것을 목표로 설계했습니다.**

---

# 핵심 시스템

## TrainingSession

`TrainingSession`은 게임의 핵심 로직을 담당하는 시스템입니다.

### 주요 역할

- 플레이어 액션 처리
- Day / Phase 전환
- 게임 상태 업데이트
- 이벤트 발행

### 핵심 코드

```csharp
public ActionResult ExecuteAction(PlayerAction action)
{
    switch(action)
    {
        case PlayerAction.Feed:
            FeedCow();
            break;

        case PlayerAction.Rest:
            RestCow();
            break;

        case PlayerAction.Sell:
            TrySell();
            break;
    }

    AdvancePhase();
}
```
플레이어의 행동을 처리하고 게임 상태를 갱신하는 핵심 루프입니다.

GameManager

GameManager는 게임 시스템을 연결하는 중앙 관리 역할을 담당합니다.

주요 역할

시스템 초기화

이벤트 연결

UI 업데이트

Save / Load 처리

GameManager는 게임 로직을 직접 처리하지 않고
각 시스템을 연결하는 역할에 집중하도록 설계했습니다.

## MarketSystem

MarketSystem은 소의 시장 가격을 관리하는 시스템입니다.

### 주요 기능

게임 시작 시 외부 API를 통해 시세 조회

조회 실패 시 fallback 가격 적용

Morning Phase마다 가격 변동 시뮬레이션
``` csharp
핵심 코드
public async Task InitPriceAsync()
{
    bool success = await TryFetchAndApplyAsync();

    if(!success)
    {
        ApplyFallbackInitialPrice();
    }
}
```
외부 API 요청 실패 시 잘못된 가격이 적용되는 것을 방지하기 위해
응답 값 검증 로직을 추가했습니다.

if (amt <= 0)
{
    return false;
}
## 외부 시세 시스템

게임 시작 시 외부 API를 통해 소 경락 시세를 조회합니다.

초기화 흐름

StartSession
   ↓
MarketSystem.InitPriceAsync
   ↓
외부 가격 조회
성공
PRICE_FETCH_SUCCESS
실패
PRICE_FETCH_FAIL
PRICE_INIT_FALLBACK
설계 의도

네트워크 오류가 발생하더라도 게임 진행이 중단되지 않도록
fallback 가격을 적용하도록 설계했습니다.

또한 가격 조회 성공 여부는 플레이어 UI에 표시하지 않고
개발 검증을 위한 로그로만 기록하도록 구성했습니다.

## Save / Load 시스템

게임 상태는 JSON 형태로 저장됩니다.

저장 데이터

Player 자산

Farm 상태

Cow 목록

Day / Phase
``` csharp
핵심 코드
public void Save(SaveData data)
{
    string json = JsonUtility.ToJson(data);
    File.WriteAllText(savePath, json);
}
```
저장 시점
NextDay 발생 시 자동 저장

이를 통해 게임 진행 상태를 안정적으로 복원할 수 있도록 구성했습니다.

전략 요소

플레이어는 소의 성장 상태와 시장 가격을 고려하여 판매 전략을 선택할 수 있습니다.

전략 A (고점 판매)

지방 비율을 목표 범위에 맞춰 높은 등급으로 판매하는 전략입니다.

특징

높은 수익 가능

성장 관리 필요

전략 B (회전 전략)

중간 등급에서 빠르게 판매하여 안정적으로 자금을 확보하는 전략입니다.

특징

안정적인 수익

리스크 낮음

전략 C (방치 전략)

먹이를 주지 않을 경우 체중과 성장 수치가 감소합니다.

특징

판매 가치 감소

장기적으로 수익 감소

테스트
ID	테스트 항목	결과
T01	초기 실행	소 1마리 생성
T02	외부 가격 조회 성공	정상 시세 적용
T03	외부 가격 조회 실패	fallback 가격 적용
T04	Feed 액션	체중 증가
T05	Rest 액션	스트레스 감소
T06	Sell 성공	자산 증가
T07	Sell 실패 (시간 제한)	판매 불가
T08	Day 전환	저장 발생
T09	Save / Load	상태 복원
T10	UI 갱신	정상 표시
T11	Cow Spawn	정상 생성
T12	Cow Select	팝업 표시
T13	Market 가격 변동	Morning 갱신
T14	Registry 관리	정상 등록
T15	마지막 Day 종료	세션 종료
실행 방법

빌드된 실행 파일을 실행합니다.

게임을 시작합니다.

턴을 진행하며 소를 관리합니다.

시장 가격을 고려하여 판매 전략을 선택합니다.

스크린샷
Title

InGame

Cow Status

Sell

Load

개발 환경

Unity

C#

JSON Save System

Event 기반 게임 구조

프로젝트 목적

본 프로젝트는 Unity 기반 게임 클라이언트 개발 과정에서
게임 시스템 설계, 이벤트 기반 구조, 데이터 관리, 외부 API 처리 등
핵심적인 개발 역량을 학습하고 정리하기 위해 제작되었습니다.
