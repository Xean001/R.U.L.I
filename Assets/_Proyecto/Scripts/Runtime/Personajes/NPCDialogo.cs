using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Dialogo de NPC: cuando Ruli se acerca por primera vez, muestra sus lineas
// en una franja negra en la parte inferior de la pantalla. Cada linea avanza
// al presionar cualquier tecla (o sola tras 'tiempoPorLinea'). Solo una vez.
public class NPCDialogo : MonoBehaviour
{
    [Header("Dialogo")]
    [TextArea]
    public string[] lineas =
    {
        "El Mall se ha convertido en un lugar irreconocible...",
        "Por favor, ayuda a limpiar todo este sitio que alguna vez fue tan visitado."
    };

    [Header("Activacion")]
    [Tooltip("Si esta apagado, el dialogo no salta por cercania: otro script debe llamar a Mostrar().")]
    public bool activarPorDistancia = true;
    public float radioActivacion = 2.5f;
    public bool congelarJugador = true;

    // true mientras el dialogo esta en pantalla
    public bool Mostrando { get; private set; }

    [Header("Presentacion")]
    public float tiempoPorLinea = 4f;      // avance automatico si no se presiona nada
    public float tamanoLetra = 30f;
    public TMP_FontAsset fuente;           // opcional (ej. PressStart2P); si no, la de TMP

    private bool mostrado;
    private Transform ruli;
    private RuliMovimiento mov;

    void Start()
    {
        mov = FindFirstObjectByType<RuliMovimiento>();
        if (mov != null) ruli = mov.transform;
    }

    void Update()
    {
        if (!activarPorDistancia || mostrado || ruli == null) return;

        if (Vector2.Distance(transform.position, ruli.position) <= radioActivacion)
            Mostrar();
    }

    // Dispara el dialogo desde otro script (ej. la entrada del jefe)
    public void Mostrar()
    {
        if (mostrado) return;
        mostrado = true;
        StartCoroutine(MostrarDialogo());
    }

    IEnumerator MostrarDialogo()
    {
        Mostrando = true;
        // Congelar a Ruli durante la conversacion
        Rigidbody2D rbRuli = null;
        if (congelarJugador && mov != null)
        {
            rbRuli = mov.GetComponent<Rigidbody2D>();
            if (rbRuli != null) rbRuli.linearVelocity = new Vector2(0f, rbRuli.linearVelocity.y);
            mov.enabled = false;
        }

        // --- UI creada al vuelo: franja negra abajo + texto ---
        var canvasGO = new GameObject("CanvasDialogo");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var escalador = canvasGO.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920f, 1080f);

        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panel = panelGO.AddComponent<Image>();
        panel.color = new Color(0f, 0f, 0f, 0.85f);
        var rtPanel = panel.rectTransform;
        rtPanel.anchorMin = new Vector2(0f, 0f);
        rtPanel.anchorMax = new Vector2(1f, 0.22f);   // franja inferior
        rtPanel.offsetMin = Vector2.zero;
        rtPanel.offsetMax = Vector2.zero;

        var textoGO = new GameObject("Texto");
        textoGO.transform.SetParent(panelGO.transform, false);
        var texto = textoGO.AddComponent<TextMeshProUGUI>();
        if (fuente != null) texto.font = fuente;
        texto.fontSize = tamanoLetra;
        texto.color = Color.white;
        texto.alignment = TextAlignmentOptions.Center;
        var rtTexto = texto.rectTransform;
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = new Vector2(40f, 15f);
        rtTexto.offsetMax = new Vector2(-40f, -15f);

        yield return null;   // deja pasar la tecla que pudiera venir presionada

        foreach (string linea in lineas)
        {
            texto.text = linea;

            float t = 0f;
            while (t < tiempoPorLinea)
            {
                t += Time.deltaTime;
                var teclado = Keyboard.current;
                if (teclado != null && teclado.anyKey.wasPressedThisFrame) break;
                var mando = Gamepad.current;
                if (mando != null && mando.buttonSouth.wasPressedThisFrame) break;
                yield return null;
            }
            yield return null;   // evita que una misma pulsacion salte dos lineas
        }

        Destroy(canvasGO);
        if (congelarJugador && mov != null) mov.enabled = true;
        Mostrando = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioActivacion);
    }
}
