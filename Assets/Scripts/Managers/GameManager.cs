using UnityEngine;
using Game.System;
using Assets.Scripts.Systems;
using System;
using Assets.Scripts.Utility;


public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    // 게임에서 주로 사용할 데이터들
    private PlayerData m_playerData;
    private FarmData m_farmData;
    private TrainingSession m_trainingSession;
    private MarketSystem m_marketSystem;
    private CowRegistry m_cowRegistry;
    private SaveSystem m_saveSystem;

    // 리소스, 컴포넌트
    [SerializeField] private BalanceConfig m_balance;
    [SerializeField] private NetworkConfig m_network;
    [SerializeField] private GameObject m_CowPrefab;
    [SerializeField] private InputManager m_Input;
    [SerializeField] private UIManager m_UiManager;
    [SerializeField] private Spawner m_spawner;



    private void Awake()
    {
        if(instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        GameInitialize();
    }
    private void OnEnable()
    {
        m_spawner.OnSpawned += m_cowRegistry.Register;

        m_cowRegistry.OnCowSelected += m_farmData.SelectCowID;
        m_cowRegistry.OnPopuped += m_UiManager.OnCowStatePopup;

        m_Input.OnAct += m_trainingSession.ExecuteAction;
        m_Input.OnClosedState += m_UiManager.OffCowStatePopup;

        m_marketSystem.OnUpdatedPrice += m_UiManager.RefreshMarketPrice;

        m_trainingSession.OnSessionUpdated += m_UiManager.RefreshUI;
        m_trainingSession.OnDayChanged += m_UiManager.RefreshCowState;
        m_trainingSession.OnFed += m_farmData.FeedingCow;

        m_trainingSession.OnCowSpawned += m_spawner.TrySpawnCow;
        m_trainingSession.OnActionFeedback += m_UiManager.PopupMessage;
        m_trainingSession.OnSold += m_farmData.ClearCowData;
        m_trainingSession.OnSold += m_cowRegistry.DeRegister;
        m_trainingSession.OnSold += m_spawner.DeSpawnCow;
        m_trainingSession.OnSold += m_UiManager.OffCowStatePopup;

        m_trainingSession.OnSaved += m_saveSystem.Save;

        m_trainingSession.OnSessionEnded += m_UiManager.OnGameOverPanel;
        m_trainingSession.OnSessionEnded += m_saveSystem.DeleteSaveData;

        
    }
    async void Start()
    {
        if(m_saveSystem.TryLoad(m_trainingSession, m_playerData, m_farmData, m_marketSystem._marketData) == true)
        {
            m_spawner.Initialize(m_CowPrefab, m_farmData.MaxCowStock);
            m_trainingSession.SessionLoad();
        }
        else
        {
            try
            {
                m_spawner.Initialize(m_CowPrefab, m_farmData.MaxCowStock);
                await m_trainingSession.StartSessionAsync();
            }
            catch(Exception ex)
            {
                Debug.LogWarning(ex.Message);
            }
        }
         
    }

    private void GameInitialize()
    {
        m_playerData = new PlayerData();
        m_farmData = new FarmData();

        m_marketSystem = new MarketSystem(new SystemClock(), m_balance, m_network);
        m_trainingSession = new TrainingSession(m_farmData, m_marketSystem, m_playerData, m_balance, m_network);

        m_cowRegistry = new CowRegistry();

        m_saveSystem = new SaveSystem(new SaveMapper(), new SaveDataRepository());

        m_playerData.Init();
        m_farmData.Init(m_balance);

        
    }
    private void OnDisable()
    {
        m_spawner.OnSpawned -= m_cowRegistry.Register;

        m_cowRegistry.OnCowSelected -= m_farmData.SelectCowID;
        m_cowRegistry.OnPopuped -= m_UiManager.OnCowStatePopup;

        m_Input.OnAct -= m_trainingSession.ExecuteAction;
        m_Input.OnClosedState -= m_UiManager.OffCowStatePopup;

        m_marketSystem.OnUpdatedPrice -= m_UiManager.RefreshMarketPrice;

        m_trainingSession.OnFed -= m_farmData.FeedingCow;
        m_trainingSession.OnSessionUpdated -= m_UiManager.RefreshUI;
        m_trainingSession.OnCowSpawned -= m_spawner.TrySpawnCow;
        m_trainingSession.OnActionFeedback -= m_UiManager.PopupMessage;

        m_trainingSession.OnDayChanged -= m_UiManager.RefreshCowState;

        m_trainingSession.OnSold -= m_farmData.ClearCowData;
        m_trainingSession.OnSold -= m_cowRegistry.DeRegister;
        m_trainingSession.OnSold -= m_spawner.DeSpawnCow;
        m_trainingSession.OnSold -= m_UiManager.OffCowStatePopup;

        m_trainingSession.OnSaved -= m_saveSystem.Save;
        m_trainingSession.OnSessionEnded -= m_UiManager.OnGameOverPanel;
        m_trainingSession.OnSessionEnded -= m_saveSystem.DeleteSaveData;
    }

}
