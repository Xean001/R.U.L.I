using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Botones del Men�")]
    public Button[] botones;

    [Header("Escenas")]
    public string sceneJugar = "Nivel1";
    public string sceneNiveles = "Niveles";
    public string sceneCreditos = "Creditos";
    public string sceneTienda = "Tienda";

    [Header("Animaci�n de Bot�n")]
    public float escalaMinima = 1f;
    public float escalaMaxima = 1.15f;
    public float velocidadPulse = 3f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoNavegacion;
    public AudioClip sonidoSeleccion;

    private int indiceActual = 0;
    private bool puedeNavegar = true;
    private Coroutine coroutinePulse;
    private Vector3 escalaOriginal;

    private void Start()
    {
        ConfigurarClicks();

        // Guardar escala original de todos los botones
        foreach (var boton in botones)
        {
            if (boton != null)
            {
                boton.transform.localScale = Vector3.one;
            }
        }

        // Iniciar en el primer bot�n
        indiceActual = 0;
        ActualizarSeleccion();
    }

    private void Update()
    {
        if (!puedeNavegar) return;
        if (botones == null || botones.Length == 0) return;


        // Navegaci�n hacia arriba
        if (RuliInput.MenuArribaPresionado())
        {
            indiceActual = (indiceActual - 1 + botones.Length) % botones.Length;
            ReproducirSonidoNavegacion();
            ActualizarSeleccion();
        }
        // Navegaci�n hacia abajo
        else if (RuliInput.MenuAbajoPresionado())
        {
            indiceActual = (indiceActual + 1) % botones.Length;
            ReproducirSonidoNavegacion();
            ActualizarSeleccion();
        }

        // Selecci�n
        if (RuliInput.SubmitPresionado())
        {
            SeleccionarOpcion();
        }
    }

    private void ConfigurarClicks()
    {
        if (botones == null) return;

        for (int i = 0; i < botones.Length; i++)
        {
            Button boton = botones[i];
            if (boton == null) continue;

            int indice = i;
            boton.onClick.AddListener(() => SeleccionarOpcionPorClick(indice));
        }
    }

    private void SeleccionarOpcionPorClick(int indice)
    {
        if (!puedeNavegar) return;
        if (botones == null || indice < 0 || indice >= botones.Length) return;

        indiceActual = indice;
        ActualizarSeleccion();
        SeleccionarOpcion();
    }

    private void ActualizarSeleccion()
    {
        // Detener animaci�n del bot�n anterior
        if (coroutinePulse != null)
        {
            StopCoroutine(coroutinePulse);
            coroutinePulse = null;
        }

        // Resetear escala de todos los botones
        foreach (var boton in botones)
        {
            if (boton != null)
            {
                boton.transform.localScale = Vector3.one;
            }
        }

        // Iniciar animaci�n en el bot�n seleccionado
        if (botones[indiceActual] != null)
        {
            escalaOriginal = botones[indiceActual].transform.localScale;
            coroutinePulse = StartCoroutine(AnimacionPulse(botones[indiceActual]));
        }
    }

    private IEnumerator AnimacionPulse(Button boton)
    {
        float tiempo = 0f;

        while (true)
        {
            tiempo += Time.deltaTime * velocidadPulse;

            // Calcular escala usando funci�n sinusoidal
            float escala = escalaMinima + (escalaMaxima - escalaMinima) * (Mathf.Sin(tiempo) * 0.5f + 0.5f);

            // Aplicar escala manteniendo la proporci�n original
            Vector3 nuevaEscala = escalaOriginal * escala;
            boton.transform.localScale = nuevaEscala;

            yield return null;
        }
    }

    private void ReproducirSonidoNavegacion()
    {
        if (audioSource != null && sonidoNavegacion != null)
        {
            audioSource.PlayOneShot(sonidoNavegacion);
        }
    }

    private void ReproducirSonidoSeleccion()
    {
        if (audioSource != null && sonidoSeleccion != null)
        {
            audioSource.PlayOneShot(sonidoSeleccion);
        }
    }

    private void SeleccionarOpcion()
    {
        puedeNavegar = false;
        ReproducirSonidoSeleccion();

        // Detener animaci�n
        if (coroutinePulse != null)
        {
            StopCoroutine(coroutinePulse);
            coroutinePulse = null;
        }

        // Efecto visual de confirmaci�n (escala m�xima moment�nea)
        if (botones[indiceActual] != null)
        {
            botones[indiceActual].transform.localScale = escalaOriginal * escalaMaxima;
        }

        switch (indiceActual)
        {
            case 0: StartCoroutine(CargarEscena(sceneJugar)); break;
            case 1: StartCoroutine(CargarEscena(sceneNiveles)); break;
            case 2: StartCoroutine(CargarEscena(sceneCreditos)); break;
            case 3: SalirDelJuego(); break;
            case 4: StartCoroutine(CargarEscena(sceneTienda)); break;
        }
    }

    // Acceso directo a la tienda (para boton con onClick propio).
    public void AbrirTienda()
    {
        StartCoroutine(CargarEscena(sceneTienda));
    }

    private IEnumerator CargarEscena(string nombreEscena)
    {
        Debug.Log(">>> Cargando escena: " + nombreEscena + " | indice: " + indiceActual);
        yield return new WaitForSeconds(0.3f); // Esperar para que se vea el efecto
        SceneManager.LoadScene(nombreEscena);
    }

    private void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
