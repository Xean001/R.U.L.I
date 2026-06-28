using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemigoDron : MonoBehaviour
{
    [Header("Deteccion")]
    public float rangoDeteccion = 7f;

    [Header("Patrulla (solo cuando NO detecta a Ruli)")]
    public float velocidadPatrulla = 2f;   // velocidad de movimiento izquierda<->derecha
    public float rangoPatrulla = 3f;        // distancia a cada lado de la posicion inicial

    [Header("Disparo")]
    public GameObject prefabDisparo;       // proyectil a instanciar
    public float intervaloDisparo = 5f;    // segundos entre disparos (en rango)
    public float velocidadDisparo = 8f;
    public float duracionCarga = 0.5f;     // tiempo de la animacion de carga antes de soltar
    public Transform puntoDisparo;         // boca de disparo (opcional; por defecto el propio dron)
    public bool miraDerechaPorDefecto = true;

    [Header("Vida")]
    public int golpesParaMorir = 3;        // golpes de Ruli (melee/tornado/disparo) para caer

    private int  golpes;
    private bool muerto;

    private Animator       anim;
    private SpriteRenderer sr;
    private Transform      objetivo;       // Ruli
    private float          tiempoProximoDisparo;
    private bool           cargando;
    private Vector3        posInicial;      // centro de la patrulla
    private float          direccion = 1f;  // -1 izq, +1 der

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
        if (puntoDisparo == null) puntoDisparo = transform;
        posInicial = transform.position;
    }

    void Update()
    {
        if (muerto) return;
        if (cargando) return;   // mientras carga/dispara queda quieto

        if (objetivo == null)
        {
            var ruli = FindFirstObjectByType<RuliMovimiento>();
            if (ruli != null) objetivo = ruli.transform;
        }

        bool detectado = objetivo != null &&
                         Vector2.Distance(transform.position, objetivo.position) <= rangoDeteccion;

        if (detectado)
        {
            // DETECTA a Ruli -> se detiene, lo mira y dispara (no se mueve)
            float dx = objetivo.position.x - transform.position.x;
            sr.flipX = miraDerechaPorDefecto ? dx < 0f : dx > 0f;

            if (Time.time >= tiempoProximoDisparo)
                StartCoroutine(CargarYDisparar());
        }
        else
        {
            // NO detecta -> patrulla izquierda<->derecha
            Patrullar();
        }
    }

    void Patrullar()
    {
        float x = transform.position.x;
        if (x >= posInicial.x + rangoPatrulla && direccion > 0f) direccion = -1f;
        if (x <= posInicial.x - rangoPatrulla && direccion < 0f) direccion =  1f;

        transform.position += Vector3.right * direccion * velocidadPatrulla * Time.deltaTime;
        sr.flipX = miraDerechaPorDefecto ? direccion < 0f : direccion > 0f;
    }

    IEnumerator CargarYDisparar()
    {
        cargando = true;
        anim.SetBool("cargando", true);

        yield return new WaitForSeconds(duracionCarga);

        Disparar();

        anim.SetBool("cargando", false);
        cargando = false;
        tiempoProximoDisparo = Time.time + intervaloDisparo;
    }

    void Disparar()
    {
        if (prefabDisparo == null || objetivo == null) return;

        Vector2 origen = puntoDisparo.position;
        Vector2 dir    = ((Vector2)objetivo.position - origen).normalized;

        var go  = Instantiate(prefabDisparo, origen, Quaternion.identity);
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0f, 0f, ang);

        var disparo = go.GetComponent<DisparoDron>();
        if (disparo != null) disparo.Lanzar(dir * velocidadDisparo);
        else
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * velocidadDisparo;
        }
    }

    // Recibe un golpe de Ruli (melee, tornado o proyectil). Muere a los N golpes.
    public void Golpe()
    {
        if (muerto) return;

        golpes++;
        StopCoroutine(nameof(Sacudir));
        StartCoroutine(Sacudir());

        if (golpes >= golpesParaMorir)
            Morir();
    }

    private void Morir()
    {
        muerto   = true;
        cargando = false;
        if (anim != null) anim.SetBool("cargando", false);
        StopAllCoroutines();
        StartCoroutine(Desaparecer());
    }

    private IEnumerator Sacudir()
    {
        Vector3 pos = transform.localPosition;
        float t = 0f;
        while (t < 0.18f)
        {
            transform.localPosition = pos + new Vector3(Mathf.Sin(t * 80f) * 0.06f, 0f, 0f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = pos;
    }

    // Sin animacion de muerte: cae un poco y se desvanece.
    private IEnumerator Desaparecer()
    {
        float t = 0f;
        Color c = sr.color;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float p = t / 0.5f;
            sr.color = new Color(c.r, c.g, c.b, 1f - p);
            transform.position += new Vector3(0f, -2f * Time.deltaTime, 0f);
            yield return null;
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
    }
}
