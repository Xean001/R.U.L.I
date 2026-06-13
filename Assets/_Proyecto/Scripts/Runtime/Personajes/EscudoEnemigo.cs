using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EscudoEnemigo : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad       = 0.6f;
    public float rangoPatrulla   = 3f;

    [Header("Deteccion")]
    public float rangoDeteccion  = 6f;

    [Header("Ataque")]
    public float duracionAtaque  = 0.9f;
    public float rangoGolpe      = 2.2f;

    [Header("Vida")]
    public int vidaMax = 20;          // golpes de Ruli para morir

    private Rigidbody2D    rb;
    private Animator       anim;
    private SpriteRenderer sr;
    private Transform      ruliTrans;
    private RuliSalud      ruliSalud;
    private EnemigoVidaUI  vidaUI;
    private Camera         camara;
    private Vector3 posInicial;
    private float   direccion = -1f;
    private int     vida;
    private int     ultimoFrameGolpe = -1;   // evita doble conteo (2 colliders)
    private bool    atacando;
    private bool    muerto;

    void Awake()
    {
        rb         = GetComponent<Rigidbody2D>();
        anim       = GetComponent<Animator>();
        sr         = GetComponent<SpriteRenderer>();
        posInicial = transform.position;
        rb.gravityScale = 3f;
        rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        vida   = vidaMax;
        vidaUI = FindFirstObjectByType<EnemigoVidaUI>();
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

        // Barra de vida del enemigo: aparece arriba al centro cuando ve a Ruli
        ActualizarBarra();

        if (atacando) return;

        MirarARuli();

        float distX = ruliTrans != null
            ? Mathf.Abs(ruliTrans.position.x - transform.position.x)
            : float.MaxValue;

        if (distX <= rangoGolpe && ruliTrans != null)
        {
            // Ruli al alcance → atacar
            StartCoroutine(SecuenciaAtaque());
        }
        else if (distX <= rangoDeteccion && ruliTrans != null)
        {
            // Ve a Ruli → caminar hacia él
            Perseguir();
        }
        else
        {
            // No ve a Ruli → patrullar caminando
            Patrullar();
        }
    }

    void Perseguir()
    {
        Girar(ruliTrans.position.x > transform.position.x ? 1f : -1f);
        anim.SetFloat("velocidadX", velocidad);
    }

    void Patrullar()
    {
        float x = transform.position.x;
        if (x >= posInicial.x + rangoPatrulla && direccion > 0f) Girar(-1f);
        if (x <= posInicial.x - rangoPatrulla && direccion < 0f) Girar( 1f);
        anim.SetFloat("velocidadX", velocidad);
    }

    void FixedUpdate()
    {
        if (muerto || atacando) return;
        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
    }

    // Solo cambia la direccion de MOVIMIENTO (no el flip)
    void Girar(float dir)
    {
        direccion = dir;
    }

    // El sprite SIEMPRE mira a Ruli (el sprite mira a la derecha por defecto)
    void MirarARuli()
    {
        if (ruliTrans != null)
            sr.flipX = ruliTrans.position.x < transform.position.x;
        else
            sr.flipX = direccion < 0f;
    }

    // La barra aparece cuando el enemigo está EN PANTALLA (cámara del juego)
    void ActualizarBarra()
    {
        if (vidaUI == null) return;

        if (EnPantalla())
        {
            vidaUI.Mostrar();
            vidaUI.SetVida((float)vida / vidaMax);
        }
        else
        {
            vidaUI.Ocultar();
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
        atacando = true;
        rb.linearVelocity = Vector2.zero;

        // Mirar hacia Ruli (mover la cara hacia él antes de golpear)
        MirarARuli();

        anim.SetFloat("velocidadX", 0f);
        anim.SetTrigger("atacar");

        // Punto de daño: 60% de la animacion
        yield return new WaitForSeconds(duracionAtaque * 0.6f);

        if (ruliSalud != null && ruliTrans != null)
        {
            float dist = Vector2.Distance(transform.position, ruliTrans.position);
            if (dist < rangoGolpe) ruliSalud.RecibirDaño();
        }

        yield return new WaitForSeconds(duracionAtaque * 0.4f);
        atacando = false;
    }

    // Recibir golpe de Ruli: baja 1 de vida, actualiza la barra y muere a los 0
    public void Golpe()
    {
        if (muerto) return;
        // El enemigo tiene 2 colliders; ignora golpes repetidos del mismo frame
        if (Time.frameCount == ultimoFrameGolpe) return;
        ultimoFrameGolpe = Time.frameCount;

        vida--;
        if (vidaUI != null)
        {
            vidaUI.Mostrar();
            vidaUI.SetVida((float)vida / vidaMax);
        }

        StopCoroutine("Sacudir");
        StartCoroutine(Sacudir());

        if (vida <= 0) Morir();
    }

    void Morir()
    {
        muerto = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType       = RigidbodyType2D.Kinematic;
        anim.SetFloat("velocidadX", 0f);
        if (vidaUI != null) vidaUI.Ocultar();

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
        if (salud != null) { salud.RecibirDaño(); return; }

        foreach (ContactPoint2D c in col.contacts)
            if (Mathf.Abs(c.normal.x) > 0.6f)
            { Girar(c.normal.x > 0f ? 1f : -1f); break; }
    }
}
