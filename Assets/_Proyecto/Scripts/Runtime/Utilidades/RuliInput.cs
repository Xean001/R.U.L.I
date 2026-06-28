using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class RuliInput
{
    private const float MovimientoDeadZone = 0.2f;
    private const float MenuPressThreshold = 0.6f;
    private const float MenuReleaseThreshold = 0.35f;

    private static int ultimoFrameMando = -1;
    private static Vector2 direccionMandoActual;
    private static Vector2 direccionMandoAnterior;
    private static bool mobileIzquierda;
    private static bool mobileDerecha;
    private static bool mobileSaltoPendiente;
    private static bool mobileAtaquePendiente;
    private static bool mobilePausaPendiente;

    public static float MovimientoHorizontal()
    {
        float mobile = HorizontalMobile();
        float teclado = HorizontalTeclado();
        float mando = HorizontalMando();

        if (Mathf.Abs(mobile) > 0f) return mobile;
        return Mathf.Abs(mando) > Mathf.Abs(teclado) ? mando : teclado;
    }

    public static bool SaltoPresionado()
    {
        // El salto es EXCLUSIVAMENTE con espacio (W queda libre para escalar).
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null && teclado.spaceKey.wasPressedThisFrame;

        bool mobilePresionado = ConsumirMobileSalto();
        return tecladoPresionado || mobilePresionado || CualquierMandoPresiono(m => m.buttonSouth);
    }

    // Eje vertical para escalar lianas: W/Arriba = sube (+1), S/Abajo = baja (-1).
    public static float EscalarVertical()
    {
        Keyboard teclado = Keyboard.current;
        float v = 0f;
        if (teclado != null)
        {
            if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed)   v += 1f;
            if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) v -= 1f;
        }

        foreach (Gamepad mando in Gamepad.all)
        {
            Vector2 d = LeerDireccionMando(mando);
            if (Mathf.Abs(d.y) > Mathf.Abs(v)) v = d.y > 0f ? 1f : -1f;
        }

        return Mathf.Clamp(v, -1f, 1f);
    }

    public static bool AtaquePresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null && teclado.fKey.wasPressedThisFrame;

        return tecladoPresionado ||
            ConsumirMobileAtaque() ||
            CualquierMandoPresiono(m => m.buttonWest) ||
            CualquierMandoPresiono(m => m.rightShoulder) ||
            CualquierMandoPresiono(m => m.rightTrigger);
    }

    public static bool RuedaAbierta { get; set; }

    public static bool RuedaHabilidadesPresionada()
    {
        Keyboard teclado = Keyboard.current;
        return teclado != null && teclado.eKey.wasPressedThisFrame;
    }

    public static bool PausaPresionada()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.pKey.wasPressedThisFrame || teclado.escapeKey.wasPressedThisFrame);

        return tecladoPresionado ||
            ConsumirMobilePausa() ||
            CualquierMandoPresiono(m => m.startButton) ||
            CualquierMandoPresiono(m => m.selectButton);
    }

    public static bool SubmitPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.enterKey.wasPressedThisFrame ||
             teclado.numpadEnterKey.wasPressedThisFrame ||
             teclado.spaceKey.wasPressedThisFrame);

        return tecladoPresionado ||
            CualquierMandoPresiono(m => m.buttonSouth) ||
            CualquierMandoPresiono(m => m.startButton);
    }

    public static bool CancelPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.escapeKey.wasPressedThisFrame || teclado.backspaceKey.wasPressedThisFrame);

        return tecladoPresionado ||
            CualquierMandoPresiono(m => m.buttonEast) ||
            CualquierMandoPresiono(m => m.selectButton);
    }

    public static bool MenuArribaPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame);

        return tecladoPresionado ||
            CualquierMandoPresiono(m => m.dpad.up) ||
            EjeMandoPresionado(Vector2.up);
    }

    public static bool MenuAbajoPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame);

        return tecladoPresionado ||
            CualquierMandoPresiono(m => m.dpad.down) ||
            EjeMandoPresionado(Vector2.down);
    }

    public static bool MenuIzquierdaPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.leftArrowKey.wasPressedThisFrame || teclado.aKey.wasPressedThisFrame);

        return tecladoPresionado ||
            CualquierMandoPresiono(m => m.dpad.left) ||
            EjeMandoPresionado(Vector2.left);
    }

    public static bool MenuDerechaPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.rightArrowKey.wasPressedThisFrame || teclado.dKey.wasPressedThisFrame);

        return tecladoPresionado ||
            CualquierMandoPresiono(m => m.dpad.right) ||
            EjeMandoPresionado(Vector2.right);
    }

    public static void SetMobileMove(int direccion, bool presionado)
    {
        if (direccion < 0)
            mobileIzquierda = presionado;
        else if (direccion > 0)
            mobileDerecha = presionado;
    }

    public static void MobileJumpDown()
    {
        mobileSaltoPendiente = true;
    }

    public static void MobileAttackDown()
    {
        mobileAtaquePendiente = true;
    }

    public static void MobilePauseDown()
    {
        mobilePausaPendiente = true;
    }

    public static void ResetMobileState()
    {
        mobileIzquierda = false;
        mobileDerecha = false;
        mobileSaltoPendiente = false;
        mobileAtaquePendiente = false;
        mobilePausaPendiente = false;
    }

    private static float HorizontalTeclado()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return 0f;

        float valor = 0f;
        if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) valor -= 1f;
        if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) valor += 1f;
        return Mathf.Clamp(valor, -1f, 1f);
    }

    private static float HorizontalMobile()
    {
        if (mobileIzquierda == mobileDerecha) return 0f;
        return mobileDerecha ? 1f : -1f;
    }

    private static bool ConsumirMobileSalto()
    {
        if (!mobileSaltoPendiente) return false;
        mobileSaltoPendiente = false;
        return true;
    }

    private static bool ConsumirMobileAtaque()
    {
        if (!mobileAtaquePendiente) return false;
        mobileAtaquePendiente = false;
        return true;
    }

    private static bool ConsumirMobilePausa()
    {
        if (!mobilePausaPendiente) return false;
        mobilePausaPendiente = false;
        return true;
    }

    private static float HorizontalMando()
    {
        float mejorValor = 0f;

        foreach (Gamepad mando in Gamepad.all)
        {
            Vector2 direccion = LeerDireccionMando(mando);
            if (Mathf.Abs(direccion.x) > Mathf.Abs(mejorValor))
                mejorValor = direccion.x;
        }

        return Mathf.Abs(mejorValor) >= MovimientoDeadZone ? mejorValor : 0f;
    }

    private static Vector2 LeerDireccionMando(Gamepad mando)
    {
        Vector2 direccion = mando.leftStick.ReadValue();
        Vector2 dpad = mando.dpad.ReadValue();

        if (Mathf.Abs(dpad.x) > 0.01f) direccion.x = dpad.x;
        if (Mathf.Abs(dpad.y) > 0.01f) direccion.y = dpad.y;

        if (Mathf.Abs(direccion.x) < MovimientoDeadZone) direccion.x = 0f;
        if (Mathf.Abs(direccion.y) < MovimientoDeadZone) direccion.y = 0f;

        return direccion;
    }

    private static bool CualquierMandoPresiono(System.Func<Gamepad, ButtonControl> boton)
    {
        foreach (Gamepad mando in Gamepad.all)
        {
            ButtonControl control = boton(mando);
            if (control != null && control.wasPressedThisFrame)
                return true;
        }

        return false;
    }

    private static bool EjeMandoPresionado(Vector2 direccion)
    {
        ActualizarDireccionMando();

        if (direccion == Vector2.up)
            return direccionMandoActual.y > MenuPressThreshold && direccionMandoAnterior.y <= MenuReleaseThreshold;
        if (direccion == Vector2.down)
            return direccionMandoActual.y < -MenuPressThreshold && direccionMandoAnterior.y >= -MenuReleaseThreshold;
        if (direccion == Vector2.right)
            return direccionMandoActual.x > MenuPressThreshold && direccionMandoAnterior.x <= MenuReleaseThreshold;
        if (direccion == Vector2.left)
            return direccionMandoActual.x < -MenuPressThreshold && direccionMandoAnterior.x >= -MenuReleaseThreshold;

        return false;
    }

    private static void ActualizarDireccionMando()
    {
        if (ultimoFrameMando == Time.frameCount) return;

        ultimoFrameMando = Time.frameCount;
        direccionMandoAnterior = direccionMandoActual;
        direccionMandoActual = Vector2.zero;

        foreach (Gamepad mando in Gamepad.all)
        {
            Vector2 direccion = LeerDireccionMando(mando);
            if (Mathf.Abs(direccion.x) > Mathf.Abs(direccionMandoActual.x))
                direccionMandoActual.x = direccion.x;
            if (Mathf.Abs(direccion.y) > Mathf.Abs(direccionMandoActual.y))
                direccionMandoActual.y = direccion.y;
        }
    }
}
