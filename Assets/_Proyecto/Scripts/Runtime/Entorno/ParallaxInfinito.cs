using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Parallax infinito. Ponlo en Sky, Sombra/Lejos, Sombra/cerca, etc.
/// </summary>
public class ParallaxInfinito : MonoBehaviour
{
    [Header("Cámara")]
    [SerializeField] private Transform camara;

    [Header("Velocidad (0=fijo, 1=sigue cámara exacto)")]
    [SerializeField, Range(0f, 1f)] private float factorX = 0.2f;
    [SerializeField, Range(0f, 1f)] private float factorY = 0f;

    [Header("Loop infinito")]
    [SerializeField] private bool loopInfinito = true;

    private Vector3 camPosAnterior;
    private Transform[] tiles;
    private float totalAncho; // distancia real entre el primer y último tile + ancho de uno

    private void Start()
    {
        if (camara == null)
            camara = Camera.main?.transform;

        camPosAnterior = camara.position;
        InicializarTiles();
    }

    private void InicializarTiles()
    {
        var lista = new List<Transform>();

        foreach (Transform hijo in transform)
        {
            var sr = hijo.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                lista.Add(hijo);
            }
            else
            {
                foreach (Transform nieto in hijo)
                {
                    if (nieto.GetComponent<SpriteRenderer>() != null)
                        lista.Add(nieto);
                }
            }
        }

        // Ordenar de izquierda a derecha por posición X
        lista.Sort((a, b) => a.position.x.CompareTo(b.position.x));
        tiles = lista.ToArray();

        if (tiles.Length > 0)
        {
            // Ancho real = posición del último - posición del primero + ancho de un sprite
            var srPrimero = tiles[0].GetComponent<SpriteRenderer>();
            float anchoPorTile = srPrimero != null ? srPrimero.bounds.size.x : 0f;

            float xMin = tiles[0].position.x;
            float xMax = tiles[tiles.Length - 1].position.x;

            totalAncho = (xMax - xMin) + anchoPorTile;
        }
    }

    private void LateUpdate()
    {
        if (camara == null || tiles == null || tiles.Length == 0) return;

        Vector3 delta = camara.position - camPosAnterior;
        transform.position += new Vector3(delta.x * factorX, delta.y * factorY, 0f);
        camPosAnterior = camara.position;

        if (!loopInfinito || totalAncho <= 0f) return;

        foreach (var tile in tiles)
        {
            float distX = camara.position.x - tile.position.x;

            if (distX > totalAncho * 0.5f)
                tile.position += new Vector3(totalAncho, 0f, 0f);
            else if (distX < -totalAncho * 0.5f)
                tile.position -= new Vector3(totalAncho, 0f, 0f);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            InicializarTiles();
    }
#endif
}
