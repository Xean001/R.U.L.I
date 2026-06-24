using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Parallax infinito por BLOQUES (no por sprites sueltos).
///
/// Cada hijo directo de este objeto es un "bloque" (puede contener 1 o muchos
/// SpriteRenderers de distinto ancho: una fila de casas, un muro + arena, etc.).
/// Los bloques deben ser copias iguales colocadas una al lado de la otra.
/// El script los recicla: cuando un bloque queda demasiado atrás respecto a la
/// cámara, salta al otro extremo, generando un scroll infinito sin huecos.
///
/// A diferencia de ParallaxInfinito, NO asume que todos los sprites midan lo
/// mismo: el periodo de repetición es el ancho total de UN bloque.
///
/// Pon este componente en el padre de cada capa de profundidad
/// (Cielo, Edificios, Frente, etc.) y dale el factorX que corresponda.
/// </summary>
public class ParallaxLoopInfinito : MonoBehaviour
{
    [Header("Cámara")]
    [SerializeField] private Transform camara;

    [Header("Velocidad (0 = scroll a velocidad real del mundo, 1 = pegado a la cámara)")]
    [SerializeField, Range(0f, 1f)] private float factorX = 0.2f;
    [SerializeField, Range(0f, 1f)] private float factorY = 0f;

    [Header("Loop infinito")]
    [SerializeField] private bool loopInfinito = true;

    [Tooltip("Ancho de un bloque (periodo de repetición). 0 = se calcula solo desde el primer bloque.")]
    [SerializeField] private float anchoBloque = 0f;

    private Vector3 camPosAnterior;
    private Transform[] bloques;
    private float[] offsetCentro; // (centro real del contenido) - (bloque.position.x)
    private float totalAncho;

    private void Start()
    {
        if (camara == null)
            camara = Camera.main != null ? Camera.main.transform : null;

        if (camara != null)
            camPosAnterior = camara.position;

        Inicializar();
    }

    private void Inicializar()
    {
        var lista = new List<Transform>();
        foreach (Transform hijo in transform)
            lista.Add(hijo);

        lista.Sort((a, b) => CentroX(a).CompareTo(CentroX(b)));
        bloques = lista.ToArray();

        offsetCentro = new float[bloques.Length];
        for (int i = 0; i < bloques.Length; i++)
            offsetCentro[i] = CentroX(bloques[i]) - bloques[i].position.x;

        float ancho = anchoBloque;
        if (ancho <= 0f && bloques.Length > 0)
            ancho = AnchoContenido(bloques[0]);

        totalAncho = ancho * Mathf.Max(1, bloques.Length);
    }

    private void LateUpdate()
    {
        if (camara == null || bloques == null || bloques.Length == 0) return;

        Vector3 delta = camara.position - camPosAnterior;
        transform.position += new Vector3(delta.x * factorX, delta.y * factorY, 0f);
        camPosAnterior = camara.position;

        if (!loopInfinito || totalAncho <= 0f) return;

        float mitad = totalAncho * 0.5f;
        for (int i = 0; i < bloques.Length; i++)
        {
            float centroActual = bloques[i].position.x + offsetCentro[i];
            float dist = camara.position.x - centroActual;

            if (dist > mitad)
                bloques[i].position += new Vector3(totalAncho, 0f, 0f);
            else if (dist < -mitad)
                bloques[i].position -= new Vector3(totalAncho, 0f, 0f);
        }
    }

    // --- Helpers de bounds ---------------------------------------------------

    private static float CentroX(Transform bloque)
    {
        if (TryBounds(bloque, out Bounds b))
            return b.center.x;
        return bloque.position.x;
    }

    private static float AnchoContenido(Transform bloque)
    {
        if (TryBounds(bloque, out Bounds b))
            return b.size.x;
        return 0f;
    }

    private static bool TryBounds(Transform bloque, out Bounds bounds)
    {
        var renderers = bloque.GetComponentsInChildren<SpriteRenderer>(true);
        bounds = default;
        bool primero = true;
        foreach (var r in renderers)
        {
            if (primero) { bounds = r.bounds; primero = false; }
            else bounds.Encapsulate(r.bounds);
        }
        return !primero;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            Inicializar();
    }
#endif
}
