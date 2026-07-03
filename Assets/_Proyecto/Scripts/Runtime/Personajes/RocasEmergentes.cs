using System.Collections;
using UnityEngine;

// Ataque de rocas que emergen del suelo (Nivel 5).
// Cada 'intervalo' segundos: muestra 'rocaAviso' parpadeando en la posicion de
// Ruli durante 'duracionAviso', luego la roca (al azar entre las 3) sube desde
// el suelo, hace danio solo con la parte de arriba, espera 'tiempoArriba' y
// vuelve a bajar hasta desaparecer.
public class RocasEmergentes : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] rocas;              // roca1, roca2, roca3 (se alterna al azar)
    public Sprite aviso;                // rocaAviso

    [Header("Tiempos")]
    public float intervalo = 10f;       // cada cuanto sale una roca
    public float intervaloFase2 = 7f;   // intervalo cuando el jefe esta a mitad de vida
    public float duracionAviso = 2f;    // cuanto parpadea el aviso antes de subir
    public float tiempoArriba = 4f;     // cuanto queda la roca arriba
    public float velocidadSubida = 6f;
    public float parpadeosPorSegundo = 3f;

    [Header("Tamano")]
    public float escala = 0.5f;         // escala de la roca y el aviso (1 = tamano original)

    [Header("Arena del jefe")]
    [Tooltip("Las rocas solo salen mientras el jefe esta en combate, y dentro de estos limites.")]
    public Transform limiteInicio;      // MuroLimiteJefeInicio
    public Transform limiteFin;         // MuroLimiteJefeFin

    [Header("Posicion")]
    [Tooltip("Si esta activo, detecta la altura del piso bajo Ruli con un raycast; si no, usa ySuelo fijo.")]
    public bool detectarSuelo = false;
    public float ySuelo = -6.2f;         // altura fija donde salen la roca y el aviso

    [Header("Render")]
    public string capaAviso = "Player";
    public int ordenAviso = 5;
    public string capaRoca = "Player";  // por delante del piso
    public int ordenRoca = 5;

    private Transform ruli;
    private JefeFinalNivel5 jefe;

    void Start()
    {
        var mov = FindFirstObjectByType<RuliMovimiento>();
        if (mov != null) ruli = mov.transform;

        jefe = FindFirstObjectByType<JefeFinalNivel5>();
        if (jefe == null) Debug.LogWarning("RocasEmergentes: no se encontro al JefeFinalNivel5 en la escena.");

        // Si no se asignaron los limites en el Inspector, buscarlos por nombre
        if (limiteInicio == null)
        {
            var go = GameObject.Find("MuroLimiteJefeInicio");
            if (go != null) limiteInicio = go.transform;
        }
        if (limiteFin == null)
        {
            var go = GameObject.Find("MuroLimiteJefeFin");
            if (go != null) limiteFin = go.transform;
        }

        if (ruli == null) Debug.LogWarning("RocasEmergentes: no se encontro a Ruli en la escena.");
        if (rocas == null || rocas.Length == 0) Debug.LogWarning("RocasEmergentes: falta asignar los sprites de rocas en el Inspector.");
        if (aviso == null) Debug.LogWarning("RocasEmergentes: falta asignar el sprite de aviso en el Inspector.");

        StartCoroutine(Ciclo());
    }

    IEnumerator Ciclo()
    {
        while (true)
        {
            float espera = (jefe != null && jefe.FaseDos) ? intervaloFase2 : intervalo;
            yield return new WaitForSeconds(espera);
            if (ruli == null || rocas == null || rocas.Length == 0 || aviso == null) continue;

            // Solo durante la pelea con el jefe (si el jefe muere, se detienen)
            if (jefe == null || !jefe.EnCombate) continue;

            // Solo dentro de la arena, entre los dos muros limite
            float x = ruli.position.x;
            if (limiteInicio != null && limiteFin != null)
            {
                float xMin = Mathf.Min(limiteInicio.position.x, limiteFin.position.x);
                float xMax = Mathf.Max(limiteInicio.position.x, limiteFin.position.x);
                x = Mathf.Clamp(x, xMin, xMax);
            }

            StartCoroutine(LanzarRoca(x, BuscarSuelo()));
        }
    }

    // Corta el ciclo y elimina avisos y rocas que esten a medias (muerte del jefe)
    public void Detener()
    {
        StopAllCoroutines();
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    // Altura de la superficie del piso justo debajo de Ruli (raycast); si no
    // encuentra nada usa ySuelo como respaldo.
    float BuscarSuelo()
    {
        if (!detectarSuelo) return ySuelo;

        var hits = Physics2D.RaycastAll(ruli.position, Vector2.down, 20f);
        foreach (var h in hits)
        {
            if (h.collider.isTrigger) continue;
            if (h.collider.GetComponentInParent<RuliMovimiento>() != null) continue;
            return h.point.y;
        }
        return ySuelo;
    }

    IEnumerator LanzarRoca(float x, float ySuelo)
    {
        Sprite sprite = rocas[Random.Range(0, rocas.Length)];
        Debug.Log($"RocasEmergentes: aviso en x={x:F1}, ySuelo={ySuelo:F1}, roca '{sprite.name}'");
        float altura = sprite.bounds.size.y * escala;
        float ancho = sprite.bounds.size.x * escala;

        // --- Aviso parpadeando sobre el suelo ---
        var avisoGO = new GameObject("RocaAviso");
        avisoGO.transform.SetParent(transform, true);   // hijo del generador para poder limpiarlo
        avisoGO.transform.position = new Vector3(x, ySuelo + aviso.bounds.extents.y * escala, 0f);
        avisoGO.transform.localScale = Vector3.one * escala;
        var srAviso = avisoGO.AddComponent<SpriteRenderer>();
        srAviso.sprite = aviso;
        srAviso.sortingLayerName = capaAviso;
        srAviso.sortingOrder = ordenAviso;

        float t = 0f;
        while (t < duracionAviso)
        {
            t += Time.deltaTime;
            float alfa = Mathf.PingPong(t * parpadeosPorSegundo * 2f, 1f);
            srAviso.color = new Color(1f, 1f, 1f, alfa);
            yield return null;
        }
        Destroy(avisoGO);

        // --- Roca: nace enterrada y sube hasta asomar completa ---
        var rocaGO = new GameObject("Roca");
        rocaGO.transform.SetParent(transform, true);
        rocaGO.transform.position = new Vector3(x, ySuelo - altura / 2f, 0f);
        rocaGO.transform.localScale = Vector3.one * escala;
        var sr = rocaGO.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingLayerName = capaRoca;
        sr.sortingOrder = ordenRoca;

        // Cuerpo solido con la silueta del sprite (editable en el Sprite Editor
        // con Custom Physics Shape). Ruli choca con la roca, no la atraviesa.
        rocaGO.AddComponent<PolygonCollider2D>();

        // Rigidbody kinematico: mueve el collider correctamente y despierta
        // al rigidbody de Ruli aunque este quieto.
        var rbRoca = rocaGO.AddComponent<Rigidbody2D>();
        rbRoca.bodyType = RigidbodyType2D.Kinematic;

        // Danio por contacto real con la silueta, solo en la parte alta y
        // solo mientras sube (ver RocaGolpe)
        var golpe = rocaGO.AddComponent<RocaGolpe>();

        float yArriba = ySuelo + altura / 2f;
        while (rocaGO.transform.position.y < yArriba)
        {
            rocaGO.transform.position = Vector3.MoveTowards(
                rocaGO.transform.position,
                new Vector3(x, yArriba, 0f),
                velocidadSubida * Time.deltaTime);
            yield return null;
        }

        // Al terminar de subir deja de hacer danio (queda solo como obstaculo)
        golpe.activo = false;

        yield return new WaitForSeconds(tiempoArriba);

        // Baja y desaparece
        float yAbajo = ySuelo - altura / 2f;
        while (rocaGO.transform.position.y > yAbajo)
        {
            rocaGO.transform.position = Vector3.MoveTowards(
                rocaGO.transform.position,
                new Vector3(x, yAbajo, 0f),
                velocidadSubida * Time.deltaTime);
            yield return null;
        }
        Destroy(rocaGO);
    }
}

// Danio de la roca: usa los puntos de contacto de la colision real (la
// silueta del PolygonCollider2D), y solo golpea si el contacto ocurre en la
// franja superior de la roca y mientras 'activo' (fase de subida).
public class RocaGolpe : MonoBehaviour
{
    [HideInInspector] public bool activo = true;
    public float fraccionSuperior = 0.3f;   // franja alta de la silueta que hace danio

    private SpriteRenderer sr;

    void Awake() { sr = GetComponent<SpriteRenderer>(); }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!activo || sr == null) return;

        var salud = col.collider.GetComponent<RuliSalud>();
        if (salud == null) return;

        // Solo dania si algun punto de contacto esta en la parte alta de la roca
        float limite = sr.bounds.max.y - sr.bounds.size.y * fraccionSuperior;
        foreach (var c in col.contacts)
        {
            if (c.point.y >= limite)
            {
                salud.RecibirDaño();
                return;
            }
        }
    }
}
