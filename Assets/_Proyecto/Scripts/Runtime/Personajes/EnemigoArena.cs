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
    [Tooltip("Daño por golpe.")]
    public int daño = 1;

    [Header("Vida")]
    [Tooltip("Golpes de Ruli para morir.")]
    public int golpesParaMorir = 10;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private Transform ruli;
    private RuliSalud ruliSalud;

    private Vector3 posInicial;
    private float direccion = 1f;
    private int golpes;
    private bool muerto;
    private float timerAtaque;

    void Awake()
    {
        rb         = GetComponent<Rigidbody2D>();
        sr         = GetComponent<SpriteRenderer>();
        anim       = GetComponent<Animator>();
        posInicial = transform.position;

        rb.gravityScale = 3f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        ruliSalud = Object.FindFirstObjectByType<RuliSalud>();
        if (ruliSalud != null) ruli = ruliSalud.transform;
    }

    void Update()
    {
        if (muerto) return;

        if (timerAtaque > 0f) timerAtaque -= Time.deltaTime;

        if (VeARuli())
            ModoAtaque();
        else
            ModoPatrulla();
    }

    void FixedUpdate()
    {
        if (muerto) return;

        // Si está atacando, no se mueve; si patrulla, avanza.
        if (anim != null && anim.GetBool("atacando"))
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
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

        if (anim != null) anim.SetBool("atacando", true);

        // Bajar vida si está al alcance y pasó el cooldown
        if (Mathf.Abs(dx) <= rangoAtaque && timerAtaque <= 0f && ruliSalud != null)
        {
            ruliSalud.RecibirDaño(daño);
            timerAtaque = intervaloAtaque;
        }
    }

    void ModoPatrulla()
    {
        if (anim != null) anim.SetBool("atacando", false);

        float x = transform.position.x;
        if (x >= posInicial.x + rangoPatrulla && direccion > 0f) Girar(-1f);
        if (x <= posInicial.x - rangoPatrulla && direccion < 0f) Girar( 1f);

        sr.flipX = direccion < 0f;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (muerto) return;

        // Choque con pared u objeto horizontal → cambiar dirección (solo patrullando)
        if (anim != null && anim.GetBool("atacando")) return;

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
        if (anim != null) anim.SetBool("atacando", false);
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
