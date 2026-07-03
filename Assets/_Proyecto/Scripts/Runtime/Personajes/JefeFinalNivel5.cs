using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Jefe final del Nivel 5. Espera fuera de pantalla a la derecha;
// cuando Ruli entra a la arena, se desplaza hasta el centro y combate:
// animacion idle por frames, vaivén vertical leve y disparo de bolas de energia.
[RequireComponent(typeof(SpriteRenderer))]
public class JefeFinalNivel5 : MonoBehaviour
{
    [Header("Animacion idle")]
    public Sprite[] framesIdle;
    public float cuadrosPorSegundo = 8f;

    [Header("Entrada")]
    public float xActivacion = 72.2f;
    public Vector2 posicionCentro = new Vector2(77.2f, -1.4f);
    public float velocidadEntrada = 4f;

    [Header("Vuelo")]
    public float amplitudVuelo = 0.15f;
    public float frecuenciaVuelo = 2f;

    [Header("Fase 2 (mitad de vida)")]
    public float amplitudMovimientoX = 3f;   // cuanto se desplaza a cada lado del centro
    public float frecuenciaMovimientoX = 0.6f;
    public float velocidadBolaExtra = 1f;    // se suma a velocidadBola en fase 2
    public float intervaloDisparoFase2 = 6f; // reemplaza a intervaloDisparo en fase 2
    public float duracionCargaFase2 = 3f;    // reemplaza a duracionCarga en fase 2

    [Header("Ataque")]
    public Sprite spriteCarga;
    public Sprite spriteDisparo;
    public float intervaloDisparo = 9f;
    public float duracionCarga = 6f;
    public float velocidadBola = 8f;
    public float escalaBola = 0.6f;
    public Vector2 offsetBola = new Vector2(0.5f, 0f);

    [Header("Vida")]
    public int vidaMax = 20;
    public AudioClip sonidoDanio;
    [Range(0f, 2f)] public float volumenDanio = 1f;
    public float radioPuntoDebil = 0.5f;   // zona de danio donde carga la bola

    [Header("Presentacion")]
    public Sprite spritePuntoDebil;        // indicador que parpadea durante el dialogo
    public float parpadeosPorSegundo = 3f;

    [Header("Muerte")]
    public Sprite[] framesExplosion;       // frames del efecto de explosion
    public int numExplosiones = 5;         // cuantas explosiones sobre el cuerpo
    public float cuadrosExplosion = 12f;   // frames por segundo de cada explosion
    public float escalaExplosion = 1f;
    public AudioClip sonidoExplosion;
    [Range(0f, 2f)] public float volumenExplosion = 1f;

    [Header("Final del nivel")]
    public Sprite spriteRuliFinal;         // ruliSprite__23: pose de Ruli viendo la muerte del jefe
    public float duracionCaminata = 4f;    // cuanto camina Ruli antes del fundido
    public float velocidadCaminata = 2f;
    public float duracionFundido = 1.5f;   // fundido a negro
    public string escenaCreditos = "Creditos";

    // true desde que empieza a cargar la bola hasta que la dispara
    public bool Cargando { get; private set; }

    // true desde que Ruli activa al jefe (entrada y combate)
    public bool EnCombate => estado == Estado.Entrando || estado == Estado.Combate;

    // true cuando le queda la mitad de vida o menos
    public bool FaseDos => vida <= vidaMax / 2;

    private SpriteRenderer sr;
    private Transform jugador;
    private enum Estado { Esperando, Entrando, Combate, Muerto }
    private Estado estado = Estado.Esperando;
    private float tiempoAnim;
    private int frameActual;
    private float tiempoDisparo;
    private int vida;
    private EnemigoVidaUI vidaUI;
    private NPCDialogo dialogo;
    private float inicioFase2 = -1f;   // momento en que entro a fase 2

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        var ruli = FindFirstObjectByType<RuliMovimiento>();
        if (ruli != null) jugador = ruli.transform;

        vida = vidaMax;
        vidaUI = FindFirstObjectByType<EnemigoVidaUI>();

        // Punto debil: trigger en la posicion donde carga la bola
        var punto = new GameObject("PuntoDebil");
        punto.transform.SetParent(transform, false);
        punto.transform.localPosition = offsetBola;
        var colPunto = punto.AddComponent<CircleCollider2D>();
        colPunto.isTrigger = true;
        colPunto.radius = radioPuntoDebil;
        punto.AddComponent<PuntoDebilJefe>().jefe = this;

