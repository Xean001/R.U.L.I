using UnityEngine;
using System;

public class ContadorMonedas : MonoBehaviour
{
    public static ContadorMonedas Instance { get; private set; }

    public int Monedas { get; private set; }
    public event Action<int> OnMonedasCambiadas;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Agregar(int cantidad = 1)
    {
        Monedas += cantidad;
        OnMonedasCambiadas?.Invoke(Monedas);
    }
}
