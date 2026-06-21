using TMPro;
using UnityEngine;

public class MostrarMonedas : MonoBehaviour
{
    private TextMeshProUGUI texto;

    private void Awake()
    {
        texto = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        texto.text = PlayerPrefs.GetInt("Monedas", 0).ToString();
    }
}