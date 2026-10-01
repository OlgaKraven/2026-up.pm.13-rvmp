using UnityEngine;
using UnityEngine.UI;

public class HUDView : MonoBehaviour
{
    public Text titleText, progressText, livesText, hintText;
    public Text resultTitle, resultMessage, menuTitle, menuGoal;

    public void ShowProgress(string title, string progress, int lives)
    {
        titleText.text = title;
        progressText.text = progress;
        livesText.text = "Жизни: " + lives;
    }

    public void ShowHint(string message, bool warning)
    {
        hintText.text = (warning ? "! " : "✓ ") + message;
        hintText.color = warning ? new Color(1f, 0.5f, 0.4f) : Color.white;
    }

    public void SetGoal(string title, string goal)
    {
        menuTitle.text = title;
        menuGoal.text = goal;
    }

    public void ShowResult(bool won)
    {
        resultTitle.text = won ? "УСПЕХ ✓" : "ПОПРОБУЙТЕ ЕЩЁ !";
        resultMessage.text = won ? "Цель выполнена. Проверьте повторный запуск." : "Ресурс исчерпан. Измените маршрут.";
    }
}
