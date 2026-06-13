using UnityEngine;
using UnityEngine.UI;

public class VidaUI : MonoBehaviour
{
    public Image[] corazones;

    // Blanco = la poción se ve con sus colores reales; vacío = atenuada
    static readonly Color colorVivo  = Color.white;
    static readonly Color colorVacio = new Color(0.3f, 0.3f, 0.3f, 0.4f);

    void Start()
    {
        var salud = FindFirstObjectByType<RuliSalud>();
        if (salud == null) return;
        salud.OnVidaCambiada += Actualizar;
        Actualizar(salud.VidaActual, salud.vidaMaxima);
    }

    public void Actualizar(int actual, int maximo)
    {
        for (int i = 0; i < corazones.Length; i++)
            if (corazones[i] != null)
                corazones[i].color = i < actual ? colorVivo : colorVacio;
    }
}
