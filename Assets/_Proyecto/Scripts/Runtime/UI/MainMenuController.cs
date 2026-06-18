using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Botones del Menú")]
    public Button[] botones;

    [Header("Escenas")]
    public string sceneJugar = "Nivel1";
    public string sceneNiveles = "Niveles";
    public string sceneCreditos = "Creditos";

    [Header("Animación de Botón")]
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
        // Guardar escala original de todos los botones
        foreach (var boton in botones)
        {
            if (boton != null)
            {
                boton.transform.localScale = Vector3.one;
            }
        }

        // Iniciar en el primer botón
        indiceActual = 0;
        ActualizarSeleccion();
    }

    private void Update()
    {
        if (!puedeNavegar) return;

        var teclado = Keyboard.current;
        if (teclado == null) return;

        // Navegación hacia arriba
        if (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame)
        {
            indiceActual = (indiceActual - 1 + botones.Length) % botones.Length;
            ReproducirSonidoNavegacion();
            ActualizarSeleccion();
        }
        // Navegación hacia abajo
        else if (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame)
        {
            indiceActual = (indiceActual + 1) % botones.Length;
            ReproducirSonidoNavegacion();
            ActualizarSeleccion();
        }

        // Selección
        if (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame)
        {
            SeleccionarOpcion();
        }
    }

    private void ActualizarSeleccion()
    {
        // Detener animación del botón anterior
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

        // Iniciar animación en el botón seleccionado
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

            // Calcular escala usando función sinusoidal
            float escala = escalaMinima + (escalaMaxima - escalaMinima) * (Mathf.Sin(tiempo) * 0.5f + 0.5f);

            // Aplicar escala manteniendo la proporción original
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

        // Detener animación
        if (coroutinePulse != null)
        {
            StopCoroutine(coroutinePulse);
            coroutinePulse = null;
        }

        // Efecto visual de confirmación (escala máxima momentánea)
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
        }
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