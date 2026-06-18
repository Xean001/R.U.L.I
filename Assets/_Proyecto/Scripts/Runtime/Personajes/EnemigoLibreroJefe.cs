using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemigoLibreroJefe : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 0.6f;
    public float rangoPatrulla = 3f;

    [Header("Detección")]
    public float rangoDeteccion = 6f;

    [Header("Ataque")]
    public float duracionAtaque = 0.9f;
    public float rangoGolpe = 2.2f;

    [Header("Vida")]
    public int vidaMax = 20;

    [Header("Audio")]
    public AudioClip sonidoCaminar;
    public AudioClip sonidoGolpe;
    public AudioClip sonidoAtaque;
    private AudioSource audioSource;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Transform ruliTrans;
    private RuliSalud ruliSalud;
    private EnemigoVidaUI vidaUI;
    private Camera camara;
    private Vector3 posInicial;
    private float direccion = -1f;
    private int vida;
    private int ultimoFrameGolpe = -1;
    private bool atacando;
    private bool caminando;
    private bool muerto;
    private float tiempoUltimoSonidoCaminar;
    private float distanciaAlJugador;
    private bool enPantalla;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        posInicial = transform.position;
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb.mass = 10f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        vida = vidaMax;
        vidaUI = FindObjectOfType<EnemigoVidaUI>();
        if (vidaUI == null)
        {
            Debug.LogWarning("⚠️ LibreroJefe: No se encontró EnemigoVidaUI en la escena");
        }

        camara = Camera.main;

        var ruliMov = FindFirstObjectByType<RuliMovimiento>();
        if (ruliMov != null)
        {
            ruliTrans = ruliMov.transform;
            ruliSalud = ruliMov.GetComponent<RuliSalud>();
        }
    }

    void Update()
    {
        if (muerto) return;

        ActualizarBarra();

        if (atacando) return;

        MirarARuli();

        if (ruliTrans != null)
        {
            distanciaAlJugador = Vector2.Distance(transform.position, ruliTrans.position);

            if (distanciaAlJugador <= rangoGolpe)
            {
                StartCoroutine(SecuenciaAtaque());
            }
            else if (distanciaAlJugador <= rangoDeteccion)
            {
                Perseguir();
            }
            else
            {
                Patrullar();
            }
        }
        else
        {
            Patrullar();
        }
    }

    void Perseguir()
    {
        float nuevaDir = ruliTrans.position.x > transform.position.x ? 1f : -1f;
        if (direccion != nuevaDir)
        {
            direccion = nuevaDir;
            sr.flipX = direccion < 0f;
        }

        if (!caminando)
        {
            caminando = true;
            anim.Play("caminar_librerojefe", 0, 0f);
        }

        if (enPantalla && sonidoCaminar != null && Time.time - tiempoUltimoSonidoCaminar > 0.5f)
        {
            audioSource.PlayOneShot(sonidoCaminar, 0.5f);
            tiempoUltimoSonidoCaminar = Time.time;
        }
    }

    void Patrullar()
    {
        float x = transform.position.x;
        if (x >= posInicial.x + rangoPatrulla && direccion > 0f) Girar(-1f);
        if (x <= posInicial.x - rangoPatrulla && direccion < 0f) Girar(1f);

        if (!caminando)
        {
            caminando = true;
            anim.Play("caminar_librerojefe", 0, 0f);
        }

        if (enPantalla && sonidoCaminar != null && Time.time - tiempoUltimoSonidoCaminar > 0.5f)
        {
            audioSource.PlayOneShot(sonidoCaminar, 0.5f);
            tiempoUltimoSonidoCaminar = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (muerto || atacando) return;
        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
    }

    void Girar(float dir)
    {
        direccion = dir;
        sr.flipX = direccion < 0f;
    }

    void MirarARuli()
    {
        if (ruliTrans != null)
            sr.flipX = ruliTrans.position.x < transform.position.x;
        else
            sr.flipX = direccion < 0f;
    }

    void ActualizarBarra()
    {
        if (vidaUI == null) return;

        bool estabaEnPantalla = enPantalla;
        enPantalla = EnPantalla();

        if (enPantalla)
        {
            vidaUI.Mostrar();
            vidaUI.SetVida((float)vida / vidaMax);
        }
        else
        {
            vidaUI.Ocultar();
        }

        if (estabaEnPantalla && !enPantalla)
        {
            caminando = false;
        }
    }

    bool EnPantalla()
    {
        if (camara == null) camara = Camera.main;
        if (camara == null) return false;
        Vector3 vp = camara.WorldToViewportPoint(transform.position);
        return vp.z > 0f
            && vp.x > -0.05f && vp.x < 1.05f
            && vp.y > -0.30f && vp.y < 1.30f;
    }

    IEnumerator SecuenciaAtaque()
    {
        if (atacando) yield break;

        atacando = true;
        caminando = false;
        rb.linearVelocity = Vector2.zero;

        MirarARuli();
        anim.Play("atacar_librerojefe", 0, 0f);

        if (sonidoAtaque != null)
        {
            audioSource.PlayOneShot(sonidoAtaque);
        }

        yield return new WaitForSeconds(duracionAtaque * 0.6f);
        if (ruliSalud != null && ruliTrans != null)
        {
            float dist = Vector2.Distance(transform.position, ruliTrans.position);
            if (dist < rangoGolpe) ruliSalud.RecibirDaño();
        }
        yield return new WaitForSeconds(duracionAtaque * 0.4f);
        atacando = false;
    }

    public void Golpe()
    {
        if (muerto) return;
        if (Time.frameCount == ultimoFrameGolpe) return;
        ultimoFrameGolpe = Time.frameCount;

        vida--;

        if (vidaUI != null)
        {
            vidaUI.Mostrar();
            vidaUI.SetVida((float)vida / vidaMax);
        }

        if (sonidoGolpe != null)
        {
            audioSource.PlayOneShot(sonidoGolpe);
        }

        StopCoroutine("Sacudir");
        StartCoroutine(Sacudir());

        if (vida <= 0) Morir();
    }

    void Morir()
    {
        muerto = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (vidaUI != null) vidaUI.Ocultar();

        // ←←← USAR VictoryController (igual que EscudoEnemigo)
        var victoryController = FindFirstObjectByType<VictoryController>();
        if (victoryController != null)
        {
            victoryController.nivelActual = 2; // ← Cambia esto según el nivel
            victoryController.MostrarVictoria();
            Debug.Log("✅ VictoryController encontrado - Mostrando victoria");
        }
        else
        {
            Debug.LogError("❌ No se encontró VictoryController en la escena");
        }

        StopAllCoroutines();
        StartCoroutine(Desaparecer());
    }

    IEnumerator Desaparecer()
    {
        float t = 0f;
        Color c = sr.color;
        while (t < 0.6f)
        {
            t += Time.deltaTime;
            sr.color = new Color(c.r, c.g, c.b, 1f - t / 0.6f);
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator Sacudir()
    {
        Vector3 pos = transform.localPosition;
        float t = 0f;
        while (t < 0.2f)
        {
            transform.localPosition = pos + new Vector3(Mathf.Sin(t * 80f) * 0.06f, 0f, 0f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = pos;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (muerto) return;

        var salud = col.gameObject.GetComponent<RuliSalud>();
        if (salud != null)
        {
            salud.RecibirDaño();
            return;
        }

        foreach (ContactPoint2D c in col.contacts)
        {
            if (Mathf.Abs(c.normal.x) > 0.6f)
            {
                Girar(c.normal.x > 0f ? 1f : -1f);
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoGolpe);
    }
}