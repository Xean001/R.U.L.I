using UnityEngine;

public class HojaRecolectable : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MonedasManager.Instance.AgregarMoneda(1);

            Destroy(gameObject);
        }
    }
}