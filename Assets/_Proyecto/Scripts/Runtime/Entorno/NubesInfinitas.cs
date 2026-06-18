using UnityEngine;

public class NubesInfinitas : MonoBehaviour
{
    [Header("Velocidad de movimiento")]
    [Tooltip("Velocidad del desplazamiento de las nubes")]
    public float velocidad = 0.02f;

    [Tooltip("Dirección: 1 = derecha, -1 = izquierda")]
    public float direccion = 1f;

    [Header("Eje de movimiento")]
    [Tooltip("Mover en X (horizontal)")]
    public bool moverEnX = true;

    [Tooltip("Mover en Y (vertical, para nubes que suben/bajan)")]
    public bool moverEnY = false;

    [Header("Material de las nubes")]
    [Tooltip("Arrastra el material del cilindro/esfera aquí")]
    public Material materialNubes;

    private Vector2 offsetActual;

    void Start()
    {
        // Si no se asignó material, intentar obtenerlo del renderer
        if (materialNubes == null)
        {
            var renderer = GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                materialNubes = renderer.material;
            }
            else
            {
                Debug.LogWarning("NubesInfinitas: No se encontró material. Asigna uno en el Inspector.");
                enabled = false;
                return;
            }
        }

        offsetActual = materialNubes.mainTextureOffset;
    }

    void Update()
    {
        if (materialNubes == null) return;

        // Calcular nuevo offset
        float desplazamientoX = moverEnX ? velocidad * direccion * Time.deltaTime : 0f;
        float desplazamientoY = moverEnY ? velocidad * direccion * Time.deltaTime : 0f;

        offsetActual += new Vector2(desplazamientoX, desplazamientoY);

        // Aplicar el offset a la textura
        materialNubes.mainTextureOffset = offsetActual;
    }
}