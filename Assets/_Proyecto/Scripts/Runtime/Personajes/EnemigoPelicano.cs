using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemigoPelicano : MonoBehaviour
{
    [Header("Detección")]
    public float rangoDeteccion = 10f;
    public LayerMask capaSuelo;
    public string tagSuelo = "piso";

    [Header("Ataque")]
    public float duracionAtaque = 2f;
    public float velocidadAtaque = 3f;
    public float fuerzaEmpuje = 5f;
    public float tiempoEntreAtaques = 1f;

    [Header("Vuelo")]
    public float tiempoMaximoVuelo = 3f;
    public float velocidadDescenso = 2f;

    [Header("Vida")]
    public int vidaMaxima = 4;

    [Header("Audio")]
    public AudioClip sonidoGirar;
    public AudioClip sonidoMuerte;
    private AudioSource audioSource;

    private static readonly Color[] tintes =
    {
        Color.white,
        new Color(1f, 0.65f, 0.3f),
        new Color(1f, 0.25f, 0.1f),
        new Color(1f, 0f, 0f),
    };

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Transform jugadorTrans;
    private RuliSalud jugadorSalud;

    private int vida;
    private int golpes;
    private bool muerto;
    private bool atacando;
    private bool volando;
    private float tiempoAtaque;
    private bool yaSonoGirar;
    private bool puedeAtacar = true;

    // Control de vuelo
    private float tiempoVueloInicio;
    private bool descendiendo;
    private bool congeladoEnAire;  // ← NUEVO: para quedarse tieso en el aire

    // Line of sight
    private Vector2 direccionJugador;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        vida = vidaMaxima;
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        var jugador = FindFirstObjectByType<RuliMovimiento>();
        if (jugador != null)
        {
            jugadorTrans = jugador.transform;
            jugadorSalud = jugador.GetComponent<RuliSalud>();
        }
    }

    void Update()
    {
        if (muerto) return;

        // Control de vuelo congelado
        if (congeladoEnAire && volando)
        {
            float tiempoVolando = Time.time - tiempoVueloInicio;

            // Si ya pasó el tiempo máximo y NO detecta al jugador → caer
            if (tiempoVolando >= tiempoMaximoVuelo)
            {
                if (!TieneLineaDeVision())
                {
                    EmpezarDescenso();
                }
                else
                {
                    // Si detecta al jugador → puede atacar de nuevo
                    congeladoEnAire = false;
                    puedeAtacar = true;
                }
            }
            else
            {
                // Mientras está congelado, seguir detectando al jugador
                if (TieneLineaDeVision() && puedeAtacar)
                {
                    congeladoEnAire = false;
                    StartCoroutine(SecuenciaAtaque());
                }
            }
        }
        else if (!atacando && !congeladoEnAire)
        {
            // Detectar jugador con línea de visión (solo si no está atacando ni congelado)
            if (jugadorTrans != null && puedeAtacar)
            {
                if (TieneLineaDeVision())
                {
                    StartCoroutine(SecuenciaAtaque());
                }
            }
        }
    }

    bool TieneLineaDeVision()
    {
        if (jugadorTrans == null) return false;

        float distancia = Vector2.Distance(transform.position, jugadorTrans.position);
        if (distancia > rangoDeteccion) return false;

        // Calcular dirección al jugador
        direccionJugador = (jugadorTrans.position - transform.position).normalized;

        // Raycast para detectar obstáculos
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direccionJugador,
            distancia,
            capaSuelo
        );

        // Si no hay obstáculos o el obstáculo es el jugador
        if (hit.collider == null) return true;

        var jugadorHit = hit.collider.GetComponent<RuliMovimiento>();
        return jugadorHit != null;
    }

    void EmpezarDescenso()
    {
        descendiendo = true;
        congeladoEnAire = false;
        // Mantener animación de vuelo mientras desciende

        // Reactivar gravedad para que caiga
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.WakeUp();

        anim.Play("idle_pelicano", 0, 0f);
    }

    IEnumerator SecuenciaAtaque()
    {
        atacando = true;
        puedeAtacar = false;
        tiempoAtaque = 0f;
        yaSonoGirar = false;

        // DESPERTAR Rigidbody
        rb.WakeUp();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reproducir sonido
        if (sonidoGirar != null && !yaSonoGirar)
        {
            audioSource.PlayOneShot(sonidoGirar);
            yaSonoGirar = true;
        }

        // Forzar animación de ataque
        anim.Play("atack_pelicano", 0, 0f);

        // Girar durante 2 segundos persiguiendo al jugador
        while (tiempoAtaque < duracionAtaque)
        {
            tiempoAtaque += Time.deltaTime;

            if (jugadorTrans != null)
            {
                Vector2 direccion = (jugadorTrans.position - transform.position).normalized;
                rb.linearVelocity = direccion * velocidadAtaque;
                sr.flipX = jugadorTrans.position.x < transform.position.x;
            }

            yield return null;
        }
        
        // DETENER movimiento inmediatamente
        rb.linearVelocity = Vector2.zero;
        atacando = false;

        // ←←← VERIFICAR INMEDIATAMENTE si está en el aire
        bool enAire = EstaEnAire();

        if (enAire)
        {
            // ←←← EN EL AIRE: IR DIRECTO A VUELO (sin pasar por idle)
            volando = true;
            congeladoEnAire = true;
            descendiendo = false;
            tiempoVueloInicio = Time.time;

            // Congelar posición
            rb.Sleep();
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

            if(jugadorTrans != null)
            {
                anim.Play("vuelo_pelicano", 0, 0f);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                atacando = false;
                anim.Play("idle_pelicano", 0, 0f);
            }

            // ←←← TRANSICIÓN DIRECTA: attack → vuelo (usando el trigger)
            
        }
        else
        {
            // ←←← EN EL SUELO: idle normal
            volando = false;
            congeladoEnAire = false;
            anim.Play("idle_pelicano", 0, 0f);
        }

        // Esperar cooldown
        yield return new WaitForSeconds(tiempoEntreAtaques);

        puedeAtacar = true;
    }

    bool EstaEnAire()
    {
        // Raycast hacia abajo para detectar suelo
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            0.6f,
            capaSuelo
        );

        return hit.collider == null;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (muerto || !atacando) return;

        // Si toca al jugador durante el ataque, bajar vida
        var salud = col.gameObject.GetComponent<RuliSalud>();
        if (salud != null)
        {
            salud.RecibirDaño();
        }

        // Si toca suelo mientras vuela/desciende
        if (volando && col.gameObject.CompareTag(tagSuelo))
        {
            Aterrizar();
        }
    }

    void Aterrizar()
    {
        volando = false;
        congeladoEnAire = false;
        descendiendo = false;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.Sleep();
        anim.Play("idle_pelicano", 0, 0f);
    }

    public void Golpe()
    {
        if (muerto) return;

        golpes++;
        sr.color = tintes[Mathf.Clamp(golpes, 0, tintes.Length - 1)];

        // Empuje hacia atrás
        Vector2 direccionEmpuje = transform.right;
        if (sr.flipX) direccionEmpuje = -transform.right;
        rb.linearVelocity = -direccionEmpuje * fuerzaEmpuje;

        //StopAllCoroutines();
        //StartCoroutine(Sacudir());

        if (golpes >= vidaMaxima)
        {
            muerto = true;
            atacando = false;
            volando = false;
            congeladoEnAire = false;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            anim.ResetTrigger("ataque");
            anim.ResetTrigger("volar");
            anim.SetTrigger("muerto");

            StartCoroutine(DesaparecerTrasAnim());
        }
    }

    IEnumerator Sacudir()
    {
        Vector3 pos = transform.localPosition;
        float t = 0f;
        while (t < 0.22f)
        {
            transform.localPosition = pos + new Vector3(Mathf.Sin(t * 80f) * 0.07f, 0f, 0f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = pos;
    }

    IEnumerator DesaparecerTrasAnim()
    {
        sr.color = Color.white;

        // Reproducir sonido de muerte
        if (sonidoMuerte != null)
        {
            audioSource.PlayOneShot(sonidoMuerte);
        }

        // Esperar a que termine la animación de muerte
        float tiempoEspera = 0f;
        while (tiempoEspera < 2f)
        {
            var info = anim.GetCurrentAnimatorStateInfo(0);

            if (info.IsName("muerte_pelicano") && info.normalizedTime >= 0.95f)
                break;

            tiempoEspera += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Dibujar línea de visión
        if (jugadorTrans != null)
        {
            Gizmos.color = TieneLineaDeVision() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, jugadorTrans.position);
        }

        // Dibujar rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}