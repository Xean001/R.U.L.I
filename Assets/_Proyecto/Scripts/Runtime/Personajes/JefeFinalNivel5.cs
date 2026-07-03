using UnityEngine;

// Jefe final del Nivel 5. Espera fuera de pantalla a la derecha;
// cuando Ruli entra a la arena, se desplaza hasta el centro y combate:
// animacion idle por frames, vaivén vertical leve y disparo de bolas de energia.
[RequireComponent(typeof(SpriteRenderer))]
public class JefeFinalNivel5 : MonoBehaviour
{
    [Header("Animacion idle")]
    public Sprite[] framesIdle;
    public float cuadrosPorSegundo = 8f;

    [Header("Entrada")]
    public float xActivacion = 72.2f;
    public Vector2 posicionCentro = new Vector2(77.2f, -1.4f);
    public float velocidadEntrada = 4f;

    [Header("Vuelo")]
    public float amplitudVuelo = 0.15f;
    public float frecuenciaVuelo = 2f;

    [Header("Ataque")]
    public Sprite spriteCarga;
    public Sprite spriteDisparo;
    public float intervaloDisparo = 3f;
    public float duracionCarga = 1f;
    public float velocidadBola = 8f;
    public float escalaBola = 0.6f;
    public Vector2 offsetBola = new Vector2(0.5f, 0f);

    private SpriteRenderer sr;
    private Transform jugador;
    private enum Estado { Esperando, Entrando, Combate }
    private Estado estado = Estado.Esperando;
    private float tiempoAnim;
    private int frameActual;
    private float tiempoDisparo;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        var ruli = FindFirstObjectByType<RuliMovimiento>();
        if (ruli != null) jugador = ruli.transform;
    }

    private void Update()
    {
        AnimarIdle();

        switch (estado)
        {
            case Estado.Esperando:
                if (jugador != null && jugador.position.x >= xActivacion)
                    estado = Estado.Entrando;
                break;

            case Estado.Entrando:
                Vector3 destino = new Vector3(posicionCentro.x, posicionCentro.y, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, destino, velocidadEntrada * Time.deltaTime);
                if (Vector3.Distance(transform.position, destino) < 0.01f)
                {
                    tiempoDisparo = 0f;
                    estado = Estado.Combate;
                }
                break;

            case Estado.Combate:
                float y = posicionCentro.y + Mathf.Sin(Time.time * frecuenciaVuelo) * amplitudVuelo;
                transform.position = new Vector3(posicionCentro.x, y, transform.position.z);

                tiempoDisparo += Time.deltaTime;
                if (tiempoDisparo >= intervaloDisparo)
                {
                    tiempoDisparo = 0f;
                    Disparar();
                }
                break;
        }
    }

    private void AnimarIdle()
    {
        if (framesIdle == null || framesIdle.Length == 0) return;

        tiempoAnim += Time.deltaTime;
        if (tiempoAnim >= 1f / cuadrosPorSegundo)
        {
            tiempoAnim = 0f;
            frameActual = (frameActual + 1) % framesIdle.Length;
            sr.sprite = framesIdle[frameActual];
        }
    }

    private void Disparar()
    {
        var go = new GameObject("BolaEnergia");
        go.transform.position = transform.position + (Vector3)offsetBola;
        go.transform.localScale = Vector3.one * escalaBola;
        var bola = go.AddComponent<BolaEnergia>();
        bola.Configurar(this, spriteCarga, spriteDisparo, duracionCarga, velocidadBola, sr.sortingLayerID, sr.sortingOrder + 1);
    }
}
