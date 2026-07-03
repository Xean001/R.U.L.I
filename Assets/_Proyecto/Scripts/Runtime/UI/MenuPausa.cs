using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private bool pausado = false;

    void Update()
    {
        if (RuliInput.PausaPresionada() || (pausado && RuliInput.CancelPresionado()))
        {
            if (pausado)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        pausado = true;
    }

    public void Reanudar()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        pausado = false;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SalirAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
}
