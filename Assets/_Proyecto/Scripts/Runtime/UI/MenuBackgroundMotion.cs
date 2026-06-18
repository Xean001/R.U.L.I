using UnityEngine;

public class MenuBackgroundMotion : MonoBehaviour
{
    [Header("Referencia al fondo")]
    public RectTransform fondoImagen;

    [Header("Movimiento Circular")]
    public float radioMovimiento = 30f;        // Qué tan grande es el círculo
    public float velocidadRotacion = 0.5f;     // Velocidad del giro (radianes por segundo)
    public float faseInicial = 0f;             // Punto de inicio del círculo

    [Header("Opcional: Movimiento en Y")]
    public bool movimientoEnY = true;          // Si false, solo se mueve en X
    public float amplitudY = 15f;              // Cuánto se mueve en Y

    private Vector3 posicionOriginal;
    private float tiempoTranscurrido;

    private void Start()
    {
        if (fondoImagen == null)
        {
            Debug.LogError("¡Asigna el fondo_imagen en el Inspector!");
            return;
        }

        posicionOriginal = fondoImagen.anchoredPosition;
        tiempoTranscurrido = faseInicial;
    }

    private void Update()
    {
        if (fondoImagen == null) return;

        tiempoTranscurrido += Time.deltaTime * velocidadRotacion;

        // Calcular posición circular
        float x = Mathf.Cos(tiempoTranscurrido) * radioMovimiento;
        float y = movimientoEnY ? Mathf.Sin(tiempoTranscurrido) * amplitudY : 0f;

        fondoImagen.anchoredPosition = posicionOriginal + new Vector3(x, y, 0f);
    }
}