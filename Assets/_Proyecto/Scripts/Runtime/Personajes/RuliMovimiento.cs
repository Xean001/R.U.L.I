using System.Collections;
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

    [Header("Ataque Tornado")]
    public float duracionTornado = 3f;          // cuanto dura el giro
    public float intervaloGolpeTornado = 0.3f;  // cada cuanto golpea
    public float radioTornado = 1f;             // alcance del tornado (mas grande)

    [Header("Ataque Disparo (arma/soplador)")]
    public GameObject prefabDisparo;            // opcional; por defecto carga Resources/ProyectilRuli
    public float velocidadDisparo = 12f;
    public Vector2 offsetDisparo = new Vector2(0.6f, 0f);
    public Vector2 offsetDisparoArriba = new Vector2(0f, 0.6f);

    [Header("Escalada (lianas)")]
    public float escalarVelocidad = 3f;

    [Header("Audio")]
    [SerializeField] private PlayerSoundcontroler soundControl;

    private Rigidbody2D rb;
    private Animator anim;
    private RuliSalud salud;
    private EquiparArma equipar;
    private bool estaEnSuelo;
    private float movimientoHorizontal;
    private bool miraDerecha = true;
    private bool saltoPendiente;
    private int saltosMaximos = 1;   // 2 si compro el doble salto en la tienda
    private int saltosUsados;
    private float empujeTimer;       // mientras corre, el control no pisa la velocidad del empujon
    private bool estaMuerto;
    private bool tornadoActivo;
    private float gravedadOriginal;
    private int lianasEnContacto;
    private bool escalando;
    private float escalarInput;

    void Awake()
    {
        rb      = GetComponent<Rigidbody2D>();
        anim    = GetComponent<Animator>();
        salud   = GetComponent<RuliSalud>();
        equipar = GetComponent<EquiparArma>();
        gravedadOriginal = rb.gravityScale;
        saltosMaximos = PlayerPrefs.GetInt("DobleSaltoComprado", 0) == 1 ? 2 : 1;
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

        // Input unificado: teclado + mando + controles tactiles
        movimientoHorizontal = RuliInput.MovimientoHorizontal();
        bool saltoInput = RuliInput.SaltoPresionado();

        // --- Escalada de lianas (W sube / S baja, Espacio salta) ---
        bool tocandoLiana = lianasEnContacto > 0;
        float vEscalar = RuliInput.EscalarVertical();

        if (escalando)
        {
            escalarInput = vEscalar;
            if (saltoInput) { DejarLiana(); saltoPendiente = true; }   // saltar a la siguiente
            else if (!tocandoLiana) DejarLiana();
        }
        else if (tocandoLiana && Mathf.Abs(vEscalar) > 0.1f && !RuliInput.RuedaAbierta)
        {
            EntrarLiana();
        }

        if (!escalando && saltoInput && (estaEnSuelo || saltosUsados < saltosMaximos) && !RuliInput.RuedaAbierta && !tornadoActivo)
            saltoPendiente = true;

        if (RuliInput.AtaquePresionado() && !RuliInput.RuedaAbierta)
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

        if (escalando)
        {
            // Sin gravedad: sube/baja por la liana y permite moverse horizontalmente
            rb.linearVelocity = new Vector2(movimientoHorizontal * velocidad, escalarInput * escalarVelocidad);
            return;
        }

        if (empujeTimer > 0f)
            empujeTimer -= Time.fixedDeltaTime;   // conserva la velocidad del empujon
        else
            rb.linearVelocity = new Vector2(movimientoHorizontal * velocidad, rb.linearVelocity.y);

        if (saltoPendiente)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            saltoPendiente = false;
            saltosUsados = estaEnSuelo ? 1 : saltosUsados + 1;
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
        saltosUsados = 0;

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
        if (EsColisionSuelo(col)) { estaEnSuelo = true; saltosUsados = 0; }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        estaEnSuelo = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (estaMuerto) return;

        if (other.GetComponent<Liana>() != null)
            lianasEnContacto++;

        if (other.CompareTag("Mortal"))
        {
            if (salud != null) salud.RecibirDaño();
            else Morir();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Liana>() != null)
        {
            lianasEnContacto = Mathf.Max(0, lianasEnContacto - 1);
            if (lianasEnContacto == 0) DejarLiana();
        }
    }

    // Empujon externo (ej. embestida del carrito): fija la velocidad y bloquea
    // el control horizontal un momento para que el golpe se sienta.
    public void Empujar(Vector2 velocidadEmpuje, float duracion = 0.25f)
    {
        if (estaMuerto) return;
        DejarLiana();
        rb.linearVelocity = velocidadEmpuje;
        empujeTimer = duracion;
    }

    void EntrarLiana()
    {
        escalando = true;
        saltoPendiente = false;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    void DejarLiana()
    {
        if (!escalando) return;
        escalando = false;
        rb.gravityScale = gravedadOriginal;
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
        // Durante el tornado forzamos "en suelo" para que el roce con enemigos
        // (OnCollisionExit2D) no dispare la animacion de salto y corte el giro.
        anim.SetBool("en suelo", estaEnSuelo || tornadoActivo || escalando);
    }

    void Atacar()
    {
        int arma = equipar != null ? equipar.indiceArmaActiva : 0;

        // Tornado (giro sostenido)
        if (arma == 1)
        {
            if (!tornadoActivo) StartCoroutine(AtaqueTornado());
            return;
        }

        // Arma/soplador -> animacion de disparo + proyectil
        if (arma == 2)
        {
            // Con W presionada (y sin estar escalando) dispara hacia arriba
            bool arriba = !escalando && RuliInput.EscalarVertical() > 0.5f;

            if (soundControl != null) soundControl.PlayAttack();
            if (anim != null)
            {
                string trigger = arriba ? "atacarDisparoArriba" : "atacarDisparo";
                anim.ResetTrigger(trigger);
                anim.SetTrigger(trigger);
            }
            DispararProyectil(arriba);
            return;
        }

        // Ataque normal (puños)
        if (soundControl != null) soundControl.PlayAttack();
        if (anim != null)
        {
            anim.ResetTrigger("atacar");
            anim.SetTrigger("atacar");
        }
        AplicarGolpe(radioAtaque);
    }

    void DispararProyectil(bool arriba = false)
    {
        GameObject prefab = prefabDisparo != null ? prefabDisparo : Resources.Load<GameObject>("ProyectilRuli");
        if (prefab == null) return;

        float dir = miraDerecha ? 1f : -1f;
        Vector2 offset = arriba ? offsetDisparoArriba : offsetDisparo;
        Vector3 origen = transform.position + new Vector3(offset.x * dir, offset.y, 0f);

        // Hacia arriba la bala se rota 90 grados (el sprite apunta a la derecha)
        Quaternion rot = arriba ? Quaternion.Euler(0f, 0f, 90f * dir) : Quaternion.identity;
        GameObject bala = Instantiate(prefab, origen, rot);

        // Orientar la bala hacia donde mira Ruli
        Vector3 esc = bala.transform.localScale;
        esc.x = Mathf.Abs(esc.x) * dir;
        bala.transform.localScale = esc;

        Vector2 vel = arriba ? new Vector2(0f, velocidadDisparo) : new Vector2(dir * velocidadDisparo, 0f);
        var p = bala.GetComponent<ProyectilRuli>();
        if (p != null) p.Lanzar(vel);
        else
        {
            var rbBala = bala.GetComponent<Rigidbody2D>();
            if (rbBala != null) rbBala.linearVelocity = vel;
        }
    }

    IEnumerator AtaqueTornado()
    {
        tornadoActivo = true;
        if (soundControl != null) soundControl.PlayAttack();
        if (anim != null)
        {
            anim.SetBool("tornadoActivo", true);
            anim.ResetTrigger("atacarTornado");
            anim.SetTrigger("atacarTornado");
        }

        // Golpea repetidamente durante toda la duracion del giro
        float t = 0f;
        while (t < duracionTornado && !estaMuerto)
        {
            AplicarGolpe(radioTornado);
            yield return new WaitForSeconds(intervaloGolpeTornado);
            t += intervaloGolpeTornado;
        }

        if (anim != null) anim.SetBool("tornadoActivo", false);
        tornadoActivo = false;
    }

    void AplicarGolpe(float radio)
    {
        Vector2 centro = (Vector2)transform.position + new Vector2(offsetAtaque.x * (miraDerecha ? 1f : -1f), offsetAtaque.y);
        var golpes = Physics2D.OverlapCircleAll(centro, radio, capaCilindros);
        foreach (var c in golpes)
        {
            var cilindro = c.GetComponent<Cilindro>();
            if (cilindro != null) cilindro.Golpe();

            var rompible = c.GetComponent<ObjetoRompible>();
            if (rompible != null) rompible.Golpe();

            var cucaracha = c.GetComponent<EnemigoCucaracha>();
            if (cucaracha != null) cucaracha.Golpe();

            var arena = c.GetComponent<EnemigoArena>();
            if (arena != null) arena.Golpe();

            var escudo = c.GetComponent<EscudoEnemigo>();
            if (escudo != null) escudo.Golpe();

            var pelicano = c.GetComponent<EnemigoPelicano>();
            if (pelicano != null) pelicano.Golpe();

            var librero = c.GetComponent<EnemigoLibreroJefe>();
            if (librero != null) librero.Golpe();

            var dron = c.GetComponent<EnemigoDron>();
            if (dron != null) dron.Golpe();
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
        // ACTIVAR GAME OVER
        var gameOverController = FindFirstObjectByType<GameOverController>();
        if (gameOverController != null)
        {
            gameOverController.MostrarGameOver();
        }
        else
        {
            // Si no hay GameOverController, reiniciar como antes
            Invoke(nameof(Reiniciar), retrasoReinicio);
        }
    }

    void Reiniciar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
