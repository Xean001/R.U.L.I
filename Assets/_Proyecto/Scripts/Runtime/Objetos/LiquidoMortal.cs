using UnityEngine;

// Liquido que sale de la alcantarilla. Si Ruli lo toca mientras esta fuera,
// muere (pierde todas las vidas). Usa collider trigger.
[RequireComponent(typeof(Collider2D))]
public class LiquidoMortal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) => Matar(other);
    void OnTriggerStay2D(Collider2D other)  => Matar(other);

    void Matar(Collider2D other)
    {
        // Muerte instantanea (pierde todas las vidas) -> GameOver
        var mov = other.GetComponent<RuliMovimiento>();
        if (mov != null) { mov.Morir(); return; }

        var salud = other.GetComponent<RuliSalud>();
        if (salud != null) salud.RecibirDaño(9999);
    }
}
