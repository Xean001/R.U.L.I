using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class Cilindro : MonoBehaviour
{
    [Tooltip("Frames de la destruccion (en orden)")]
    public Sprite[] frames;

    [Tooltip("Cuantos golpes para avanzar al siguiente frame")]
    public int golpesPorFrame = 3;

    [Tooltip("Golpes extra despues del ultimo frame para destruir el objeto")]
    public int golpesExtraFinal = 3;

    public GameObject monedaPrefab;

    private int golpesAcumulados;
    private int frameActual;
    private int golpesExtraAcumulados;
    private bool enFaseFinal;
    private SpriteRenderer sr;
    private PolygonCollider2D poly;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        poly = GetComponent<PolygonCollider2D>();
        var anim = GetComponent<Animator>();
        if (anim != null) anim.enabled = false;
        if (frames != null && frames.Length > 0) AplicarFrame(0);
    }

    public void Golpe()
    {
        if (frames == null || frames.Length == 0) return;

        if (enFaseFinal)
        {
            golpesExtraAcumulados++;
            if (golpesExtraAcumulados >= golpesExtraFinal)
            {
                if (monedaPrefab != null)
                    Instantiate(monedaPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                Destroy(gameObject);
            }
            return;
        }

        golpesAcumulados++;
        if (golpesAcumulados < golpesPorFrame) return;

        golpesAcumulados = 0;
        frameActual++;

        if (frameActual < frames.Length)
        {
            AplicarFrame(frameActual);
        }
        else
        {
            enFaseFinal = true;
            frameActual = frames.Length - 1;
        }
    }

    void AplicarFrame(int idx)
    {
        if (sr != null) sr.sprite = frames[idx];
        ActualizarColliderDesdeSprite(frames[idx]);
    }

    void ActualizarColliderDesdeSprite(Sprite sprite)
    {
        if (poly == null || sprite == null) return;

        int shapeCount = sprite.GetPhysicsShapeCount();
        if (shapeCount > 0)
        {
            poly.pathCount = shapeCount;
            var temp = new List<Vector2>();
            for (int i = 0; i < shapeCount; i++)
            {
                sprite.GetPhysicsShape(i, temp);
                poly.SetPath(i, temp.ToArray());
            }
        }
        else
        {
            var size = sprite.bounds.size;
            float hw = size.x * 0.5f;
            float hh = size.y * 0.5f;
            poly.pathCount = 1;
            poly.SetPath(0, new[] {
                new Vector2(-hw, -hh),
                new Vector2( hw, -hh),
                new Vector2( hw,  hh),
                new Vector2(-hw,  hh),
            });
        }
    }
}
