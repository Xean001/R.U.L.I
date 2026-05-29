using UnityEngine;

/// <summary>
/// Asigna el Sorting Layer y Order in Layer a todos los SpriteRenderers hijos.
/// Ponlo en el Empty Object padre de cada grupo (Ruli, Ground, Escenary, Background).
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class OrdenCapas : MonoBehaviour
{
    public enum Capa
    {
        Background  = 0,
        Escenary    = 1,
        Ground      = 2,
        Player      = 3
    }

    [SerializeField] private Capa capa = Capa.Player;
    [SerializeField] private int ordenBase = 0;

    private void OnValidate() => Aplicar();
    private void Awake()      => Aplicar();
    private void OnEnable()   => Aplicar();

    [ContextMenu("Aplicar Orden")]
    public void Aplicar()
    {
        string nombreCapa = capa.ToString();
        var renderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in renderers)
        {
            sr.sortingLayerName = nombreCapa;
            sr.sortingOrder     = ordenBase;
        }
    }
}
