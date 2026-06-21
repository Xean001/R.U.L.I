using UnityEngine;
using UnityEngine.InputSystem;

public class EquiparArma : MonoBehaviour
{
    public GameObject armaTornado;
    public GameObject armaSoplador;

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (PlayerPrefs.GetInt("Arma2Comprada", 0) == 1)
            {
                armaTornado.SetActive(!armaTornado.activeSelf);
            }
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (PlayerPrefs.GetInt("Arma1Comprada", 0) == 1)
            {
                armaSoplador.SetActive(!armaSoplador.activeSelf);
            }
        }
    }
}
