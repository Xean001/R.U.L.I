using UnityEngine;
using TMPro;

public class MonedasUI : MonoBehaviour
{
    public TextMeshProUGUI texto;

    void Start()
    {
        Actualizar(0);
        var contador = FindFirstObjectByType<ContadorMonedas>();
        if (contador != null)
        {
            contador.OnMonedasCambiadas += Actualizar;
            Actualizar(contador.Monedas);
        }
    }

    void Actualizar(int cantidad) => texto.text = $"x{cantidad}";
}
