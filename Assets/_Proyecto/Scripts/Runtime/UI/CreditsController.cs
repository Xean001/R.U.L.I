using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    [Header("Botón Volver al Menú")]
    public Button btnVolverMenu;

    [Header("Escenas")]
    public string escenaMenuPrincipal = "MenuPrincipal";

    [Header("Audio")]
    public AudioClip sonidoSeleccion;
    private AudioSource audioSource;

    [Header("Animación de Botón")]
    public float escalaMinima = 1f;
    public float escalaMaxima = 1.15f;
    public float velocidadPulse = 3f;

    private bool puedeNavegar = true;
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
        if (btnVolverMenu != null)
            btnVolverMenu.onClick.AddListener(VolverAlMenuPorClick);

        // Iniciar animación del botón
        if (btnVolverMenu != null)
        {
            escalaOriginal = btnVolverMenu.transform.localScale;
            coroutinePulse = StartCoroutine(AnimacionPulseBoton(btnVolverMenu));
        }
    }

    private void VolverAlMenuPorClick()
    {
        if (!puedeNavegar) return;

        ReproducirSonido(sonidoSeleccion);
        VolverAlMenu();
    }

    private void Update()
    {
        if (!puedeNavegar) return;


        // Selección con Enter o Espacio
        if (RuliInput.SubmitPresionado() || RuliInput.CancelPresionado())
        {
            ReproducirSonido(sonidoSeleccion);
            VolverAlMenu();
        }
    }

    private IEnumerator AnimacionPulseBoton(Button boton)
    {
        float tiempo = 0f;

        while (puedeNavegar)
        {
            tiempo += Time.deltaTime * velocidadPulse;
            float escala = escalaMinima + (escalaMaxima - escalaMinima) * (Mathf.Sin(tiempo) * 0.5f + 0.5f);
            boton.transform.localScale = escalaOriginal * escala;
            yield return null;
        }

        if (boton != null)
            boton.transform.localScale = escalaOriginal;
    }

    private void VolverAlMenu()
    {
        puedeNavegar = false;

        // Detener animación
        if (coroutinePulse != null)
        {
            StopCoroutine(coroutinePulse);
            coroutinePulse = null;
        }

        SceneManager.LoadScene(escenaMenuPrincipal);
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
