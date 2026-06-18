using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausaController : MonoBehaviour
{
    [Header("Panel Pausa")]
    public RectTransform panelPausa;

    [Header("Botones")]
    public Button btnRenudar;
    public Button btnReintentar;
    public Button btnSalir;

    [Header("Animación")]
    public float duracionAnimacion = 0.4f;
    public Vector2 posicionOculto = new Vector2(0, -500f); // Escondido arriba
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

    private bool estaPausado = false;
    private bool puedeNavegar = false;
    private int indiceActual = 0;
    private Button[] botones;
    private Coroutine coroutinePulse;
    private Vector3 escalaOriginal;
    private bool animando = false;

    void Awake()
    {
        // Inicialmente oculto
        if (panelPausa != null)
        {
            panelPausa.localScale = Vector3.zero;
            panelPausa.anchoredPosition = posicionOculto;
        }

        // Configurar botones
        botones = new Button[] { btnRenudar, btnReintentar, btnSalir };

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        var teclado = Keyboard.current;
        if (teclado == null) return;

        // Toggle pausa con tecla P
        if (teclado.pKey.wasPressedThisFrame && !animando)
        {
            if (estaPausado)
                CerrarPausa();
            else
                AbrirPausa();
        }

        // Navegación solo cuando está pausado
        if (!puedeNavegar) return;

        if (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame)
        {
            indiceActual = (indiceActual - 1 + botones.Length) % botones.Length;
            ReproducirSonido(sonidoNavegacion);
            ActualizarSeleccionBotones();
        }
        else if (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame)
        {
            indiceActual = (indiceActual + 1) % botones.Length;
            ReproducirSonido(sonidoNavegacion);
            ActualizarSeleccionBotones();
        }

        if (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame)
        {
            ReproducirSonido(sonidoSeleccion);
            SeleccionarOpcion();
        }
    }

    public void AbrirPausa()
    {
        if (estaPausado || animando) return;
        estaPausado = true;        
        animando = true;

        Time.timeScale = 0f;

        // Mutear música del juego
        MutearMusicaJuego(true);

        // Sonido de aparición
        ReproducirSonido(sonidoAparicion);

        StartCoroutine(AnimacionEntrada());
    }

    public void CerrarPausa()
    {
        if (!estaPausado || animando) return;
        animando = true;

        StartCoroutine(AnimacionSalida());
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
            panelPausa.localScale = Vector3.Lerp(escalaInicial, Vector3.one, t);
            yield return null;
        }

        // Fase 2: Bajar (posición oculto -> posición visible)
        tiempo = 0f;
        while (tiempo < duracionAnimacion * 0.5f)
        {
            tiempo += Time.unscaledDeltaTime;  
            float t = tiempo / (duracionAnimacion * 0.5f);
            panelPausa.anchoredPosition = Vector2.Lerp(posInicial, posicionVisible, t);
            yield return null;
        }

        animando = false;
        puedeNavegar = true;
        indiceActual = 0;
        ActualizarSeleccionBotones();
    }

    IEnumerator AnimacionSalida()
    {
        float tiempo = 0f;
        Vector2 posFinal = posicionOculto;
        Vector2 posInicial = posicionVisible;

        // Fase 1: Subir (posición visible -> posición oculto)
        while (tiempo < duracionAnimacion * 0.5f)
        {
            tiempo += Time.unscaledDeltaTime;  // ← CAMBIO AQUÍ
            float t = tiempo / (duracionAnimacion * 0.5f);
            panelPausa.anchoredPosition = Vector2.Lerp(posInicial, posFinal, t);
            yield return null;
        }

        // Fase 2: Desaparecer (scale 1 -> 0)
        tiempo = 0f;
        while (tiempo < duracionAnimacion * 0.5f)
        {
            tiempo += Time.unscaledDeltaTime;  // ← CAMBIO AQUÍ
            float t = tiempo / (duracionAnimacion * 0.5f);
            panelPausa.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            yield return null;
        }

        // Detener animación de botones
        if (coroutinePulse != null)
        {
            StopCoroutine(coroutinePulse);
            coroutinePulse = null;
        }

        // Resetear botones
        for (int i = 0; i < botones.Length; i++)
        {
            if (botones[i] != null)
                botones[i].transform.localScale = Vector3.one;
        }

        animando = false;
        puedeNavegar = false;
        estaPausado = false;

        Time.timeScale = 1f;

        // Desmutear música
        MutearMusicaJuego(false);
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

        while (puedeNavegar && estaPausado)
        {
            tiempo += Time.unscaledDeltaTime * velocidadPulse;  // ← CAMBIO AQUÍ
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
            case 0: Renudar(); break;
            case 1: Reintentar(); break;
            case 2: SalirAlMenu(); break;
        }
    }

    public void Renudar()
    {
        CerrarPausa();
    }

    public void Reintentar()
    {
        Time.timeScale = 1f;
        MutearMusicaJuego(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        MutearMusicaJuego(false);
        SceneManager.LoadScene("MenuPrincipal");
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