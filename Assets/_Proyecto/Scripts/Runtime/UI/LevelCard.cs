using UnityEngine;
using UnityEngine.UI;

public class LevelCard : MonoBehaviour
{
    [Header("Nivel")]
    public int numeroNivel = 1;
    public string nombreEscena = "Nivel1";
    public bool desbloqueadoPorDefecto = false;

    [Header("Imágenes")]
    public Image imagenPreview;
    public Sprite spriteDesbloqueado;
    public Sprite spriteBloqueado;

    [Header("Efecto de selección")]
    public Image imagenEfectoSeleccion;
    public Vector2 offsetEfecto = Vector2.zero;

    private bool estaDesbloqueado;
    private Vector3 escalaNormal;

    private void Awake()
    {
        escalaNormal = transform.localScale;

        string key = "Nivel" + numeroNivel + "Desbloqueado";
        estaDesbloqueado = desbloqueadoPorDefecto || PlayerPrefs.GetInt(key, 0) == 1;

        if (imagenEfectoSeleccion != null)
            imagenEfectoSeleccion.gameObject.SetActive(false);

        ActualizarVisual();
    }

    public bool EstaDesbloqueado() => estaDesbloqueado;
    public string GetNombreEscena() => nombreEscena;

    public void Seleccionar()
    {
        if (imagenEfectoSeleccion != null)
        {
            imagenEfectoSeleccion.gameObject.SetActive(true);
            imagenEfectoSeleccion.rectTransform.anchoredPosition = offsetEfecto;
        }
    }

    public void Deseleccionar()
    {
        if (imagenEfectoSeleccion != null)
            imagenEfectoSeleccion.gameObject.SetActive(false);
    }

    private void ActualizarVisual()
    {
        if (imagenPreview == null) return;

        if (estaDesbloqueado)
        {
            imagenPreview.sprite = spriteDesbloqueado;
            //imagenPreview.color = Color.white;
        }
        else
        {
            imagenPreview.sprite = spriteBloqueado;
            //imagenPreview.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
    }
}
