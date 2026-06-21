using UnityEngine;

public class TiendaManager : MonoBehaviour
{
    public void ComprarSoplador()
    {
        int monedas = PlayerPrefs.GetInt("Monedas", 0);

        if (monedas >= 5)
        {
            monedas -= 5;

            PlayerPrefs.SetInt("Monedas", monedas);
            PlayerPrefs.SetInt("Arma1Comprada", 1);

            PlayerPrefs.Save();

            Debug.Log("Soplador comprado");
        }
        else
        {
            Debug.Log("No tienes suficientes hojas");
        }
    }

    public void ComprarTornado()
    {
        int monedas = PlayerPrefs.GetInt("Monedas", 0);

        if (monedas >= 3)
        {
            monedas -= 3;

            PlayerPrefs.SetInt("Monedas", monedas);
            PlayerPrefs.SetInt("Arma2Comprada", 1);

            PlayerPrefs.Save();

            Debug.Log("Tornado comprado");
        }
        else
        {
            Debug.Log("No tienes suficientes hojas");
        }
    }
}