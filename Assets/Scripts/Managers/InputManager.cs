using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Button FeedButton;
    [SerializeField]
    private Button RestButton;
    [SerializeField]
    private Button BuyCowButton;
    [SerializeField]
    private Button BuyFeedButton;
    [SerializeField]
    private Button SellButton;
    [SerializeField]
    private Button CloseStateButton;
    [SerializeField]
    private Button ReplayButton;
    [SerializeField]
    private Button GoTitleButton;

    public event Action<TrainingAction> OnAct;
    
    public event Action<int> OnClosedState;

    private void Start()
    {
        InitButton();
    }


    private void InitButton()
    {
        if (FeedButton != null)
            FeedButton.onClick.AddListener(OnFeedClicked);

        if (RestButton != null)
            RestButton.onClick.AddListener(OnRestClicked);

        if (BuyCowButton != null)
            BuyCowButton.onClick.AddListener(OnBuyCowClicked);

        if (BuyFeedButton != null)
            BuyFeedButton.onClick.AddListener(OnBuyFeedClicked);

        if (SellButton != null)
            SellButton.onClick.AddListener(OnSellCowClicked);

        if (ReplayButton != null)
            ReplayButton.onClick.AddListener(Replay);

        if (GoTitleButton != null)
            GoTitleButton.onClick.AddListener(GoTitle);

        if (CloseStateButton != null)
            CloseStateButton.onClick.AddListener(Close);

    }

    private void OnFeedClicked() => RunAction(TrainingAction.Feed);
    private void OnRestClicked() => RunAction(TrainingAction.Rest);
    private void OnBuyCowClicked() => RunAction(TrainingAction.BuyCow);
    private void OnBuyFeedClicked() => RunAction(TrainingAction.BuyFeed);
    private void OnSellCowClicked() => RunAction(TrainingAction.SellCow);

    private void Replay()
    {
        SceneManager.LoadScene("GameScene");
    }
    private void GoTitle()
    {
        SceneManager.LoadScene("Title");
    }

    private void Close()
    {
        var handler = OnClosedState;

        if (handler == null)
            return;

        OnClosedState?.Invoke(0);
    }
    private void RunAction(TrainingAction action)
    {

        var handler = OnAct;
        if (handler == null)
            return;
        
        handler.Invoke(action);
        
    }
  
}
