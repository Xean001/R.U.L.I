using UnityEngine;

public class TiendaManager : MonoBehaviour
{
    [Header("Precios (hojas)")]
    public int precioSoplador = 5;
    public int precioTornado  = 3;
    public int precioDobleSalto = 5;

    public void ComprarSoplador()
    {
        if (PlayerPrefs.GetInt("Arma1Comprada", 0) == 1)
        {
            Debug.Log("Soplador ya comprado");
            return;
        }

        if (BancoMonedas.Gastar(precioSoplador))
        {
            PlayerPrefs.SetInt("Arma1Comprada", 1);
            PlayerPrefs.Save();
            Debug.Log("Soplador comprado");
        }
        else
        {
            Debug.Log("No tienes suficientes hojas");
        }
    }

    public void ComprarDobleSalto()
    {
        if (PlayerPrefs.GetInt("DobleSaltoComprado", 0) == 1)
        {
            Debug.Log("Doble salto ya comprado");
            return;
        }

        if (BancoMonedas.Gastar(precioDobleSalto))
        {
            PlayerPrefs.SetInt("DobleSaltoComprado", 1);
            PlayerPrefs.Save();
            Debug.Log("Doble salto comprado");
        }
        else
        {
            Debug.Log("No tienes suficientes hojas");
        }
    }

    public void ComprarTornado()
    {
        if (PlayerPrefs.GetInt("Arma2Comprada", 0) == 1)
        {
            Debug.Log("Tornado ya comprado");
            return;
        }

        if (BancoMonedas.Gastar(precioTornado))
        {
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
