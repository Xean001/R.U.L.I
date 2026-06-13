using UnityEngine;

// Muestra los carteles del tutorial de uno en uno segun avanza el jugador.
// WASD -> (avanza) SALTAR -> (avanza) ATACAR -> (avanza) nada (empieza el nivel).
public class TutorialControles : MonoBehaviour
{
    public Transform jugador;
    public GameObject wasd;
    public GameObject saltar;
    public GameObject atacar;

    [Header("Umbrales en X (el jugador avanza a la derecha)")]
    public float xHastaWasd   = -42f;  // x <  esto         -> WASD
    public float xHastaSaltar = -26f;  // entre los dos     -> SALTAR
    public float xHastaFin    = -10f;  // entre/luego       -> ATACAR / nada

    void Update()
    {
        if (jugador == null) return;
        float x = jugador.position.x;
        Set(wasd,   x <  xHastaWasd);
        Set(saltar, x >= xHastaWasd   && x < xHastaSaltar);
        Set(atacar, x >= xHastaSaltar && x < xHastaFin);
    }

    void Set(GameObject g, bool visible)
    {
        if (g != null && g.activeSelf != visible) g.SetActive(visible);
    }
}
