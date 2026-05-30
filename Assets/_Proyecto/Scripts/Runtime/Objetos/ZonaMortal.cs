using UnityEngine;

public class ZonaMortal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        var mov = other.GetComponent<RuliMovimiento>();
        if (mov != null) mov.Morir();
    }
}
