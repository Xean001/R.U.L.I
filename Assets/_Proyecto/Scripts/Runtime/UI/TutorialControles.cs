using UnityEngine;

// Muestra los carteles del tutorial de uno en uno segun avanza el jugador.
// Cambia las indicaciones segun el ultimo tipo de control usado.
public class TutorialControles : MonoBehaviour
{
    private class CartelTutorial
    {
        public TextMesh titulo;
        public TextMesh detalle;
    }

    [System.Serializable]
    public class MensajesTutorial
    {
        public string mover = "MOVER\nA/D";
        public string saltar = "SALTAR\nESPACIO";
        public string pausa = "PAUSA\nESC / P";
        public string atacar = "ATACAR\nF o CLICK";
    }

    public Transform jugador;
    public GameObject wasd;
    public GameObject saltar;
    public GameObject pausa;
    public GameObject atacar;

    [Header("Diseno de carteles")]
    public Color colorTitulo = new Color(1f, 0.95f, 0.74f, 1f);
    public Color colorDetalle = Color.white;
    public Color colorFondo = new Color(0.08f, 0.12f, 0.16f, 0.94f);
    public Color colorBorde = new Color(0.93f, 0.74f, 0.29f, 1f);
    public Color colorAcento = new Color(0.18f, 0.74f, 0.86f, 1f);
    public Color colorSombra = new Color(0f, 0f, 0f, 0.36f);
    public Vector2 tamanoCartel = new Vector2(3.8f, 1.35f);

    [Header("Umbrales en X (el jugador avanza a la derecha)")]
    public float xHastaWasd   = -42f;  // x <  esto         -> WASD
    public float xHastaSaltar = -26f;  // entre los dos     -> SALTAR
    public float xHastaFin    = -10f;  // entre/luego       -> ATACAR / nada

    private CartelTutorial cartelWasd;
    private CartelTutorial cartelSaltar;
    private CartelTutorial cartelPausa;
    private CartelTutorial cartelAtacar;
    private RuliTipoControl tipoControlMostrado;
    private static Sprite spriteBlanco;
    private const float TamanoTituloReal = 0.035f;
    private const float TamanoDetalleReal = 0.0275f;

    void Awake()
    {
        if (pausa == null)
        {
            Transform t = transform.Find("Cartel_PAUSA");
            if (t != null) pausa = t.gameObject;
        }

        cartelWasd = CrearCartel(wasd);
        cartelSaltar = CrearCartel(saltar);
        cartelPausa = CrearCartel(pausa);
        cartelAtacar = CrearCartel(atacar);
        tipoControlMostrado = (RuliTipoControl)(-1);
        ActualizarMensajes(true);
    }

    void Update()
    {
        if (jugador == null) return;

        ActualizarMensajes(false);

        float x = jugador.position.x;
        Set(wasd,   x <  xHastaWasd);
        Set(saltar, x >= xHastaWasd   && x < xHastaSaltar);
        Set(pausa,  x >= xHastaWasd   && x < xHastaSaltar);
        Set(atacar, x >= xHastaSaltar && x < xHastaFin);
    }

    void Set(GameObject g, bool visible)
    {
        if (g != null && g.activeSelf != visible) g.SetActive(visible);
    }

