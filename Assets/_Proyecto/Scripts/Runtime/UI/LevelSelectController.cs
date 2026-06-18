using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectController : MonoBehaviour
{
    [Header("Cartas de nivel")]
    public LevelCard[] cartas;

    [Header("Botón Volver al Menú")]
    public Button btnVolverMenu;

    [Header("Escenas")]
    public string escenaMenuPrincipal = "MenuPrincipal";

    [Header("Audio")]
    public AudioClip sonidoNavegacion;
    public AudioClip sonidoSeleccion;
    private AudioSource audioSource;

    [Header("Animación de Selección")]
    public float escalaMinima = 1f;
    public float escalaMaxima = 1.15f;
    public float velocidadPulse = 3f;

    private int indiceActual = 0;
    private bool enBotonMenu = false;
    private bool puedeNavegar = true;
    private int ultimoIndiceNivel = 0;
    private Coroutine coroutinePulse;
    private Vector3 escalaOriginal;

    private void Awake()
    {
        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        indiceActual = PrimerDesbloqueado();
        ultimoIndiceNivel = indiceActual;
        enBotonMenu = false;
        ActualizarSeleccion();
    }

    private void Update()
    {
        if (!puedeNavegar) return;
        var teclado = Keyboard.current;
        if (teclado == null) return;

        // Navegación izquierda/derecha (solo en fila de niveles)
        if (!enBotonMenu)
        {
            if (teclado.leftArrowKey.wasPressedThisFrame || teclado.aKey.wasPressedThisFrame)
            {
                int nuevo = BuscarAnteriorDesbloqueado(indiceActual);
                if (nuevo != indiceActual)
                {
                    indiceActual = nuevo;
                    ultimoIndiceNivel = indiceActual;
                    ReproducirSonido(sonidoNavegacion);
                    ActualizarSeleccion();
                }
            }
            else if (teclado.rightArrowKey.wasPressedThisFrame || teclado.dKey.wasPressedThisFrame)
            {
                int nuevo = BuscarSiguienteDesbloqueado(indiceActual);
                if (nuevo != indiceActual)
                {
                    indiceActual = nuevo;
                    ultimoIndiceNivel = indiceActual;
                    ReproducirSonido(sonidoNavegacion);
                    ActualizarSeleccion();
                }
            }

            // Bajar al botón de menú
            if (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame)
            {
                enBotonMenu = true;
                ReproducirSonido(sonidoNavegacion);
                ActualizarSeleccion();
            }
        }
        else
        {
            // Subir desde el botón de menú a los niveles
            if (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame)
            {
                enBotonMenu = false;
                indiceActual = ultimoIndiceNivel;
                ReproducirSonido(sonidoNavegacion);
                ActualizarSeleccion();
            }
        }

        // Selección
        if (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame)
        {
            ReproducirSonido(sonidoSeleccion);
            if (enBotonMenu)
                VolverAlMenu();
            else
                SeleccionarNivel();
        }

        // Escape para volver al menú
        if (teclado.escapeKey.wasPressedThisFrame)
        {
            ReproducirSonido(sonidoSeleccion);
            VolverAlMenu();
        }
    }

    private void ActualizarSeleccion()
    {
        // Detener animación anterior
        if (coroutinePulse != null)
        {
            StopCoroutine(coroutinePulse);
            coroutinePulse = null;
        }

        // Resetear escala de todas las cartas
        for (int i = 0; i < cartas.Length; i++)
        {
            if (cartas[i] != null)
                cartas[i].transform.localScale = Vector3.one;
        }

        // Resetear escala del botón
        if (btnVolverMenu != null)
            btnVolverMenu.transform.localScale = Vector3.one;

        if (enBotonMenu)
        {
            // Deseleccionar todas las cartas
            for (int i = 0; i < cartas.Length; i++)
                cartas[i].Deseleccionar();

            // Animar botón de menú
            if (btnVolverMenu != null)
            {
                escalaOriginal = btnVolverMenu.transform.localScale;
                coroutinePulse = StartCoroutine(AnimacionPulseBoton(btnVolverMenu));
            }
        }
        else
        {
            // Seleccionar carta actual
            for (int i = 0; i < cartas.Length; i++)
            {
                if (i == indiceActual)
                    cartas[i].Seleccionar();
                else
                    cartas[i].Deseleccionar();
            }

            // Animar carta seleccionada
            if (cartas[indiceActual] != null)
            {
                escalaOriginal = cartas[indiceActual].transform.localScale;
                coroutinePulse = StartCoroutine(AnimacionPulseCarta(cartas[indiceActual]));
            }
        }
    }

    private IEnumerator AnimacionPulseCarta(LevelCard carta)
    {
        float tiempo = 0f;

        while (puedeNavegar && !enBotonMenu)
        {
            tiempo += Time.deltaTime * velocidadPulse;
            float escala = escalaMinima + (escalaMaxima - escalaMinima) * (Mathf.Sin(tiempo) * 0.5f + 0.5f);
            carta.transform.localScale = escalaOriginal * escala;
            yield return null;
        }

        if (carta != null)
            carta.transform.localScale = escalaOriginal;
    }

    private IEnumerator AnimacionPulseBoton(Button boton)
    {
        float tiempo = 0f;

        while (puedeNavegar && enBotonMenu)
        {
            tiempo += Time.deltaTime * velocidadPulse;
            float escala = escalaMinima + (escalaMaxima - escalaMinima) * (Mathf.Sin(tiempo) * 0.5f + 0.5f);
            boton.transform.localScale = escalaOriginal * escala;
            yield return null;
        }

        if (boton != null)
            boton.transform.localScale = escalaOriginal;
    }

    private void SeleccionarNivel()
    {
        LevelCard carta = cartas[indiceActual];
        if (!carta.EstaDesbloqueado()) return;
        puedeNavegar = false;
        SceneManager.LoadScene(carta.GetNombreEscena());
    }

    private void VolverAlMenu()
    {
        puedeNavegar = false;
        SceneManager.LoadScene(escenaMenuPrincipal);
    }

    private int PrimerDesbloqueado()
    {
        for (int i = 0; i < cartas.Length; i++)
            if (cartas[i].EstaDesbloqueado()) return i;
        return 0;
    }

    private int BuscarSiguienteDesbloqueado(int desde)
    {
        for (int i = desde + 1; i < cartas.Length; i++)
            if (cartas[i].EstaDesbloqueado()) return i;
        return desde;
    }

    private int BuscarAnteriorDesbloqueado(int desde)
    {
        for (int i = desde - 1; i >= 0; i--)
            if (cartas[i].EstaDesbloqueado()) return i;
        return desde;
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}