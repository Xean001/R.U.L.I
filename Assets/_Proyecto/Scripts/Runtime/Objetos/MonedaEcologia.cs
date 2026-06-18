using UnityEngine;
using System.Collections;

public class MonedaEcologia : MonoBehaviour
{
    public float amplitudFlotacion = 0.15f;
    public float velocidadFlotacion = 2f;

    private Vector3 posBase;


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoAparicion;
    public AudioClip sonidoRecoleccion;

    void Start()
    {
        posBase = transform.position;

        if (audioSource != null && sonidoAparicion != null)
        {
            audioSource.PlayOneShot(sonidoAparicion);
        }
    }

    void Update()
    {
        transform.position = posBase + new Vector3(0f,
            Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<RuliMovimiento>() == null) return;
        ContadorMonedas.Instance?.Agregar(1);

        // SONIDO DE RECOLECCIÓN
        if (audioSource != null && sonidoRecoleccion != null)
        {
            audioSource.PlayOneShot(sonidoRecoleccion);
        }

        StartCoroutine(Recolectar());
    }

    IEnumerator Recolectar()
    {
        enabled = false; // detener flotación
        float t = 0f;
        Vector3 escalaInicial = transform.localScale;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            float p = t / 0.2f;
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, p);
            yield return null;
        }
        Destroy(gameObject);
    }
}
