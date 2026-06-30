using UnityEngine;

/// <summary>
/// Proyectil que dispara Ruli con el arma/soplador. Viaja recto (sin gravedad),
/// daña enemigos/objetos rompibles al tocarlos y desaparece. Ignora a Ruli y a
/// los pickups tipo trigger (monedas).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProyectilRuli : MonoBehaviour
{
    [Tooltip("Tiempo de vida maximo por si no choca con nada (segundos).")]
    public float vidaMaxSegundos = 4f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    public void Lanzar(Vector2 velocidad)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = velocidad;
        Destroy(gameObject, vidaMaxSegundos);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar a quien dispara (Ruli) y a otros proyectiles
        if (other.GetComponentInParent<RuliMovimiento>() != null) return;
        if (other.GetComponent<ProyectilRuli>() != null) return;

        bool golpeoEnemigo = false;

        var cucaracha = other.GetComponent<EnemigoCucaracha>();
        if (cucaracha != null) { cucaracha.Golpe(); golpeoEnemigo = true; }

        var arena = other.GetComponent<EnemigoArena>();
        if (arena != null) { arena.Golpe(); golpeoEnemigo = true; }

        var escudo = other.GetComponent<EscudoEnemigo>();
        if (escudo != null) { escudo.Golpe(); golpeoEnemigo = true; }

        var pelicano = other.GetComponent<EnemigoPelicano>();
        if (pelicano != null) { pelicano.Golpe(); golpeoEnemigo = true; }

        var librero = other.GetComponent<EnemigoLibreroJefe>();
        if (librero != null) { librero.Golpe(); golpeoEnemigo = true; }

        var dron = other.GetComponent<EnemigoDron>();
        if (dron != null) { dron.Golpe(); golpeoEnemigo = true; }

        var rompible = other.GetComponent<ObjetoRompible>();
        if (rompible != null) { rompible.Golpe(); golpeoEnemigo = true; }

        var cilindro = other.GetComponent<Cilindro>();
        if (cilindro != null) { cilindro.Golpe(); golpeoEnemigo = true; }

        // Atraviesa pickups (triggers tipo moneda). Desaparece al pegar a un
        // enemigo o a algo solido (plataforma, estructura).
        if (golpeoEnemigo || !other.isTrigger)
            Destroy(gameObject);
    }
}
