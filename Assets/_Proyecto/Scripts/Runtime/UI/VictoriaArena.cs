using UnityEngine;

/// <summary>
/// Dispara la victoria cuando se han eliminado TODOS los EnemigoArena de la escena.
/// Usa VictoriaNivel si existe (desbloquea el siguiente nivel y va al menu de
/// Niveles); si no, recurre a VictoryController.
/// </summary>
public class VictoriaArena : MonoBehaviour
{
    [Tooltip("Cada cuanto revisa si quedan enemigos (seg).")]
    [SerializeField] private float intervaloChequeo = 0.5f;

    private bool gano;
    private int totalInicial;
    private float timer;

    private void Start()
    {
        totalInicial = ContarArena();
        if (totalInicial == 0)
            Debug.LogWarning("VictoriaArena: no hay EnemigoArena en la escena al iniciar.");
    }

    private void Update()
    {
        if (gano || totalInicial == 0) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = intervaloChequeo;

        if (ContarArena() == 0)
        {
            gano = true;

            // Igual que Nivel2/Nivel4: usa el VictoryController (pantalla con
            // boton "Siguiente Nivel" que carga el nivel siguiente directo).
            var vc = FindFirstObjectByType<VictoryController>();
            if (vc != null)
            {
                vc.MostrarVictoria();
                Debug.Log("¡Todos los enemigos de arena eliminados! Victoria.");
                return;
            }

            var vn = FindFirstObjectByType<VictoriaNivel>();
            if (vn != null)
            {
                vn.Ganar();
                Debug.Log("¡Todos los enemigos de arena eliminados! Victoria.");
            }
            else
            {
                Debug.LogError("VictoriaArena: no se encontro VictoryController ni VictoriaNivel.");
            }
        }
    }

    private static int ContarArena()
    {
        return FindObjectsByType<EnemigoArena>(FindObjectsSortMode.None).Length;
    }
}
