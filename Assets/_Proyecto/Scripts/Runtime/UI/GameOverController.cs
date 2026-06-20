using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [Header("Panel Game Over")]
    public RectTransform panelGameOver;

    [Header("Botones")]
    public Button btnReintentar;
    public Button btnMenu;

    [Header("Animación de Botones")]
    public float escalaMinima = 1f;
    public float escalaMaxima = 1.15f;
    public float velocidadPulse = 3f;
    private Coroutine coroutinePulse;
    private Vector3 escalaOriginal;


    [Header("Animación")]
    public float duracionAnimacion = 0.5f;
    public float escalaFinal = 1f;
    public Vector2 posicionFinal = Vector2.zero; // Top 0, Bottom 0
    public Vector2 posicionInicial = new Vector2(0, 449.8f); // Escondido arriba

    [Header("Audio")]
    public AudioClip sonidoAparicion;
    public AudioClip sonidoNavegacion;
    public AudioClip sonidoSeleccion;
    private AudioSource audioSource;

    private bool activo = false;
    private int indiceActual = 0;
    private Button[] botones;
    private AudioSource audioCameraOriginal; 
    private bool puedeNavegar = false;

    void Awake()
    {
        // Inicialmente oculto
        if (panelGameOver != null)
        {
            panelGameOver.localScale = Vector3.zero;
            panelGameOver.anchoredPosition = posicionInicial;
        }

        // Configurar botones
        botones = new Button[] { btnReintentar, btnMenu };
        ConfigurarClicks();

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void ConfigurarClicks()
    {
        if (btnReintentar != null)
            btnReintentar.onClick.AddListener(() => SeleccionarOpcionPorClick(0));
        if (btnMenu != null)
            btnMenu.onClick.AddListener(() => SeleccionarOpcionPorClick(1));
    }

    private void SeleccionarOpcionPorClick(int indice)
    {
        if (!puedeNavegar) return;
        if (botones == null || indice < 0 || indice >= botones.Length) return;

        indiceActual = indice;
        ActualizarSeleccionBotones();
        ReproducirSonido(sonidoSeleccion);
        SeleccionarOpcion();
    }

    public void MostrarGameOver()
    {
        if (activo) return;
        activo = true;

        // MUTEAR MÚSICA DEL JUEGO
        MutearMusicaJuego(true);

        // Sonido de aparición
        if (audioSource != null && sonidoAparicion != null)
            audioSource.PlayOneShot(sonidoAparicion);

        StartCoroutine(AnimacionEntrada());
    }

    IEnumerator AnimacionEntrada()
    {
        float tiempo = 0f;
        Vector3 escalaInicial = Vector3.zero;
        Vector2 posInicial = posicionInicial;

        // Fase 1: Aparecer (scale 0 -> 1)
        while (tiempo < duracionAnimacion * 0.5f)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / (duracionAnimacion * 0.5f);
            panelGameOver.localScale = Vector3.Lerp(escalaInicial, Vector3.one * escalaFinal, t);
            yield return null;
        }

        // Fase 2: Bajar (posición inicial -> posición final)
        tiempo = 0f;
        while (tiempo < duracionAnimacion * 0.5f)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / (duracionAnimacion * 0.5f);
            panelGameOver.anchoredPosition = Vector2.Lerp(posInicial, posicionFinal, t);
            yield return null;
        }

        // Activar navegación
        puedeNavegar = true;
        indiceActual = 0;  // ← SELECCIONAR REINTENTAR POR DEFECTO
        ActualizarSeleccionBotones();

        // Activar navegación
        puedeNavegar = true;
        indiceActual = 0;
        ActualizarSeleccionBotones();
    }

    void Update()
    {
        if (!puedeNavegar) return;


        // Navegación arriba/abajo
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

        // Selección
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

        while (puedeNavegar)
        {
            tiempo += Time.deltaTime * velocidadPulse;
            float escala = escalaMinima + (escalaMaxima - escalaMinima) * (Mathf.Sin(tiempo) * 0.5f + 0.5f);
            boton.transform.localScale = escalaOriginal * escala;
            yield return null;
        }

        // Resetear al tamaño original cuando termine
        if (boton != null)
            boton.transform.localScale = escalaOriginal;
    }

    void SeleccionarOpcion()
    {
        puedeNavegar = false;

        switch (indiceActual)
        {
            case 0: Reintentar(); break;
            case 1: VolverAlMenu(); break;
        }
    }

    public void Reintentar()
    {
        
        MutearMusicaJuego(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {

        MutearMusicaJuego(false);
        SceneManager.LoadScene("MenuPrincipal");
    }

    void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
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
}
