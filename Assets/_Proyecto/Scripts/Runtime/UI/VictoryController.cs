using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryController : MonoBehaviour
{
    [Header("Panel Victoria")]
    public RectTransform panelVictoria;

    [Header("Botones")]
    public Button btnSiguienteNivel;
    public Button btnMenu;

    [Header("Animación")]
    public float duracionAnimacion = 0.4f;
    public Vector2 posicionOculto = new Vector2(0, -508.88f); // Escondido arriba
    public Vector2 posicionVisible = Vector2.zero; // Top 0, Bottom 0

    [Header("Audio")]
    public AudioClip sonidoAparicion;
    public AudioClip sonidoNavegacion;
    public AudioClip sonidoSeleccion;
    private AudioSource audioSource;
    private AudioSource audioCameraOriginal;

    [Header("Animación de Botones")]
    public float escalaMinima = 1f;
    public float escalaMaxima = 1.15f;
    public float velocidadPulse = 3f;

    [Header("Niveles")]
    public int nivelActual = 1;
    public string escenaMenu = "MenuPrincipal";

    private bool victoriaMostrada = false;
    private bool puedeNavegar = false;
    private int indiceActual = 0;
    private Button[] botones;
    private Coroutine coroutinePulse;
    private Vector3 escalaOriginal;
    private bool animando = false;

    void Awake()
    {
        // Inicialmente oculto
        if (panelVictoria != null)
        {
            panelVictoria.localScale = Vector3.zero;
            panelVictoria.anchoredPosition = posicionOculto;
        }

        // Configurar botones
        botones = new Button[] { btnSiguienteNivel, btnMenu };
        ConfigurarClicks();

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void ConfigurarClicks()
    {
        if (btnSiguienteNivel != null)
            btnSiguienteNivel.onClick.AddListener(() => SeleccionarOpcionPorClick(0));
        if (btnMenu != null)
            btnMenu.onClick.AddListener(() => SeleccionarOpcionPorClick(1));
    }

    private void SeleccionarOpcionPorClick(int indice)
    {
        if (!puedeNavegar || animando) return;
        if (botones == null || indice < 0 || indice >= botones.Length) return;

        indiceActual = indice;
        ActualizarSeleccionBotones();
        ReproducirSonido(sonidoSeleccion);
        SeleccionarOpcion();
    }

    public void MostrarVictoria()
    {
        if (victoriaMostrada || animando) return;
        victoriaMostrada = true;
        animando = true;

        // CONGELAR EL JUEGO
        Time.timeScale = 0f;

        // Mutear música del juego
        MutearMusicaJuego(true);

        // Sonido de aparición
        ReproducirSonido(sonidoAparicion);

        // Marcar nivel como completado
        MarcarNivelCompletado();

        StartCoroutine(AnimacionEntrada());
    }

    void MarcarNivelCompletado()
    {
        // Marcar nivel actual como completado
        PlayerPrefs.SetInt("Nivel" + nivelActual + "Completado", 1);
        PlayerPrefs.SetInt("Nivel" + nivelActual + "Desbloqueado", 1);
        // Desbloquear siguiente nivel
        PlayerPrefs.SetInt("Nivel" + (nivelActual + 1) + "Desbloqueado", 1);
        PlayerPrefs.Save();
    }

    IEnumerator AnimacionEntrada()
    {
        float tiempo = 0f;
        Vector3 escalaInicial = Vector3.zero;
        Vector2 posInicial = posicionOculto;

        // Fase 1: Aparecer (scale 0 -> 1)
        while (tiempo < duracionAnimacion * 0.5f)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / (duracionAnimacion * 0.5f);
            panelVictoria.localScale = Vector3.Lerp(escalaInicial, Vector3.one, t);
            yield return null;
        }

        // Fase 2: Bajar (posición oculto -> posición visible)
        tiempo = 0f;
        while (tiempo < duracionAnimacion * 0.5f)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / (duracionAnimacion * 0.5f);
            panelVictoria.anchoredPosition = Vector2.Lerp(posInicial, posicionVisible, t);
            yield return null;
        }

        animando = false;
        puedeNavegar = true;
        indiceActual = 0; // Seleccionar "Siguiente Nivel" por defecto
        ActualizarSeleccionBotones();
    }

    void Update()
    {
        if (!puedeNavegar) return;


        if (RuliInput.MenuArribaPresionado())
        {
            indiceActual = (indiceActual - 1 + botones.Length) % botones.Length;
            ReproducirSonido(sonidoNavegacion);
            ActualizarSeleccionBotones();
        }
        else if (RuliInput.MenuAbajoPresionado())
        {
            indiceActual = (indiceActual + 1) % botones.Length;
            ReproducirSonido(sonidoNavegacion);
            ActualizarSeleccionBotones();
        }

        if (RuliInput.SubmitPresionado())
        {
            ReproducirSonido(sonidoSeleccion);
            SeleccionarOpcion();
        }
        else if (RuliInput.CancelPresionado())
        {
            ReproducirSonido(sonidoSeleccion);
            VolverAlMenu();
        }
    }

    void ActualizarSeleccionBotones()
    {
        // Detener animación anterior
        if (coroutinePulse != null)
        {
            StopCoroutine(coroutinePulse);
            coroutinePulse = null;
        }

        // Resetear escala de todos los botones
        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] != null)
                botones[i].transform.localScale = Vector3.one;
        }

        // Iniciar animación en el botón seleccionado
        if (botones[indiceActual] != null && puedeNavegar)
        {
            coroutinePulse = StartCoroutine(AnimacionPulseBoton(botones[indiceActual]));
        }
    }

    IEnumerator AnimacionPulseBoton(Button boton)
    {
        float tiempo = 0f;
        escalaOriginal = boton.transform.localScale;

        while (puedeNavegar && victoriaMostrada)
        {
            tiempo += Time.unscaledDeltaTime * velocidadPulse;
            float escala = escalaMinima + (escalaMaxima - escalaMinima) * (Mathf.Sin(tiempo) * 0.5f + 0.5f);
            boton.transform.localScale = escalaOriginal * escala;
            yield return null;
        }

        if (boton != null)
            boton.transform.localScale = escalaOriginal;
    }

    void SeleccionarOpcion()
    {
        puedeNavegar = false;

        switch (indiceActual)
        {
            case 0: SiguienteNivel(); break;
            case 1: VolverAlMenu(); break;
        }
    }

    public void SiguienteNivel()
    {
        // DESCONGELAR EL JUEGO
        Time.timeScale = 1f;
        MutearMusicaJuego(false);

        // Cargar siguiente nivel
        string siguienteNivel = "Nivel" + (nivelActual + 1);
        SceneManager.LoadScene(siguienteNivel);
    }

    public void VolverAlMenu()
    {
        // DESCONGELAR EL JUEGO
        Time.timeScale = 1f;
        MutearMusicaJuego(false);

        SceneManager.LoadScene(escenaMenu);
    }

    void MutearMusicaJuego(bool mutear)
    {
        if (audioCameraOriginal == null)
        {
            var mainCam = Camera.main;
            if (mainCam != null)
                audioCameraOriginal = mainCam.GetComponent<AudioSource>();
        }

        if (audioCameraOriginal != null)
            audioCameraOriginal.mute = mutear;
    }

    void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
