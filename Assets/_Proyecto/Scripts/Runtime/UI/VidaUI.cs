using UnityEngine;
using UnityEngine.UI;

public class VidaUI : MonoBehaviour
{
    public Image[] corazones;

    static readonly Color colorVivo  = new Color(0.95f, 0.1f, 0.1f);
    static readonly Color colorVacio = new Color(0.3f,  0.3f, 0.3f, 0.4f);

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