        dialogo = GetComponent<NPCDialogo>();
    }

    private void Update()
    {
        if (estado == Estado.Muerto) return;   // congelado durante las explosiones

        AnimarIdle();

        switch (estado)
        {
            case Estado.Esperando:
                if (jugador != null && jugador.position.x >= xActivacion)
                {
                    estado = Estado.Entrando;
                    if (vidaUI != null)
                    {
                        vidaUI.Mostrar();
                        vidaUI.SetVida(1f);
                    }
                    if (dialogo != null)
                    {
                        dialogo.Mostrar();
                        StartCoroutine(ParpadearPuntoDebil());
                    }
                }
                break;

            case Estado.Entrando:
                Vector3 destino = new Vector3(posicionCentro.x, posicionCentro.y, transform.position.z);
                transform.position = Vector3.MoveTowards(transform.position, destino, velocidadEntrada * Time.deltaTime);
                if (Vector3.Distance(transform.position, destino) < 0.01f)
                {
                    tiempoDisparo = 0f;
                    estado = Estado.Combate;
                }
                break;

            case Estado.Combate:
                float y = posicionCentro.y + Mathf.Sin(Time.time * frecuenciaVuelo) * amplitudVuelo;

                // Fase 2: ademas del vaiven vertical, patrulla de izquierda a derecha.
                // La onda arranca desde el centro (sin(0) = 0) y la amplitud crece
                // suave el primer segundo para que no haya salto.
                float xCombate = posicionCentro.x;
                if (FaseDos)
                {
                    if (inicioFase2 < 0f) inicioFase2 = Time.time;
                    float transcurrido = Time.time - inicioFase2;
                    float amplitud = amplitudMovimientoX * Mathf.Clamp01(transcurrido);
                    xCombate += Mathf.Sin(transcurrido * frecuenciaMovimientoX) * amplitud;
                }

                transform.position = new Vector3(xCombate, y, transform.position.z);

                tiempoDisparo += Time.deltaTime;
                if (tiempoDisparo >= (FaseDos ? intervaloDisparoFase2 : intervaloDisparo))
                {
                    tiempoDisparo = 0f;
                    Disparar();
                }
                break;
        }
    }

    private void AnimarIdle()
    {
        if (framesIdle == null || framesIdle.Length == 0) return;

        tiempoAnim += Time.deltaTime;
        if (tiempoAnim >= 1f / cuadrosPorSegundo)
        {
            tiempoAnim = 0f;
            frameActual = (frameActual + 1) % framesIdle.Length;
            sr.sprite = framesIdle[frameActual];
        }
    }

    // Recibe un golpe de Ruli (proyectil, tornado, etc.) y actualiza la barra.
    public void Golpe()
    {
        if (vida <= 0) return;

        vida--;
        if (vidaUI != null) vidaUI.SetVida((float)vida / vidaMax);

        if (sonidoDanio != null)
        {
            var sonidoGO = new GameObject("SonidoDanio");
            var fuente = sonidoGO.AddComponent<AudioSource>();
            fuente.clip = sonidoDanio;
            fuente.spatialBlend = 0f;   // 2D, sin atenuacion por distancia
            fuente.volume = Mathf.Clamp01(volumenDanio);
            fuente.Play();
            Destroy(sonidoGO, sonidoDanio.length);
        }

        if (vida <= 0) Morir();
    }

    private void Morir()
    {
        estado = Estado.Muerto;   // se queda estatico donde lo mataron
        if (vidaUI != null) vidaUI.Ocultar();

        // Se acabaron las rocas (incluidas las que estaban a medio salir)
        var rocas = FindFirstObjectByType<RocasEmergentes>();
        if (rocas != null) rocas.Detener();

        // Ruli se detiene a ver la escena con su pose final
        var mov = FindFirstObjectByType<RuliMovimiento>();
        if (mov != null)
        {
            var rbRuli = mov.GetComponent<Rigidbody2D>();
            if (rbRuli != null) rbRuli.linearVelocity = Vector2.zero;
            mov.enabled = false;

            var animRuli = mov.GetComponent<Animator>();
            if (animRuli != null) animRuli.enabled = false;   // que no pise el sprite
            var srRuli = mov.GetComponent<SpriteRenderer>();
            if (srRuli != null && spriteRuliFinal != null) srRuli.sprite = spriteRuliFinal;
        }

        StartCoroutine(SecuenciaExplosiones());
    }

    // Explosiones una tras otra en distintas partes del cuerpo; al final desaparece.
    private IEnumerator SecuenciaExplosiones()
    {
        if (framesExplosion != null && framesExplosion.Length > 0)
        {
            for (int i = 0; i < numExplosiones; i++)
            {
                // Punto al azar dentro del cuerpo del jefe
                Vector3 offset = new Vector3(
                    Random.Range(-sr.bounds.extents.x, sr.bounds.extents.x) * 0.7f,
                    Random.Range(-sr.bounds.extents.y, sr.bounds.extents.y) * 0.7f,
                    0f);

                var exp = new GameObject("Explosion");
                exp.transform.position = sr.bounds.center + offset;
                exp.transform.localScale = Vector3.one * escalaExplosion;
                var srExp = exp.AddComponent<SpriteRenderer>();
                srExp.sortingLayerID = sr.sortingLayerID;
                srExp.sortingOrder = sr.sortingOrder + 2;

                if (sonidoExplosion != null)
                {
                    // Fuente 2D en objeto aparte: volumen pleno sin atenuacion por
                    // distancia, y vive lo que dure el clip (no se corta con el efecto)
                    var sonidoGO = new GameObject("SonidoExplosion");
                    var fuente = sonidoGO.AddComponent<AudioSource>();
                    fuente.clip = sonidoExplosion;
                    fuente.spatialBlend = 0f;   // 2D
                    fuente.volume = Mathf.Clamp01(volumenExplosion);
                    fuente.Play();
                    Destroy(sonidoGO, sonidoExplosion.length);
                }

                foreach (var frame in framesExplosion)
                {
                    srExp.sprite = frame;
                    yield return new WaitForSeconds(1f / cuadrosExplosion);
                }
                Destroy(exp);
            }
        }

        // El jefe desaparece (se oculta: el objeto sigue vivo para dirigir el final)
        sr.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

        yield return StartCoroutine(FinalDelNivel());
    }

    // Ruli camina hacia adelante con la camara quieta, fundido a negro y creditos.
    private IEnumerator FinalDelNivel()
    {
        // La camara deja de seguir a Ruli
        if (Camera.main != null)
        {
            var seguimiento = Camera.main.GetComponent<CameraFollow>();
            if (seguimiento != null) seguimiento.enabled = false;
        }

        // Quitar los muros limite para que Ruli salga caminando sin chocar
        foreach (string muro in new[] { "MuroLimiteJefeInicio", "MuroLimiteJefeFin", "MuroLimiteIzquierdo" })
        {
            var go = GameObject.Find(muro);
            if (go == null) continue;
            foreach (var col in go.GetComponentsInChildren<Collider2D>()) col.enabled = false;
        }

        // Ruli camina hacia adelante (animacion de correr, sin control del jugador)
        var mov = FindFirstObjectByType<RuliMovimiento>();
        Animator animRuli = null;
        if (mov != null)
        {
            animRuli = mov.GetComponent<Animator>();
            if (animRuli != null)
            {
                animRuli.enabled = true;
                animRuli.SetBool("en suelo", true);
            }
            // Mirando a la derecha
            Vector3 esc = mov.transform.localScale;
            esc.x = Mathf.Abs(esc.x);
            mov.transform.localScale = esc;
        }

        float t = 0f;
        while (t < duracionCaminata)
        {
            t += Time.deltaTime;
            if (mov != null)
            {
                mov.transform.position += Vector3.right * (velocidadCaminata * Time.deltaTime);
                if (animRuli != null) animRuli.SetFloat("velocidadX", 1f);
            }
            yield return null;
        }

        // Fundido a negro ("se apaga la luz")
        var canvasGO = new GameObject("FundidoFinal");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var negro = new GameObject("Negro");
        negro.transform.SetParent(canvasGO.transform, false);
        var img = negro.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0f);
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        t = 0f;
        while (t < duracionFundido)
        {
            t += Time.deltaTime;
            img.color = new Color(0f, 0f, 0f, Mathf.Clamp01(t / duracionFundido));
            if (mov != null)   // sigue caminando mientras oscurece
            {
                mov.transform.position += Vector3.right * (velocidadCaminata * Time.deltaTime);
                if (animRuli != null) animRuli.SetFloat("velocidadX", 1f);
            }
            yield return null;
        }

        SceneManager.LoadScene(escenaCreditos);
    }

    // La bola avisa cuando termina la carga y sale disparada
    public void BolaLanzada() { Cargando = false; }

    // Indicador del punto debil parpadeando, solo mientras dura el dialogo
    private IEnumerator ParpadearPuntoDebil()
    {
        if (spritePuntoDebil == null) yield break;

        var marcador = new GameObject("MarcadorPuntoDebil");
        marcador.transform.SetParent(transform, false);
        marcador.transform.localPosition = offsetBola;
        var srMarcador = marcador.AddComponent<SpriteRenderer>();
        srMarcador.sprite = spritePuntoDebil;
        srMarcador.sortingLayerID = sr.sortingLayerID;
        srMarcador.sortingOrder = sr.sortingOrder + 2;

        // Espera a que el dialogo realmente arranque
        yield return null;

        float t = 0f;
        while (dialogo != null && dialogo.Mostrando)
        {
            t += Time.deltaTime;
            float alfa = Mathf.PingPong(t * parpadeosPorSegundo * 2f, 1f);
            srMarcador.color = new Color(1f, 1f, 1f, alfa);
            yield return null;
        }

        Destroy(marcador);
    }

    private void Disparar()
    {
        Cargando = true;
        var go = new GameObject("BolaEnergia");
        go.transform.position = transform.position + (Vector3)offsetBola;
        go.transform.localScale = Vector3.one * escalaBola;
        var bola = go.AddComponent<BolaEnergia>();
        float velocidad = velocidadBola + (FaseDos ? velocidadBolaExtra : 0f);
        float carga = FaseDos ? duracionCargaFase2 : duracionCarga;
        bola.Configurar(this, spriteCarga, spriteDisparo, carga, velocidad, sr.sortingLayerID, sr.sortingOrder + 1);
    }
}

// Punto debil del jefe (donde carga la bola): el disparo de Ruli le baja vida
// solo cuando el jefe NO esta cargando.
public class PuntoDebilJefe : MonoBehaviour
{
    [HideInInspector] public JefeFinalNivel5 jefe;

    public void Golpe()
    {
        if (jefe != null && !jefe.Cargando)
            jefe.Golpe();
    }
}
