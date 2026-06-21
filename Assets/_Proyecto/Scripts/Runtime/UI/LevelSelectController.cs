using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelSelectController : MonoBehaviour
{
    [Header("Cartas de nivel")]
    public LevelCard[] cartas;

    [Header("Escenas")]
    public string escenaMenuPrincipal = "MenuPrincipal";

    private int indiceActual = 0;
    private bool puedeNavegar = true;

    private void Start()
    {
        // Empezar siempre en el primer nivel desbloqueado
        indiceActual = PrimerDesbloqueado();
        ActualizarSeleccion();
    }

    private void Update()
    {
        if (!puedeNavegar) return;

        var teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.leftArrowKey.wasPressedThisFrame || teclado.aKey.wasPressedThisFrame)
        {
            int nuevo = BuscarAnteriorDesbloqueado(indiceActual);
            if (nuevo != indiceActual) { indiceActual = nuevo; ActualizarSeleccion(); }
        }
        else if (teclado.rightArrowKey.wasPressedThisFrame || teclado.dKey.wasPressedThisFrame)
        {
            int nuevo = BuscarSiguienteDesbloqueado(indiceActual);
            if (nuevo != indiceActual) { indiceActual = nuevo; ActualizarSeleccion(); }
        }

        if (teclado.enterKey.wasPressedThisFrame || teclado.spaceKey.wasPressedThisFrame)
        {
            SeleccionarNivel();
        }

        if (teclado.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(escenaMenuPrincipal);
        }
    }

    private void ActualizarSeleccion()
    {
        for (int i = 0; i < cartas.Length; i++)
        {
            if (i == indiceActual)
                cartas[i].Seleccionar();
            else
                cartas[i].Deseleccionar();
        }
    }

    private void SeleccionarNivel()
    {
        LevelCard carta = cartas[indiceActual];
        if (!carta.EstaDesbloqueado()) return;

        puedeNavegar = false;
        SceneManager.LoadScene(carta.GetNombreEscena());
    }

    // Devuelve el índice del primer nivel desbloqueado
    private int PrimerDesbloqueado()
    {
        for (int i = 0; i < cartas.Length; i++)
            if (cartas[i].EstaDesbloqueado()) return i;
        return 0;
    }

    // Busca el siguiente desbloqueado hacia la derecha (sin cruzar bloqueados)
    private int BuscarSiguienteDesbloqueado(int desde)
    {
        for (int i = desde + 1; i < cartas.Length; i++)
            if (cartas[i].EstaDesbloqueado()) return i;
        return desde; // ya está en el último desbloqueado, no avanza
    }

    // Busca el anterior desbloqueado hacia la izquierda
    private int BuscarAnteriorDesbloqueado(int desde)
    {
        for (int i = desde - 1; i >= 0; i--)
            if (cartas[i].EstaDesbloqueado()) return i;
        return desde; // ya está en el primero, no retrocede
    }
}
