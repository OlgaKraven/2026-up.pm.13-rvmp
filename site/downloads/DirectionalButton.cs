using UnityEngine;
using UnityEngine.EventSystems;

public class DirectionalButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Vector2 direction;
    public void OnPointerDown(PointerEventData data) { GameManager.Instance.SetDirection(direction); }
    public void OnPointerUp(PointerEventData data) { Clear(); }
    public void OnPointerExit(PointerEventData data) { Clear(); }
    private void OnDisable() { Clear(); }
    private void Clear()
    {
        if (GameManager.Instance != null) GameManager.Instance.SetDirection(Vector2.zero);
    }
}
