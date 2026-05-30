using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RuliMovimiento : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 10f;

    [Header("Muerte")]
    public float retrasoReinicio = 1.5f;

    [Header("Ataque")]
    public float radioAtaque = 0.6f;
    public Vector2 offsetAtaque = new Vector2(0.4f, 0f);
    public LayerMask capaCilindros = ~0;

    [Header("Audio")]
    [SerializeField] private PlayerSoundcontroler soundControl;

    private Rigidbody2D rb;
    private Animator anim;
    private RuliSalud salud;
    private bool estaEnSuelo;
    private float movimientoHorizontal;
    private bool miraDerecha = true;
    private bool saltoPendiente;
    private bool estaMuerto;

    void Awake()
    {
        rb     = GetComponent<Rigidbody2D>();
        anim   = GetComponent<Animator>();
        salud  = GetComponent<RuliSalud>();
        if (soundControl == null)
            soundControl = GetComponent<PlayerSoundcontroler>();
    }

    void Start()
    {
        if (soundControl != null) soundControl.PlaySpawn();
    }

    void Update()
    {
        if (estaMuerto) return;

        var teclado = Keyboard.current;
        if (teclado == null) return;

        movimientoHorizontal = 0f;
        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) movimientoHorizontal = 1f;
        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) movimientoHorizontal = -1f;

        if ((teclado.wKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame) && estaEnSuelo)
            saltoPendiente = true;

        if (teclado.fKey.wasPressedThisFrame)
            Atacar();

        VoltearPersonaje();
        ActualizarAnimaciones();

        if (soundControl != null)
        {
            if (!saltoPendiente && estaEnSuelo && Mathf.Abs(movimientoHorizontal) > 0.01f)
                soundControl.PlayRun();
            else
                soundControl.StopRun();
        }
    }

    void FixedUpdate()
    {
        if (estaMuerto) return;

        rb.linearVelocity = new Vector2(movimientoHorizontal * velocidad, rb.linearVelocity.y);

        if (saltoPendiente)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            saltoPendiente = false;
            estaEnSuelo = false;
            if (anim != null) anim.SetTrigger("saltar");
            if (soundControl != null)
            {
                soundControl.StopRun();
                soundControl.PlayJump();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!EsColisionSuelo(col)) return;
        estaEnSuelo = true;

        var llanta = col.gameObject.GetComponent<Llanta>();
        if (llanta != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, llanta.fuerzaRebote);
            estaEnSuelo = false;
            if (anim != null) anim.SetTrigger("saltar");
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (EsColisionSuelo(col)) estaEnSuelo = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        estaEnSuelo = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (estaMuerto) return;
        if (!other.CompareTag("Mortal")) return;
        if (salud != null) salud.RecibirDaño();
        else Morir();
    }

    bool EsColisionSuelo(Collision2D col)
    {
        foreach (ContactPoint2D contacto in col.contacts)
            if (contacto.normal.y > 0.5f) return true;
        return false;
    }

    void VoltearPersonaje()
    {
        if (movimientoHorizontal > 0 && !miraDerecha) Voltear();
        else if (movimientoHorizontal < 0 && miraDerecha) Voltear();
    }

    void Voltear()
    {
        miraDerecha = !miraDerecha;
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    void ActualizarAnimaciones()
    {
        if (anim == null) return;
        anim.SetFloat("velocidadX", Mathf.Abs(movimientoHorizontal));
        anim.SetBool("en suelo", estaEnSuelo);
    }

    void Atacar()
    {
        if (soundControl != null) soundControl.PlayAttack();
        if (anim != null)
        {
            anim.ResetTrigger("atacar");
            anim.SetTrigger("atacar");
        }

        Vector2 centro = (Vector2)transform.position + new Vector2(offsetAtaque.x * (miraDerecha ? 1f : -1f), offsetAtaque.y);
        var golpes = Physics2D.OverlapCircleAll(centro, radioAtaque, capaCilindros);
        foreach (var c in golpes)
        {
            var cilindro = c.GetComponent<Cilindro>();
            if (cilindro != null) cilindro.Golpe();

            var rompible = c.GetComponent<ObjetoRompible>();
            if (rompible != null) rompible.Golpe();

            var cucaracha = c.GetComponent<EnemigoCucaracha>();
            if (cucaracha != null) cucaracha.Golpe();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 centro = (Vector2)transform.position + new Vector2(offsetAtaque.x * (miraDerecha ? 1f : -1f), offsetAtaque.y);
        Gizmos.DrawWireSphere(centro, radioAtaque);
    }

    public void Morir()
    {
        estaMuerto = true;
        movimientoHorizontal = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (soundControl != null) soundControl.PlayDead();
        if (soundControl != null) soundControl.StopRun();
        if (anim != null)
        {
            anim.SetFloat("velocidadX", 0f);
            anim.SetBool("en suelo", true);
            anim.ResetTrigger("muerto");
            anim.SetTrigger("muerto");
        }
        Invoke(nameof(Reiniciar), retrasoReinicio);
    }

    void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
