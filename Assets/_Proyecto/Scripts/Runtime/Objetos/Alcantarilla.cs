using UnityEngine;
using System.Collections;

// Alcantarilla del Nivel 5. No es plataforma (sin collider que bloquee). Cada
// "intervalo" segundos hace salir el liquido (lo activa para que reproduzca su
// animacion y quede peligroso "duracionLiquido" segundos) y luego lo oculta.
public class Alcantarilla : MonoBehaviour
{
    [Tooltip("Objeto hijo del liquido (con su Animator + collider mortal).")]
    public GameObject liquido;

    [Tooltip("Cada cuantos segundos sale el liquido.")]
    public float intervalo = 5f;

    [Tooltip("Cuanto tiempo permanece el liquido fuera (y mortal) por salida.")]
    public float duracionLiquido = 1f;

    void Start()
    {
        if (liquido != null) liquido.SetActive(false);
        StartCoroutine(Ciclo());
    }

    IEnumerator Ciclo()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalo);

            if (liquido != null)
            {
                liquido.SetActive(true);            // al activarse, su Animator reproduce la salida
                yield return new WaitForSeconds(duracionLiquido);
                liquido.SetActive(false);
            }
        }
    }
}
