using UnityEngine;
using UnityEngine.UI;

public class EquiparArma : MonoBehaviour
{
    [Header("Armas en escena (opcional)")]
    public GameObject armaTornado;
    public GameObject armaSoplador;

    [Header("Rueda de habilidades")]
    [Tooltip("Panel que contiene la rueda. Sus hijos deben llamarse SlotPunos, SlotTornado, SlotSoplador y Highlight.")]
    public GameObject panelRueda;

    [Header("Colores de seleccion")]
    public Color colorNormal     = Color.white;
    public Color colorBloqueado  = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color colorSeleccion  = new Color(0.2f, 1f, 0.6f, 1f);

    // 0 = puños (sin arma), 1 = tornado (tienda), 2 = arma (se gana en Nivel1)
    public int indiceArmaActiva = 0;

    private Image[] slots;
    private Image highlight;
    private int seleccion;
    private bool ruedaAbierta;
    private const int TOTAL = 3;

    void Awake()
    {
        // Si no hay panel asignado en la escena, lo crea desde Resources.
        // Asi la rueda funciona en cualquier nivel con solo poner EquiparArma en Ruli.
        if (panelRueda == null)
        {
            GameObject prefab = Resources.Load<GameObject>("RuedaHabilidades");
            if (prefab != null)
            {
                panelRueda = Instantiate(prefab);
                panelRueda.name = "PanelRuedaHabilidades";
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                    panelRueda.transform.SetParent(canvas.transform, false); // preserva layout UI
            }
        }

        if (panelRueda != null)
        {
            slots = new Image[TOTAL];
            slots[0] = BuscarImagen("SlotPunos");
            slots[1] = BuscarImagen("SlotTornado");
            slots[2] = BuscarImagen("SlotSoplador");
            highlight = BuscarImagen("Highlight");
            panelRueda.SetActive(false);
        }
        RuliInput.RuedaAbierta = false;
    }

    private Image BuscarImagen(string nombre)
    {
        Transform t = panelRueda.transform.Find(nombre);
        return t != null ? t.GetComponent<Image>() : null;
    }

    void Update()
    {
        if (RuliInput.RuedaHabilidadesPresionada())
        {
            if (ruedaAbierta) CerrarRueda(confirmar: true);
            else AbrirRueda();
            return;
        }

        if (!ruedaAbierta) return;

        if (RuliInput.MenuIzquierdaPresionado())      Navegar(-1);
        else if (RuliInput.MenuDerechaPresionado())   Navegar(1);

        if (RuliInput.SubmitPresionado())             CerrarRueda(confirmar: true);
        else if (RuliInput.CancelPresionado())        CerrarRueda(confirmar: false);
    }

    private void AbrirRueda()
    {
        if (panelRueda == null) return;
        ruedaAbierta = true;
        RuliInput.RuedaAbierta = true;
        seleccion = indiceArmaActiva;
        Time.timeScale = 0f;
        panelRueda.SetActive(true);
        Refrescar();
    }

    private void CerrarRueda(bool confirmar)
    {
        ruedaAbierta = false;
        RuliInput.RuedaAbierta = false;
        Time.timeScale = 1f;

        if (confirmar && Disponible(seleccion))
            ActivarArmaIndice(seleccion);

        if (panelRueda != null) panelRueda.SetActive(false);
    }

    private void Navegar(int delta)
    {
        seleccion = (seleccion + delta + TOTAL) % TOTAL;
        Refrescar();
    }

    private void Refrescar()
    {
        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].color = (i == seleccion) ? colorSeleccion
                               : (Disponible(i) ? colorNormal : colorBloqueado);
            }
        }

        if (highlight != null && slots != null && slots[seleccion] != null)
            highlight.rectTransform.position = slots[seleccion].rectTransform.position;
    }

    private bool Disponible(int indice)
    {
        if (indice == 0) return true;
        // Tornado: se compra en la tienda. Arma: se gana al derrotar al jefe de Nivel1.
        if (indice == 1) return PlayerPrefs.GetInt("Arma2Comprada", 0) == 1;
        return PlayerPrefs.GetInt("ArmaConseguida", 0) == 1;
    }

    public void ActivarArmaIndice(int indice)
    {
        indiceArmaActiva = indice;

        if (armaTornado != null)  armaTornado.SetActive(false);
        if (armaSoplador != null) armaSoplador.SetActive(false);

        if (indice == 1 && Disponible(1) && armaTornado != null)
            armaTornado.SetActive(true);
        else if (indice == 2 && Disponible(2) && armaSoplador != null)
            armaSoplador.SetActive(true);
    }
}
