using UnityEngine;

public class EquipamientoManager : MonoBehaviour
{
    public GameObject iconoTornado;
    public GameObject iconoSoplador;

    private void Start()
    {
        iconoTornado.SetActive(false);
        iconoSoplador.SetActive(false);

        if (PlayerPrefs.GetInt("Arma2Comprada", 0) == 1)
        {
            iconoTornado.SetActive(true);
        }

        if (PlayerPrefs.GetInt("Arma1Comprada", 0) == 1)
        {
            iconoSoplador.SetActive(true);
        }
    }
}