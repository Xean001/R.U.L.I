using UnityEngine;

public class MonedasManager : MonoBehaviour
{
    public static MonedasManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int ObtenerMonedas()
    {
        return PlayerPrefs.GetInt("Monedas", 0);
    }

    public void AgregarMoneda(int cantidad)
    {
        int monedas = PlayerPrefs.GetInt("Monedas", 0);

        monedas += cantidad;

        PlayerPrefs.SetInt("Monedas", monedas);
        PlayerPrefs.Save();
    }
}