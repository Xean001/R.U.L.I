using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public TextMeshProUGUI[] menuItems;
    public TextMeshProUGUI arrowIndicator;

    public string sceneJugar = "Nivel1";
    public string sceneNiveles = "Niveles";
    public string sceneCreditos = "Creditos";

    [Header("Flecha")]
    public float velocidadParpadeo = 0.4f;

    private int indiceActual = 0;
    private bool puedeNavegar = true;
    private bool flechaVisible = true;
    private Coroutine coroutineParpadeo;

    private void Start()
    {
        ActualizarSeleccion();
        coroutineParpadeo = StartCoroutine(ParpadearFlecha());
    }

    private void Update()
    {
        if (!puedeNavegar) return;

        var teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame)
        {
            indiceActual = (indiceActual - 1 + menuItems.Length) % menuItems.Length;
            ActualizarSeleccion();
        }
        else if (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame)
        {
            indiceActual = (indiceActual + 1) % menuItems.Length;
            ActualizarSeleccion();
        }

        if (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame)
        {
            SeleccionarOpcion();
        }
    }

    private void ActualizarSeleccion()
    {
        PosicionarFlecha();
    }

    private void PosicionarFlecha()
    {
        if (arrowIndicator == null || menuItems.Length == 0) return;

        RectTransform rectItem = menuItems[indiceActual].GetComponent<RectTransform>();
        RectTransform rectArrow = arrowIndicator.GetComponent<RectTransform>();

        rectArrow.anchoredPosition = new Vector2(600, rectItem.anchoredPosition.y);
    }

    private IEnumerator ParpadearFlecha()
    {
        while (true)
        {
            flechaVisible = !flechaVisible;
            if (arrowIndicator != null)
                arrowIndicator.enabled = flechaVisible;
            yield return new WaitForSeconds(velocidadParpadeo);
        }
    }

    private void SeleccionarOpcion()
    {
        puedeNavegar = false;
        if (coroutineParpadeo != null) StopCoroutine(coroutineParpadeo);
        if (arrowIndicator != null) arrowIndicator.enabled = true;

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
        yield return new WaitForSeconds(0.1f);
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
