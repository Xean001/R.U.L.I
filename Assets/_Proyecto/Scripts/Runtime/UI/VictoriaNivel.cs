using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Muestra la pantalla de VICTORIA, marca el nivel como completado y manda al menu de niveles.
public class VictoriaNivel : MonoBehaviour
{
    public GameObject panelVictoria;
    public int nivelActual = 1;
    public string escenaNiveles = "Niveles";
    public float esperaAntesDelMenu = 3.5f;

    private bool gano;

    void Awake()
    {
        if (panelVictoria != null) panelVictoria.SetActive(false);
    }

    public void Ganar()
    {
        if (gano) return;
        gano = true;

        // Marcar completado + desbloquear el siguiente nivel
        PlayerPrefs.SetInt("Nivel" + nivelActual + "Completado", 1);
        PlayerPrefs.SetInt("Nivel" + nivelActual + "Desbloqueado", 1);
        PlayerPrefs.SetInt("Nivel" + (nivelActual + 1) + "Desbloqueado", 1);
        PlayerPrefs.Save();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        StartCoroutine(IrAlMenu());
    }

    IEnumerator IrAlMenu()
    {
        yield return new WaitForSeconds(esperaAntesDelMenu);
        SceneManager.LoadScene(escenaNiveles);
    }
}
