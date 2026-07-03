using UnityEngine;

// Zona de bloqueo de camara para la arena del jefe.
// Al entrar el jugador, desactiva el CameraFollow de la camara
// y la desplaza suavemente hasta una posicion fija.
[RequireComponent(typeof(BoxCollider2D))]
public class CameraLockZone : MonoBehaviour
{
    [SerializeField] private Vector2 posicionFija = Vector2.zero;
    [SerializeField] private float velocidadTransicion = 2f;

    private Transform camara;
    private bool activado;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado) return;
        if (other.GetComponent<RuliMovimiento>() == null) return;

        activado = true;
        camara = Camera.main.transform;
        var follow = camara.GetComponent<CameraFollow>();
        if (follow != null) follow.enabled = false;
    }

    private void LateUpdate()
    {
        if (!activado || camara == null) return;

        Vector3 destino = new Vector3(posicionFija.x, posicionFija.y, camara.position.z);
        camara.position = Vector3.Lerp(camara.position, destino, velocidadTransicion * Time.deltaTime);
    }
}
