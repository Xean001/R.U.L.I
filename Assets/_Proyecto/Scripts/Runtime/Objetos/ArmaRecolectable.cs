using UnityEngine;

/// <summary>
/// Objeto que cae cuando muere el enemigo (escudero/jefe). Cuando Ruli lo
/// toca, dispara la pantalla de Victoria y desaparece.
/// Ponlo en un GameObject con SpriteRenderer + Collider2D + Rigidbody2D.
/// Debe estar INACTIVO en la escena; el enemigo lo activa al morir.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ArmaRecolectable : MonoBehaviour
{
    [Tooltip("Nivel que se marca como completado al recogerla.")]
    public int nivelActual = 1;

    [Tooltip("Flag de PlayerPrefs que se activa al recogerla. Habilita el arma en la rueda de habilidades.")]
    public string flagArma = "ArmaConseguida";

    [Header("Flotacion (cuando ya esta en el suelo)")]
    public float amplitud = 0.12f;
    public float velocidad = 3f;

    private bool recogida;

    private void OnCollisionEnter2D(Collision2D col) => Intentar(col.collider);
    private void OnTriggerEnter2D(Collider2D other) => Intentar(other);

    private void Intentar(Collider2D c)
    {
        if (recogida) return;
        // Solo Ruli la recoge
        if (c.GetComponentInParent<RuliSalud>() == null &&
            c.GetComponentInParent<RuliMovimiento>() == null)
            return;

        recogida = true;

        // Marca el arma como conseguida -> se habilita en la rueda de habilidades.
        if (!string.IsNullOrEmpty(flagArma))
        {
            PlayerPrefs.SetInt(flagArma, 1);
            PlayerPrefs.Save();
        }

        var vc = Object.FindFirstObjectByType<VictoryController>();
        if (vc != null)
        {
            vc.nivelActual = nivelActual;
            vc.MostrarVictoria();
        }
        else
        {
            Debug.LogError("ArmaRecolectable: no se encontro VictoryController en la escena.");
        }

        Destroy(gameObject);
    }
}
