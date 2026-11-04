using UnityEngine;

public class CollectibleVisual : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private float rotationSpeed = 70f;
    [SerializeField, Range(0f, 0.2f)] private float pulseAmount = 0.08f;
    [SerializeField, Min(0f)] private float pulseSpeed = 4f;
    private Vector3 initialScale;

    private void OnEnable()
    {
        if (visual != null) initialScale = visual.localScale;
    }

    private void Update()
    {
        if (visual == null) return;
        visual.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        visual.localScale = initialScale * (1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount);
    }

    private void OnDisable()
    {
        if (visual == null) return;
        visual.localScale = initialScale;
        visual.localRotation = Quaternion.identity;
    }
}
