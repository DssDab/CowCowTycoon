using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Button StartButton;
    [SerializeField] private Button ExitButton;


    private void Start()
    {
        if (StartButton != null)
            StartButton.onClick.AddListener(() => SceneManager.LoadScene("GameScene"));

        if (ExitButton != null)
            ExitButton.onClick.AddListener(ExitGame);
    }

    private void ExitGame()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
