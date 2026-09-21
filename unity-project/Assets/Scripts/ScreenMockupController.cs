using UnityEngine;

public class ScreenMockupController : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject resultPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetOnly(mainMenuPanel);
        Time.timeScale = 0f;
    }

    public void ShowGame()
    {
        SetOnly(gamePanel);
        Time.timeScale = 1f;
    }

    public void ShowPause()
    {
        SetOnly(pausePanel);
        Time.timeScale = 0f;
    }

    public void ShowResult()
    {
        SetOnly(resultPanel);
        Time.timeScale = 0f;
    }

    private void SetOnly(GameObject activePanel)
    {
        GameObject[] panels =
        {
            mainMenuPanel, gamePanel, pausePanel, resultPanel
        };

        foreach (GameObject panel in panels)
            if (panel != null) panel.SetActive(panel == activePanel);
    }
}
