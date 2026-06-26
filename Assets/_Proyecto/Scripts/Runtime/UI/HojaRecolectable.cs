using UnityEngine;

public class HojaRecolectable : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Cuenta por-partida; se banca al ganar el nivel (anti-farmeo).
            ContadorMonedas.Instance?.Agregar(1);

            Destroy(gameObject);
        }
    }
}