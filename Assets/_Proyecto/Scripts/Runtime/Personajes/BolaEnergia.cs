using UnityEngine;

// Proyectil del jefe final del Nivel 5.
// Fase 1 (carga): pegada al centro del jefe, aparece de opacidad 0 a 1.
// Fase 2 (disparo): cambia al sprite de disparo y viaja en linea recta
// hacia la posicion donde estaba Ruli al terminar la carga (no lo persigue).
public class BolaEnergia : MonoBehaviour
{
    private SpriteRenderer sr;
    private JefeFinalNivel5 jefe;
    private Sprite spriteDisparo;
    private float duracionCarga;
    private float velocidad;
    private float tiempo;
    private bool disparada;
    private Vector3 direccion;
    private Vector3 escalaFinal;

    public void Configurar(JefeFinalNivel5 jefe, Sprite carga, Sprite disparo,
        float duracionCarga, float velocidad, int sortingLayerID, int sortingOrder)
    {
        this.jefe = jefe;
        this.spriteDisparo = disparo;
        this.duracionCarga = duracionCarga;
        this.velocidad = velocidad;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = carga;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = sortingOrder;
        sr.color = new Color(1f, 1f, 1f, 0f);

        escalaFinal = transform.localScale;
        transform.localScale = Vector3.zero;

        Destroy(gameObject, 12f);
    }

    private void Update()
    {
        if (sr == null) return;

        if (!disparada)
        {
            tiempo += Time.deltaTime;
            float alfa = Mathf.Clamp01(tiempo / duracionCarga);
            sr.color = new Color(1f, 1f, 1f, alfa);
            transform.localScale = escalaFinal * alfa;

            if (jefe != null)
                transform.position = jefe.transform.position + (Vector3)jefe.offsetBola;

            if (alfa >= 1f) Lanzar();
        }
        else
        {
            transform.position += direccion * (velocidad * Time.deltaTime);
        }
    }

    private void Lanzar()
    {
        disparada = true;
        sr.sprite = spriteDisparo;

        var ruli = FindFirstObjectByType<RuliMovimiento>();
        direccion = ruli != null
            ? (ruli.transform.position - transform.position).normalized
            : Vector3.left;

        var col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f;

        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!disparada) return;

        var salud = other.GetComponent<RuliSalud>();
        if (salud != null)
        {
            salud.RecibirDaño();
            Destroy(gameObject);
        }
        else if (other.CompareTag("piso"))
        {
            Destroy(gameObject);
        }
    }
}
