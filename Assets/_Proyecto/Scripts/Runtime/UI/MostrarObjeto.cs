using UnityEngine;

public class MostrarObjeto : MonoBehaviour
{
    public GameObject objetoAMostrar;

    public void Mostrar()
    {
        objetoAMostrar.SetActive(true);
    }
}