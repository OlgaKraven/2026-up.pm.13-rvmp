using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class Collectible : MonoBehaviour
{
    public int index;
    private void Awake() { GetComponent<Collider2D>().isTrigger = true; }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerController>() != null) GameManager.Instance.Collect(this);
    }
}
