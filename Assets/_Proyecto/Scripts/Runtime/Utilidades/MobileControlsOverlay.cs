using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MobileControlsOverlay : MonoBehaviour
{
    private const int SortingOrder = 3000;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded += (_, __) => CrearSiCorresponde();
        CrearSiCorresponde();
    }

    private static void CrearSiCorresponde()
    {
        if (!DebeMostrarControles()) return;
        if (!HayJugadorEnEscena()) return;
        if (FindAnyObjectByType<MobileControlsOverlay>() != null) return;

        new GameObject("MobileControlsOverlay").AddComponent<MobileControlsOverlay>();
    }

    private static bool DebeMostrarControles()
    {
        return Application.isMobilePlatform;
    }

    private static bool HayJugadorEnEscena()
    {
        return FindAnyObjectByType<RuliMovimiento>() != null ||
               FindAnyObjectByType<ruliprueba>() != null;
    }

    private IEnumerator Start()
    {
        yield return null;

        if (!HayJugadorEnEscena())
        {
            Destroy(gameObject);
            yield break;
        }

        UiInputBootstrap.EnsureEventSystem(true);
        ConstruirCanvas();
    }

    private void OnDestroy()
    {
        RuliInput.ResetMobileState();
    }

    private void ConstruirCanvas()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        CrearBoton("MobileLeft", "<", new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(130f, 135f), new Vector2(150f, 150f), MobileControlAction.Left);
        CrearBoton("MobileRight", ">", new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(315f, 135f), new Vector2(150f, 150f), MobileControlAction.Right);
        CrearBoton("MobileJump", "JUMP", new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-330f, 145f), new Vector2(165f, 165f), MobileControlAction.Jump);
        CrearBoton("MobileAttack", "ATK", new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-140f, 145f), new Vector2(165f, 165f), MobileControlAction.Attack);
        if (FindAnyObjectByType<PausaController>() != null)
        {
            CrearBoton("MobilePause", "II", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-92f, -82f), new Vector2(110f, 92f), MobileControlAction.Pause);
        }
    }

    private void CrearBoton(string nombre, string texto, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 posicion, Vector2 tamano, MobileControlAction accion)
    {
        GameObject boton = new GameObject(nombre);
        boton.transform.SetParent(transform, false);

        RectTransform rect = boton.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;

        Image fondo = boton.AddComponent<Image>();
        fondo.color = new Color(0f, 0f, 0f, 0.42f);

        Button button = boton.AddComponent<Button>();
        button.targetGraphic = fondo;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0.72f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.86f);
        colors.pressedColor = new Color(0.7f, 0.9f, 1f, 0.9f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.25f);
        button.colors = colors;

        MobileControlButton control = boton.AddComponent<MobileControlButton>();
        control.accion = accion;

        GameObject label = new GameObject("Label");
        label.transform.SetParent(boton.transform, false);

        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text text = label.AddComponent<Text>();
        text.text = texto;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = accion == MobileControlAction.Pause ? 34 : 36;
        text.fontStyle = FontStyle.Bold;
        text.color = new Color(1f, 1f, 1f, 0.92f);
        text.raycastTarget = false;
    }
}

public enum MobileControlAction
{
    Left,
    Right,
    Jump,
    Attack,
    Pause
}

public class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public MobileControlAction accion;

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (accion)
        {
            case MobileControlAction.Left:
                RuliInput.SetMobileMove(-1, true);
                break;
            case MobileControlAction.Right:
                RuliInput.SetMobileMove(1, true);
                break;
            case MobileControlAction.Jump:
                RuliInput.MobileJumpDown();
                break;
            case MobileControlAction.Attack:
                RuliInput.MobileAttackDown();
                break;
            case MobileControlAction.Pause:
                RuliInput.MobilePauseDown();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        LiberarMovimiento();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        LiberarMovimiento();
    }

    private void LiberarMovimiento()
    {
        if (accion == MobileControlAction.Left)
            RuliInput.SetMobileMove(-1, false);
        else if (accion == MobileControlAction.Right)
            RuliInput.SetMobileMove(1, false);
    }
}
