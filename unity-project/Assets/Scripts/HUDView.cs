using UnityEngine;
using UnityEngine.UI;

public class HUDView : MonoBehaviour
{
    [Header("Текстовые поля HUD")]
    [SerializeField] private Text collectibleText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text hintText;

    [Header("Панель результата")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultTitle;
    [SerializeField] private Text resultMessage;

    public void ShowProgress(int collected, int target, int lives)
    {
        collectibleText.text = $"Кристаллы: {collected}/{target}";
        livesText.text = $"Жизни: {lives}";
    }

    public void ShowHint(string message)
    {
        hintText.text = message;
        hintText.gameObject.SetActive(!string.IsNullOrWhiteSpace(message));
    }

    public void ShowResult(bool won)
    {
        resultPanel.SetActive(true);
        resultTitle.text = won ? "УРОВЕНЬ ПРОЙДЕН" : "ПОПРОБУЙТЕ ЕЩЁ";
        resultMessage.text = won
            ? "Все предметы собраны, выход открыт."
            : "Попытки закончились. Измените маршрут.";
    }

    public void HideResult()
    {
        resultPanel.SetActive(false);
    }
}