    private CartelTutorial CrearCartel(GameObject cartel)
    {
        if (cartel == null) return null;

        SpriteRenderer cartelRenderer = cartel.GetComponent<SpriteRenderer>();
        if (cartelRenderer != null)
            cartelRenderer.enabled = false;

        GameObject go = new GameObject("CartelDinamicoControl");
        go.transform.SetParent(cartel.transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = EscalaCompensada(cartel.transform);

        int sortingLayer = cartelRenderer != null ? cartelRenderer.sortingLayerID : 0;
        int sortingBase = cartelRenderer != null ? cartelRenderer.sortingOrder : 0;

        CrearRect(go.transform, "Sombra", new Vector2(0.12f, -0.12f), tamanoCartel, colorSombra, sortingLayer, sortingBase + 1);
        CrearRect(go.transform, "Borde", Vector2.zero, tamanoCartel, colorBorde, sortingLayer, sortingBase + 2);
        CrearRect(go.transform, "Fondo", Vector2.zero, tamanoCartel - new Vector2(0.12f, 0.12f), colorFondo, sortingLayer, sortingBase + 3);
        CrearRect(go.transform, "Acento", new Vector2(0f, (tamanoCartel.y * 0.5f) - 0.16f),
            new Vector2(tamanoCartel.x - 0.28f, 0.08f), colorAcento, sortingLayer, sortingBase + 4);

        CartelTutorial vista = new CartelTutorial();
        vista.titulo = CrearLineaTexto(go.transform, "Titulo", new Vector2(0f, 0.26f), TamanoTituloReal, colorTitulo, sortingLayer, sortingBase + 5);
        vista.detalle = CrearLineaTexto(go.transform, "Detalle", new Vector2(0f, -0.24f), TamanoDetalleReal, colorDetalle, sortingLayer, sortingBase + 5);
        return vista;
    }

    private Vector3 EscalaCompensada(Transform cartel)
    {
        Vector3 escala = cartel.localScale;
        float x = Mathf.Abs(escala.x) > 0.001f ? 1f / Mathf.Abs(escala.x) : 1f;
        float y = Mathf.Abs(escala.y) > 0.001f ? 1f / Mathf.Abs(escala.y) : 1f;
        return new Vector3(x, y, 1f);
    }

    private void CrearRect(Transform parent, string nombre, Vector2 posicion, Vector2 tamano, Color color,
        int sortingLayer, int sortingOrder)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(posicion.x, posicion.y, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = new Vector3(tamano.x, tamano.y, 1f);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteBlanco();
        renderer.color = color;
        renderer.sortingLayerID = sortingLayer;
        renderer.sortingOrder = sortingOrder;
    }

    private TextMesh CrearLineaTexto(Transform parent, string nombre, Vector2 posicion, float tamano, Color color,
        int sortingLayer, int sortingOrder)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(posicion.x, posicion.y, -0.01f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        TextMesh texto = go.AddComponent<TextMesh>();
        texto.anchor = TextAnchor.MiddleCenter;
        texto.alignment = TextAlignment.Center;
        texto.color = color;
        texto.characterSize = tamano;
        texto.fontSize = 64;
        texto.text = "";

        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sortingLayerID = sortingLayer;
            renderer.sortingOrder = sortingOrder;
        }

        return texto;
    }

    private static Sprite SpriteBlanco()
    {
        if (spriteBlanco != null) return spriteBlanco;

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        spriteBlanco = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return spriteBlanco;
    }

    private void ActualizarMensajes(bool forzar)
    {
        RuliTipoControl tipo = RuliInput.TipoControlActual;
        if (!forzar && tipo == tipoControlMostrado) return;

        tipoControlMostrado = tipo;
        MensajesTutorial mensajes = MensajesPara(tipo);

        SetMensaje(cartelWasd, mensajes.mover);
        SetMensaje(cartelSaltar, mensajes.saltar);
        SetMensaje(cartelPausa, mensajes.pausa);
        SetMensaje(cartelAtacar, mensajes.atacar);
    }

    private MensajesTutorial MensajesPara(RuliTipoControl tipo)
    {
        if (tipo == RuliTipoControl.Mando)
        {
            return new MensajesTutorial
            {
                mover = "MOVER\nSTICK",
                saltar = "SALTAR\nA",
                pausa = "PAUSA\nSTART",
                atacar = "ATACAR\nX / RT"
            };
        }

        if (tipo == RuliTipoControl.Movil)
        {
            return new MensajesTutorial
            {
                mover = "MOVER\n< / >",
                saltar = "SALTAR\nJUMP",
                pausa = "PAUSA\nII",
                atacar = "ATACAR\nATK"
            };
        }

        return new MensajesTutorial
        {
            mover = "MOVER\nA/D",
            saltar = "SALTAR\nESPACIO",
            pausa = "PAUSA\nESC / P",
            atacar = "ATACAR\nF / CLICK"
        };
    }

    private void SetMensaje(CartelTutorial cartel, string valor)
    {
        if (cartel == null) return;

        string[] partes = valor.Split('\n');
        string titulo = partes.Length > 0 ? partes[0] : valor;
        string detalle = partes.Length > 1 ? partes[1] : "";

        if (cartel.titulo != null && cartel.titulo.text != titulo)
            cartel.titulo.text = titulo;
        if (cartel.detalle != null && cartel.detalle.text != detalle)
            cartel.detalle.text = detalle;
    }
}
