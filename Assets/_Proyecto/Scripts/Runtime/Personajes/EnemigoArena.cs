using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemigoArena : MonoBehaviour
{
    [Header("Patrulla")]
    public float velocidad = 1.8f;
    public float rangoPatrulla = 3f;

    [Header("Detección / Ataque")]
    [Tooltip("Distancia a la que ve a Ruli y empieza a atacar.")]
    public float rangoVision = 4f;
    [Tooltip("Distancia a la que el golpe le baja vida a Ruli.")]
    public float rangoAtaque = 1.8f;
    [Tooltip("Diferencia de altura máxima para considerar que ve a Ruli.")]
    public float toleranciaAltura = 2.5f;
    [Tooltip("Segundos entre golpe y golpe mientras ataca.")]
    public float intervaloAtaque = 0.8f;
    [Tooltip("Tiempo que dura la animacion/estado de ataque antes de entrar en cooldown.")]
    public float duracionAtaque = 0.35f;
    [Tooltip("Daño por golpe.")]
    public int daño = 1;

    [Header("Vida")]
    [Tooltip("Golpes de Ruli para morir.")]
    public int golpesParaMorir = 10;

    [Header("Persecucion")]
    [Tooltip("Si esta activo, camina hacia Ruli cuando lo ve y solo ataca al estar cerca (rangoAtaque).")]
    public bool perseguir = false;

    [Tooltip("Marca si el sprite (sin voltear) mira a la DERECHA. Desactivalo si el arte mira a la izquierda.")]
    public bool spriteMiraDerecha = true;

    [Header("Aparicion")]
    [Tooltip("Si esta activo, el enemigo empieza oculto y aparece cuando Ruli se acerca.")]
    public bool aparecerAlAcercarse = false;
    [Tooltip("Distancia desde la que Ruli hace aparecer al enemigo.")]
    public float rangoAparicion = 7f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private Collider2D[] colliders;
    private Transform ruli;
    private RuliSalud ruliSalud;

    private Vector3 posInicial;
    private float direccion = 1f;
    private int golpes;
    private bool muerto;
    private bool atacando;
    private bool visible = true;
    private float timerCooldownAtaque;
    private float timerDuracionAtaque;

    void Awake()
    {
        rb         = GetComponent<Rigidbody2D>();
        sr         = GetComponent<SpriteRenderer>();
        anim       = GetComponent<Animator>();
        colliders  = GetComponents<Collider2D>();
        posInicial = transform.position;

        rb.gravityScale = 3f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

        if (aparecerAlAcercarse)
            OcultarHastaQueRuliSeAcerque();
    }

    void Start()
    {
        ruliSalud = Object.FindFirstObjectByType<RuliSalud>();
        if (ruliSalud != null) ruli = ruliSalud.transform;
    }

    void Update()
    {
        if (muerto) return;

        if (!visible)
        {
            IntentarAparecer();
            return;
        }

        if (timerCooldownAtaque > 0f) timerCooldownAtaque -= Time.deltaTime;

        if (timerDuracionAtaque > 0f)
        {
            timerDuracionAtaque -= Time.deltaTime;
            if (timerDuracionAtaque <= 0f)
                TerminarAtaque();
        }

        if (VeARuli())
        {
            float distX = Mathf.Abs(ruli.position.x - transform.position.x);
            if (perseguir && distX > rangoAtaque)
                ModoPerseguir();   // lo ve pero esta lejos -> camina hacia el
            else
                ModoAtaque();      // cerca (o sin perseguir) -> ataca
        }
        else
            ModoPatrulla();
    }

    void FixedUpdate()
    {
        if (muerto || !visible) return;

        // Si está atacando, no se mueve; si patrulla, avanza.
        if (atacando)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
    }

    void LateUpdate()
    {
        if (!visible) return;

        // Encara segun la orientacion del arte (corrige el volteo si el sprite mira a la izquierda).
        if (sr != null)
            sr.flipX = spriteMiraDerecha ? direccion < 0f : direccion > 0f;
    }

    bool VeARuli()
    {
        if (ruli == null) return false;
        float dx = ruli.position.x - transform.position.x;
        float dy = Mathf.Abs(ruli.position.y - transform.position.y);
        return Mathf.Abs(dx) <= rangoVision && dy <= toleranciaAltura;
    }

    void ModoAtaque()
    {
        // Encarar a Ruli
        float dx = ruli.position.x - transform.position.x;
        direccion = dx >= 0f ? 1f : -1f;
        sr.flipX  = direccion < 0f;

        if (atacando || timerCooldownAtaque > 0f) return;

        // Ataca una sola vez, luego espera el cooldown antes de repetir.
        if (Mathf.Abs(dx) <= rangoAtaque)
            IniciarAtaque();
    }

    void ModoPerseguir()
    {
        // Camina hacia Ruli (sin atacar) hasta quedar al alcance.
        TerminarAtaque();
        float dx = ruli.position.x - transform.position.x;
        direccion = dx >= 0f ? 1f : -1f;
        sr.flipX  = direccion < 0f;
    }

    void ModoPatrulla()
    {
        TerminarAtaque();

        float x = transform.position.x;
        if (x >= posInicial.x + rangoPatrulla && direccion > 0f) Girar(-1f);
        if (x <= posInicial.x - rangoPatrulla && direccion < 0f) Girar( 1f);

        sr.flipX = direccion < 0f;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (muerto) return;

        // Choque con pared u objeto horizontal → cambiar dirección (solo patrullando)
        if (atacando) return;

        foreach (ContactPoint2D c in col.contacts)
        {
            if (Mathf.Abs(c.normal.x) > 0.6f)
            {
                Girar(c.normal.x > 0f ? 1f : -1f);
                break;
            }
        }
    }

    void Girar(float nuevaDireccion)
    {
        direccion = nuevaDireccion;
        sr.flipX  = direccion < 0f;
    }

    void OcultarHastaQueRuliSeAcerque()
    {
        visible = false;
        if (sr != null) sr.enabled = false;
        if (anim != null) anim.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        foreach (var col in colliders)
            col.enabled = false;
    }

    void IntentarAparecer()
    {
        if (ruli == null) return;

        float distancia = Vector2.Distance(transform.position, ruli.position);
        if (distancia > rangoAparicion) return;

        visible = true;
        posInicial = transform.position;

        if (sr != null) sr.enabled = true;
        if (anim != null) anim.enabled = true;
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        foreach (var col in colliders)
            col.enabled = true;
    }

    void IniciarAtaque()
    {
        atacando = true;
        timerDuracionAtaque = duracionAtaque;
        timerCooldownAtaque = intervaloAtaque;

        if (anim != null) anim.SetBool("atacando", true);
        if (ruliSalud != null) ruliSalud.RecibirDaño(daño);
    }

    void TerminarAtaque()
    {
        atacando = false;
        timerDuracionAtaque = 0f;
        if (anim != null) anim.SetBool("atacando", false);
    }

    public void Golpe()
    {
        if (muerto) return;
        golpes++;

        StartCoroutine(Parpadeo());

        if (golpes >= golpesParaMorir)
            Morir();
    }

    void Morir()
    {
        muerto = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType       = RigidbodyType2D.Kinematic;
        TerminarAtaque();
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        StopAllCoroutines();
        StartCoroutine(Desaparecer());
    }

    IEnumerator Parpadeo()
    {
        sr.color = new Color(1f, 0.4f, 0.3f);
        yield return new WaitForSeconds(0.12f);
        if (!muerto) sr.color = Color.white;
    }

    IEnumerator Desaparecer()
    {
        float t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / 0.5f);
            sr.color = new Color(1f, 1f, 1f, a);
            transform.position += new Vector3(0f, Time.deltaTime * 0.6f, 0f);
            yield return null;
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoVision);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}
