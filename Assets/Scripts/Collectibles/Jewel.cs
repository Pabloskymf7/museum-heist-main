using UnityEngine;

public class Jewel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.hasJewel = true;
        AudioManager.Instance?.PlayJewelPickup();
        Destroy(gameObject);
    }
}
