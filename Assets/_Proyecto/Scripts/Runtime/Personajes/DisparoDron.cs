using UnityEngine;

// (reimport) Proyectil del dron. Viaja en linea recta en la direccion con la que se lanza
// (gravedad 0) y DESAPARECE al tocar cualquier collider (plataforma, estructura,
// etc.). Si toca a Ruli le hace daño. Ignora al propio dron.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class DisparoDron : MonoBehaviour
{
    [Tooltip("Tiempo de vida maximo por si no choca con nada (segundos).")]
    public float vidaMaxSegundos = 5f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    // Llamado por EnemigoDron al instanciar.
    public void Lanzar(Vector2 velocidad)
    {
        rb.linearVelocity = velocidad;
        Destroy(gameObject, vidaMaxSegundos);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar al dron que lo disparo
        if (other.GetComponent<EnemigoDron>() != null) return;

        var salud = other.GetComponent<RuliSalud>();
        if (salud != null) salud.RecibirDaño();

        Destroy(gameObject);   // desaparece al tocar plataforma o cualquier cosa
    }
}
