using UnityEngine;

/// <summary>
/// Dispara la victoria cuando se han eliminado TODAS las cucarachas de la escena.
/// Usa el mismo VictoryController que el resto del juego.
/// </summary>
public class VictoriaCucarachas : MonoBehaviour
{
    [Tooltip("Cada cuánto revisa si quedan cucarachas (seg).")]
    [SerializeField] private float intervaloChequeo = 0.5f;

    private bool gano;
    private int totalInicial;
    private float timer;

    private void Start()
    {
        totalInicial = ContarCucarachas();
        if (totalInicial == 0)
            Debug.LogWarning("VictoriaCucarachas: no hay cucarachas en la escena al iniciar.");
    }

    private void Update()
    {
        if (gano || totalInicial == 0) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = intervaloChequeo;

        if (ContarCucarachas() == 0)
        {
            gano = true;
            var vc = FindFirstObjectByType<VictoryController>();
            if (vc != null)
            {
                vc.MostrarVictoria();
                Debug.Log("¡Todas las cucarachas eliminadas! Victoria.");
            }
            else
            {
                Debug.LogError("VictoriaCucarachas: no se encontró VictoryController en la escena.");
            }
        }
    }

    private static int ContarCucarachas()
    {
        return FindObjectsByType<EnemigoCucaracha>(FindObjectsSortMode.None).Length;
    }
}
