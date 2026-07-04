using System.Collections;
using UnityEngine;

// Trampa de ancla (Nivel 4): el ancla cuelga en lo alto. Cuando Ruli pasa por
// debajo, tiembla un momento (aviso, para darle tiempo de escapar) y luego cae.
// Solo mata si el ancla, mientras cae, alcanza de verdad a Ruli (solapamiento
// real con el sprite completo). Si escapa, golpea el suelo sin danio y, si
// 'reiniciar' esta activo, vuelve a subir para rearmar la trampa.
[RequireComponent(typeof(Rigidbody2D))]
public class AnclaTrampa : MonoBehaviour
{
    [Header("Deteccion")]
    [Tooltip("Medio ancho de la zona bajo el ancla que activa la trampa (unidades del mundo).")]
    public float radioDeteccionX = 1.5f;

    [Header("Tiempos")]
    [Tooltip("Cuanto tiembla el ancla antes de caer. Es el tiempo que Ruli tiene para escapar.")]
    public float tiempoAviso = 1f;
    [Tooltip("Si esta activo, el ancla vuelve a subir y se rearma despues de caer.")]
    public bool reiniciar = true;
    [Tooltip("Espera en el suelo antes de volver a subir (solo si 'reiniciar').")]
    public float tiempoReset = 3f;

    [Header("Caida")]
    [Tooltip("Aceleracion de la caida (como la gravedad).")]
    public float gravedad = 35f;
    [Tooltip("Velocidad maxima de caida.")]
    public float velocidadMaxCaida = 30f;
    [Tooltip("Velocidad a la que vuelve a subir al rearmarse.")]
    public float velocidadSubida = 8f;
    [Tooltip("Distancia de caida si el raycast no encuentra suelo.")]
    public float distanciaCaidaFallback = 10f;
    [Tooltip("Capas consideradas 'suelo' para el raycast de aterrizaje.")]
    public LayerMask capaSuelo = ~0;

    [Header("Temblor del aviso")]
    public float amplitudTemblor = 0.12f;
    public float frecuenciaTemblor = 30f;

    [Header("Audio (opcional)")]
    public AudioClip sonidoAviso;
    public AudioClip sonidoGolpe;

    private RuliMovimiento ruliMov;
    private Transform ruli;
    private Collider2D ruliCol;
    private SpriteRenderer sr;
    private ZonaMortal zonaMortal;
    private AudioSource audioSource;
    private Vector3 posOriginal;
    private float mitadAlto;
    private bool activada;

    void Awake()
    {
        posOriginal = transform.position;
        sr = GetComponent<SpriteRenderer>();
        mitadAlto = sr != null ? sr.bounds.extents.y : 1f;
        zonaMortal = GetComponent<ZonaMortal>();
        audioSource = GetComponent<AudioSource>();
        // El ancla NO usa ZonaMortal: la muerte la controla esta trampa con un
        // solapamiento real y solo durante la caida. Asi no mata "de lejos".
        if (zonaMortal != null) zonaMortal.enabled = false;
    }

    void Start()
    {
        ruliMov = FindFirstObjectByType<RuliMovimiento>();
        if (ruliMov != null)
        {
            ruli = ruliMov.transform;
            ruliCol = ruliMov.GetComponent<Collider2D>();
            if (ruliCol == null) ruliCol = ruliMov.GetComponentInChildren<Collider2D>();
        }
        else Debug.LogWarning("AnclaTrampa: no se encontro a Ruli en la escena.");
    }

    void Update()
    {
        if (activada || ruli == null) return;

        // Ruli debe estar dentro de la franja horizontal y COMPLETAMENTE debajo
        // del ancla (su parte de arriba por debajo del borde inferior del ancla).
        float anclaBottom = posOriginal.y - mitadAlto;
        float ruliTop = ruliCol != null ? ruliCol.bounds.max.y : ruli.position.y;
        bool debajo = ruliTop < anclaBottom;
        bool enFranja = Mathf.Abs(ruli.position.x - posOriginal.x) <= radioDeteccionX;
        if (debajo && enFranja)
            StartCoroutine(Caer());
    }

    IEnumerator Caer()
    {
        activada = true;

        // --- Aviso: el ancla tiembla en su sitio (tiempo para escapar) ---
        if (audioSource != null && sonidoAviso != null) audioSource.PlayOneShot(sonidoAviso);
        float t = 0f;
        while (t < tiempoAviso)
        {
            t += Time.deltaTime;
            float dx = Mathf.Sin(t * frecuenciaTemblor) * amplitudTemblor;
            transform.position = posOriginal + new Vector3(dx, 0f, 0f);
            yield return null;
        }
        transform.position = posOriginal;

        // --- Caida: acelera hacia abajo hasta el suelo, matando SOLO si de
        //     verdad alcanza a Ruli (solapamiento real con el sprite). ---
        float yObjetivo = CalcularSuelo();
        float vel = 0f;
        while (transform.position.y > yObjetivo)
        {
            vel = Mathf.Min(vel + gravedad * Time.deltaTime, velocidadMaxCaida);
            float ny = Mathf.Max(yObjetivo, transform.position.y - vel * Time.deltaTime);
            transform.position = new Vector3(posOriginal.x, ny, posOriginal.z);

            if (AlcanzaARuli())
            {
                ruliMov.Morir();
                break;
            }
            yield return null;
        }

        if (audioSource != null && sonidoGolpe != null) audioSource.PlayOneShot(sonidoGolpe);

        // --- Reinicio opcional: espera, sube y rearma ---
        if (reiniciar)
        {
            yield return new WaitForSeconds(tiempoReset);
            while (transform.position.y < posOriginal.y)
            {
                float ny = Mathf.Min(posOriginal.y, transform.position.y + velocidadSubida * Time.deltaTime);
                transform.position = new Vector3(posOriginal.x, ny, posOriginal.z);
                yield return null;
            }
            transform.position = posOriginal;
            activada = false;
        }
    }

    // El ancla alcanza a Ruli cuando el sprite visible del ancla se solapa con
    // el collider de Ruli (chequeo AABB, incluye toda el ancla, no solo el collider).
    bool AlcanzaARuli()
    {
        if (ruliMov == null || ruliCol == null || sr == null) return false;
        if (ruliMov.enabled == false) return false;
        return sr.bounds.Intersects(ruliCol.bounds);
    }

    // Busca el suelo justo debajo del ancla; devuelve la Y donde debe quedar el
    // centro del ancla (con su borde inferior apoyado en el suelo).
    float CalcularSuelo()
    {
        Vector2 origen = new Vector2(posOriginal.x, posOriginal.y - mitadAlto - 0.05f);
        var hits = Physics2D.RaycastAll(origen, Vector2.down, distanciaCaidaFallback + mitadAlto, capaSuelo);
        foreach (var h in hits)
        {
            if (h.collider.isTrigger) continue;
            if (h.collider.GetComponentInParent<RuliMovimiento>() != null) continue;
            if (h.collider.transform == transform) continue;
            return h.point.y + mitadAlto;
        }
        return posOriginal.y - distanciaCaidaFallback;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 c = Application.isPlaying ? posOriginal : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(c + Vector3.left * radioDeteccionX, c + Vector3.right * radioDeteccionX);
        Gizmos.DrawWireCube(c, new Vector3(radioDeteccionX * 2f, 0.2f, 0f));
    }
}
