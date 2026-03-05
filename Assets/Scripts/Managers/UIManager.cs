using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct StatePopup
{
    public GameObject CowStatPopup;
    public TMP_Text WeightText;
    public TMP_Text MuscleText;
    public TMP_Text FatText;
}
public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text TurnText;
    [SerializeField]
    private TMP_Text PlayerHpText;
    [SerializeField]
    private TMP_Text PlayerMoneyText;
    [SerializeField]
    private TMP_Text FeedStockText;
    [SerializeField]
    private TMP_Text MarketPriceText;
    [SerializeField]
    private TMP_Text PopupText;
    [SerializeField]
    private TMP_Text GameoverText;

    [SerializeField]
    private GameObject MarketPopup;
    [SerializeField]
    private GameObject GameoverPanel;
    [SerializeField] 
    private StatePopup StatePopup;
    

    
    // UI 갱신
    public void RefreshUI(DayPhase phase, int day, int hp, int money, int feedStock)
    {
        TurnText.text = $"{day.ToString()} Day : {phase}";
        PlayerHpText.text = $"체력\n{hp}";
        PlayerMoneyText.text = $"돈\n{money}";
        FeedStockText.text = $"먹이\n{feedStock}";
    }
    public void PopupMessage(string msg)
    {
        Animator anim = PopupText.GetComponent<Animator>();
        PopupText.text = msg ;
        anim.Play("FadeText",-1,0);
    }
    public void OnGameOverPanel(string message)
    {
        GameoverPanel.SetActive(true);
        GameoverText.text = message;
    }
    public void OnCowStatePopup(CowData cowData, Vector3 pos)
    {
        StatePopup.CowStatPopup.SetActive(true);
        StatePopup.CowStatPopup.transform.position = pos;
        StatePopup.WeightText.text = $"체중 : {cowData.Weight.ToString("N0")}Kg";
        StatePopup.MuscleText.text = $"근육량 : {cowData.Muscle.ToString("N0")}Kg";
        StatePopup.FatText.text = $"지방 : {cowData.Fat.ToString("N0")}Kg";
    }
    public void OffCowStatePopup(int id)=> StatePopup.CowStatPopup.SetActive(false);
    public void RefreshCowState(CowData cowData)
    {
        StatePopup.WeightText.text = $"체중 : {cowData.Weight.ToString("N0")}Kg";
        StatePopup.MuscleText.text = $"근육량 : {cowData.Muscle.ToString("N0")}Kg";
        StatePopup.FatText.text = $"지방 : {cowData.Fat.ToString("N0")}Kg";
    }
    public void RefreshMarketPrice(int marketPrice)
    {
        MarketPriceText.text = $"시세\n {marketPrice}";
    }
}
