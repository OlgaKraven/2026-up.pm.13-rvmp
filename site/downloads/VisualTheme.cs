using UnityEngine;
using UnityEngine.UI;

public class VisualTheme : MonoBehaviour
{
    [Header("Палитра: роль цвета сохраняется на всех экранах")]
    public Color background = new Color(0.035f, 0.055f, 0.10f);
    public Color surface = new Color(0.07f, 0.10f, 0.18f);
    public Color player = new Color(0.19f, 0.88f, 1f);
    public Color target = new Color(1f, 0.78f, 0.16f);
    public Color danger = new Color(1f, 0.25f, 0.28f);
    public Color success = new Color(0.25f, 0.82f, 0.45f);
    public Color text = Color.white;
    public Color accent = new Color(0.16f, 0.55f, 0.75f);

    public void Apply()
    {
        Camera.main.backgroundColor = background;
        foreach (ThemeRole item in FindObjectsByType<ThemeRole>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Color color = item.role switch
            {
                ThemeRole.Role.Player => player,
                ThemeRole.Role.Target => target,
                ThemeRole.Role.Danger => danger,
                ThemeRole.Role.Success => success,
                ThemeRole.Role.Text => text,
                _ => surface
            };
            if (item.TryGetComponent(out SpriteRenderer sprite)) sprite.color = color;
            if (item.TryGetComponent(out Graphic graphic)) graphic.color = color;
            if (item.TryGetComponent(out Button button))
            {
                if (item.TryGetComponent(out Graphic buttonGraphic)) buttonGraphic.color = Color.white;
                ColorBlock states = button.colors;
                states.normalColor = surface;
                states.highlightedColor = accent;
                states.pressedColor = success;
                button.colors = states;
            }
        }
    }
}
