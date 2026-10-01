using UnityEngine;

public class ScreenMockupController : MonoBehaviour
{
    public GameObject mainMenuPanel, gamePanel, pausePanel, resultPanel;

    public void ShowMainMenu() { SetOnly(mainMenuPanel); Time.timeScale = 0f; }
    public void ShowGame() { SetOnly(gamePanel); Time.timeScale = 1f; }
    public void ShowPause() { SetOnly(pausePanel); Time.timeScale = 0f; }
    public void ShowResult() { SetOnly(resultPanel); Time.timeScale = 0f; }

    private void SetOnly(GameObject selected)
    {
        foreach (GameObject panel in new[] { mainMenuPanel, gamePanel, pausePanel, resultPanel })
            panel.SetActive(panel == selected);
    }

    private void OnDestroy() { Time.timeScale = 1f; }
}
