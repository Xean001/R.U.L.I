using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemigoCucaracha : MonoBehaviour
{
    [Header("Patrulla")]
    public float velocidad     = 1.5f;
    public float rangoPatrulla = 2.5f;

    private static readonly Color[] tintes =
    {
        Color.white,
        new Color(1f, 0.65f, 0.3f),
        new Color(1f, 0.25f, 0.1f),
    };

    private Rigidbody2D    rb;
    private Animator       anim;
    private SpriteRenderer sr;
    private Vector3 posInicial;
    private float   direccion = 1f;
    private int     golpes;
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


    void Update()
    {
        if (muerto) return;

        // Invertir por rango de patrulla
        float x = transform.position.x;
        if (x >= posInicial.x + rangoPatrulla && direccion > 0f) Girar(-1f);
        if (x <= posInicial.x - rangoPatrulla && direccion < 0f) Girar( 1f);

        anim.SetFloat("velocidadX", Mathf.Abs(rb.linearVelocity.x));
        sr.flipX = direccion < 0f;
    }

    void FixedUpdate()
    {
        if (muerto) return;
        rb.linearVelocity = new Vector2(direccion * velocidad, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (muerto) return;

        // Si choca con Ruli → bajar vida
        var salud = col.gameObject.GetComponent<RuliSalud>();
        if (salud != null)
        {
            salud.RecibirDaño();
            return;
        }

        // Si choca con pared u objeto horizontal → cambiar dirección
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
        direccion  = nuevaDireccion;
        sr.flipX   = direccion < 0f;
    }

    public void Golpe()
    {
        if (muerto) return;
        golpes++;
        sr.color = tintes[Mathf.Clamp(golpes, 0, tintes.Length - 1)];
        StopAllCoroutines();
        StartCoroutine(Sacudir());

        if (golpes >= 3)
        {
            muerto = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType       = RigidbodyType2D.Kinematic;
            anim.SetFloat("velocidadX", 0f);
            anim.SetTrigger("muerto");
            StartCoroutine(DesaparecesTrasAnim());
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

    IEnumerator DesaparecesTrasAnim()
    {
        sr.color = Color.white;

        // Esperar a que el estado dead empiece
        yield return null;
        yield return null;

        // Esperar a que la animación dead termine
        while (true)
        {
            var info = anim.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("dead") && info.normalizedTime >= 0.95f)
                break;
            yield return null;
        }

        Destroy(gameObject);
    }
}
