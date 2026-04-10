using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.hasJewel)
            GameManager.Instance.PlayerWon();
        // Si no tiene la joya, no hace nada
    }
}