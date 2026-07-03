using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemigoCarrito : MonoBehaviour
{
    public float rangoDeteccion = 6f;

    public float velocidadEmbestida = 7f;
    public bool  miraDerechaPorDefecto = true;

    public bool dañaAlEmbestir = true;

    [Header("Empuje")]
    public float fuerzaEmpuje = 6f;         // empujon horizontal hacia donde va el carrito
    public float fuerzaEmpujeVertical = 3f; // levanta un poco a Ruli para que el empuje se sienta

    public float alturaMinExplotar = 0.35f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D miCollider;
    private Transform objetivo;
    private float direccion;
    private bool embistiendo;
    private bool destruido;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        miCollider = GetComponent<Collider2D>();

        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (destruido || embistiendo) return;
        BuscarYDetectar();
    }

    void FixedUpdate()
    {
        if (destruido || !embistiendo) return;
        rb.linearVelocity = new Vector2(direccion * velocidadEmbestida, rb.linearVelocity.y);
    }

    void BuscarYDetectar()
    {
        if (objetivo == null)
        {
            var ruli = FindFirstObjectByType<RuliMovimiento>();
            if (ruli != null) objetivo = ruli.transform;
        }
        if (objetivo == null) return;

        float dx = objetivo.position.x - transform.position.x;
        if (Mathf.Abs(dx) <= rangoDeteccion)
            IniciarEmbestida(dx >= 0f ? 1f : -1f);
    }

    void IniciarEmbestida(float dir)
    {
        embistiendo = true;
        direccion = dir; // queda fija
        sr.flipX = miraDerechaPorDefecto ? direccion < 0f : direccion > 0f;
        anim.SetBool("embistiendo", true);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (destruido) return;

        var salud = col.gameObject.GetComponent<RuliSalud>();
        if (salud != null)
        {
            if (embistiendo)
            {
                if (dañaAlEmbestir) salud.RecibirDaño();

                // Empujon en la direccion en que iba el carrito
                var mov = col.gameObject.GetComponent<RuliMovimiento>();
                if (mov != null)
                    mov.Empujar(new Vector2(direccion * fuerzaEmpuje, fuerzaEmpujeVertical));

                Explotar();                 // al topar al personaje tambien se destruye
            }
            return;
        }

        if (!embistiendo) return;

        bool frontal = false;
        foreach (ContactPoint2D c in col.contacts)
        {
            if (Mathf.Abs(c.normal.x) > 0.6f) { frontal = true; break; }
        }
        if (!frontal) return; 

        float baseCarrito = miCollider.bounds.min.y;
        float topObstaculo = col.collider.bounds.max.y;
        float alturaBloqueo = topObstaculo - baseCarrito;

        if (alturaBloqueo >= alturaMinExplotar)
        {
            Explotar();
        }
        else
        {
            Physics2D.IgnoreCollision(miCollider, col.collider, true);
        }
    }

    void Explotar()
    {
        destruido = true;
        embistiendo = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        anim.SetBool("embistiendo", false);
        anim.SetTrigger("destruir");
        StartCoroutine(DesaparecerTrasAnim());
    }

    IEnumerator DesaparecerTrasAnim()
    {
        yield return null;
        yield return null;

        while (true)
        {
            var info = anim.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("destruccion") && info.normalizedTime >= 0.95f) break;
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Golpe()
    {
        if (!destruido) Explotar();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}
