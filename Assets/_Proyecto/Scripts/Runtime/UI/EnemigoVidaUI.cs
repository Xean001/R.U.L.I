using UnityEngine;
using UnityEngine.UI;

// Barra de vida del enemigo (jefe) que aparece arriba al centro.
// Vive en el objeto del marco (siempre activo); se muestra/oculta con CanvasGroup
// y vacía la barra roja "Relleno" encogiendo su ancho (pivote a la izquierda).
[RequireComponent(typeof(CanvasGroup))]
public class EnemigoVidaUI : MonoBehaviour
{
    private CanvasGroup  grupo;
    private RectTransform relleno;
    private float        anchoMax;

    void Awake()
    {
        grupo = GetComponent<CanvasGroup>();

        var t = transform.Find("Relleno");
        if (t != null)
        {
            relleno  = t as RectTransform;
            anchoMax = relleno.sizeDelta.x;
        }

        grupo.alpha = 0f;   // oculta al inicio
    }

    public void Mostrar()
    {
        if (grupo != null) grupo.alpha = 1f;
    }

    public void Ocultar()
    {
        if (grupo != null) grupo.alpha = 0f;
    }

    // t = vida normalizada (0..1)
    public void SetVida(float t)
    {
        if (relleno == null) return;
        t = Mathf.Clamp01(t);
        Vector2 s = relleno.sizeDelta;
        s.x = anchoMax * t;
        relleno.sizeDelta = s;
    }
}
