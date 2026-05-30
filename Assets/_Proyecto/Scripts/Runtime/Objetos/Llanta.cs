using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class Llanta : MonoBehaviour
{
    public float fuerzaRebote = 15f;

    void OnCollisionEnter2D(Collision2D col)
    {
        var rb = col.rigidbody;
        if (rb == null) return;

        foreach (ContactPoint2D c in col.contacts)
        {
            // Normal apunta hacia arriba → Ruli cae desde arriba
            if (c.normal.y > 0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaRebote);
                break;
            }
        }
    }
}
