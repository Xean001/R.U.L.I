using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public enum RuliTipoControl
{
    TecladoMouse,
    Mando,
    Movil
}

public static class RuliInput
{
    private const float MovimientoDeadZone = 0.2f;
    private const float MenuPressThreshold = 0.6f;
    private const float MenuReleaseThreshold = 0.35f;

    private static int ultimoFrameMando = -1;
    private static Vector2 direccionMandoActual;
    private static Vector2 direccionMandoAnterior;
    private static Vector2 direccionJoystickActual;
    private static Vector2 direccionJoystickAnterior;
    private static bool mobileIzquierda;
    private static bool mobileDerecha;
    private static bool mobileArriba;
    private static bool mobileAbajo;
    private static bool mobileSaltoPendiente;
    private static bool mobileAtaquePendiente;
    private static bool mobilePausaPendiente;
    private static bool mobileRuedaPendiente;
    private static bool mobileSubmitPendiente;
    private static bool mobileCancelPendiente;
    private static bool mobileMenuArribaPendiente;
    private static bool mobileMenuAbajoPendiente;
    private static bool mobileMenuIzquierdaPendiente;
    private static bool mobileMenuDerechaPendiente;
    private static RuliTipoControl ultimoTipoControl = Application.isMobilePlatform
        ? RuliTipoControl.Movil
        : RuliTipoControl.TecladoMouse;

    public static RuliTipoControl TipoControlActual => ultimoTipoControl;

    public static float MovimientoHorizontal()
    {
        float mobile = HorizontalMobile();
        float teclado = HorizontalTeclado();
        float mando = HorizontalMando();
        float joystick = HorizontalJoystick();

        if (Mathf.Abs(mobile) > 0f)
        {
            MarcarControl(RuliTipoControl.Movil);
            return mobile;
        }

        float fisico = Mathf.Abs(mando) > Mathf.Abs(teclado) ? mando : teclado;
        if (Mathf.Abs(joystick) > Mathf.Abs(fisico))
        {
            if (Mathf.Abs(joystick) > 0f) MarcarControl(RuliTipoControl.Mando);
            return joystick;
        }

        if (Mathf.Abs(fisico) > 0f)
            MarcarControl(Mathf.Abs(mando) > Mathf.Abs(teclado) ? RuliTipoControl.Mando : RuliTipoControl.TecladoMouse);

        return fisico;
    }

    public static bool SaltoPresionado()
    {
        // El salto es EXCLUSIVAMENTE con espacio (W queda libre para escalar).
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null && teclado.spaceKey.wasPressedThisFrame;

        bool mobilePresionado = ConsumirMobileSalto();
        bool mandoPresionado = CualquierMandoPresiono(m => m.buttonSouth) ||
            CualquierJoystickPresiono(EsBotonPrincipalJoystick);

        if (tecladoPresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mobilePresionado || mandoPresionado;
    }

    // Eje vertical para escalar lianas: W/Arriba = sube (+1), S/Abajo = baja (-1).
    public static float EscalarVertical()
    {
        Keyboard teclado = Keyboard.current;
        float v = 0f;
        bool tecladoActivo = false;
        bool mandoActivo = false;
        if (teclado != null)
        {
            if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed)
            {
                v += 1f;
                tecladoActivo = true;
            }

            if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed)
            {
                v -= 1f;
                tecladoActivo = true;
            }
        }

        float mobile = VerticalMobile();
        if (Mathf.Abs(mobile) > Mathf.Abs(v))
        {
            v = mobile;
            MarcarControl(RuliTipoControl.Movil);
        }

        foreach (Gamepad mando in Gamepad.all)
        {
            Vector2 d = LeerDireccionMando(mando);
            if (Mathf.Abs(d.y) > Mathf.Abs(v))
            {
                v = d.y > 0f ? 1f : -1f;
                mandoActivo = true;
            }
        }

        foreach (Joystick joystick in Joystick.all)
        {
            Vector2 d = LeerDireccionJoystick(joystick);
            if (Mathf.Abs(d.y) > Mathf.Abs(v))
            {
                v = d.y > 0f ? 1f : -1f;
                mandoActivo = true;
            }
        }

        if (mandoActivo)
            MarcarControl(RuliTipoControl.Mando);
        else if (tecladoActivo)
            MarcarControl(RuliTipoControl.TecladoMouse);

        return Mathf.Clamp(v, -1f, 1f);
    }

    public static bool AtaquePresionado()
    {
        Keyboard teclado = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.fKey.wasPressedThisFrame ||
             teclado.jKey.wasPressedThisFrame ||
             teclado.zKey.wasPressedThisFrame);
        bool mousePresionado = mouse != null && mouse.leftButton.wasPressedThisFrame;

        bool mobilePresionado = ConsumirMobileAtaque();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.buttonWest) ||
            CualquierMandoPresiono(m => m.buttonEast) ||
            CualquierMandoPresiono(m => m.rightShoulder) ||
            CualquierMandoPresiono(m => m.rightTrigger) ||
            CualquierJoystickPresiono(EsBotonAtaqueJoystick);

        if (tecladoPresionado || mousePresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mousePresionado || mobilePresionado || mandoPresionado;
    }

    public static bool RuedaAbierta { get; set; }

    public static bool RuedaHabilidadesPresionada()
    {
        Keyboard teclado = Keyboard.current;
        Mouse mouse = Mouse.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.eKey.wasPressedThisFrame ||
             teclado.tabKey.wasPressedThisFrame ||
             teclado.qKey.wasPressedThisFrame);
        bool mousePresionado = mouse != null && mouse.rightButton.wasPressedThisFrame;

        bool mobilePresionado = ConsumirMobileRueda();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.buttonNorth) ||
            CualquierMandoPresiono(m => m.leftShoulder) ||
            CualquierMandoPresiono(m => m.selectButton) ||
            CualquierJoystickPresiono(EsBotonRuedaJoystick);

        if (tecladoPresionado || mousePresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mousePresionado || mobilePresionado || mandoPresionado;
    }

    public static bool PausaPresionada()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.pKey.wasPressedThisFrame || teclado.escapeKey.wasPressedThisFrame);

        bool mobilePresionado = ConsumirMobilePausa();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.startButton) ||
            CualquierJoystickPresiono(EsBotonPausaJoystick);

        if (tecladoPresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mobilePresionado || mandoPresionado;
    }

    public static bool SubmitPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.enterKey.wasPressedThisFrame ||
             teclado.numpadEnterKey.wasPressedThisFrame ||
             teclado.spaceKey.wasPressedThisFrame);

        bool mobilePresionado = ConsumirMobileSubmit();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.buttonSouth) ||
            CualquierMandoPresiono(m => m.startButton) ||
            CualquierJoystickPresiono(EsBotonPrincipalJoystick);

        if (tecladoPresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mobilePresionado || mandoPresionado;
    }

    public static bool CancelPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.escapeKey.wasPressedThisFrame || teclado.backspaceKey.wasPressedThisFrame);

        bool mobilePresionado = ConsumirMobileCancel();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.buttonEast) ||
            CualquierMandoPresiono(m => m.selectButton) ||
            CualquierJoystickPresiono(EsBotonCancelJoystick);

        if (tecladoPresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mobilePresionado || mandoPresionado;
    }

    public static bool MenuArribaPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame);

        bool mobilePresionado = ConsumirMobileMenuArriba();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.dpad.up) ||
            EjeMandoPresionado(Vector2.up) ||
            EjeJoystickPresionado(Vector2.up);

        if (tecladoPresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mobilePresionado || mandoPresionado;
    }

    public static bool MenuAbajoPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame);

        bool mobilePresionado = ConsumirMobileMenuAbajo();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.dpad.down) ||
            EjeMandoPresionado(Vector2.down) ||
            EjeJoystickPresionado(Vector2.down);

        if (tecladoPresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mobilePresionado || mandoPresionado;
    }

    public static bool MenuIzquierdaPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.leftArrowKey.wasPressedThisFrame || teclado.aKey.wasPressedThisFrame);

        bool mobilePresionado = ConsumirMobileMenuIzquierda();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.dpad.left) ||
            EjeMandoPresionado(Vector2.left) ||
            EjeJoystickPresionado(Vector2.left);

        if (tecladoPresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mobilePresionado || mandoPresionado;
    }

    public static bool MenuDerechaPresionado()
    {
        Keyboard teclado = Keyboard.current;
        bool tecladoPresionado = teclado != null &&
            (teclado.rightArrowKey.wasPressedThisFrame || teclado.dKey.wasPressedThisFrame);

        bool mobilePresionado = ConsumirMobileMenuDerecha();
        bool mandoPresionado =
            CualquierMandoPresiono(m => m.dpad.right) ||
            EjeMandoPresionado(Vector2.right) ||
            EjeJoystickPresionado(Vector2.right);

        if (tecladoPresionado) MarcarControl(RuliTipoControl.TecladoMouse);
        else if (mobilePresionado) MarcarControl(RuliTipoControl.Movil);
        else if (mandoPresionado) MarcarControl(RuliTipoControl.Mando);

        return tecladoPresionado || mobilePresionado || mandoPresionado;
    }

    public static void SetMobileMove(int direccion, bool presionado)
    {
        if (direccion < 0)
        {
            mobileIzquierda = presionado;
            MarcarControl(RuliTipoControl.Movil);
            if (presionado) mobileMenuIzquierdaPendiente = true;
        }
        else if (direccion > 0)
        {
            mobileDerecha = presionado;
            MarcarControl(RuliTipoControl.Movil);
            if (presionado) mobileMenuDerechaPendiente = true;
        }
    }

    public static void SetMobileVertical(int direccion, bool presionado)
    {
        if (direccion < 0)
        {
            mobileAbajo = presionado;
            MarcarControl(RuliTipoControl.Movil);
            if (presionado) mobileMenuAbajoPendiente = true;
        }
        else if (direccion > 0)
        {
            mobileArriba = presionado;
            MarcarControl(RuliTipoControl.Movil);
            if (presionado) mobileMenuArribaPendiente = true;
        }
    }

    public static void MobileJumpDown()
    {
        MarcarControl(RuliTipoControl.Movil);
        mobileSaltoPendiente = true;
    }

    public static void MobileAttackDown()
    {
        MarcarControl(RuliTipoControl.Movil);
        mobileAtaquePendiente = true;
    }

    public static void MobilePauseDown()
    {
        MarcarControl(RuliTipoControl.Movil);
        mobilePausaPendiente = true;
    }

    public static void MobileRuedaDown()
    {
        MarcarControl(RuliTipoControl.Movil);
        mobileRuedaPendiente = true;
    }

    public static void MobileSubmitDown()
    {
        MarcarControl(RuliTipoControl.Movil);
        mobileSubmitPendiente = true;
    }

    public static void MobileCancelDown()
    {
        MarcarControl(RuliTipoControl.Movil);
        mobileCancelPendiente = true;
    }

    public static void ResetMobileState()
    {
        mobileIzquierda = false;
        mobileDerecha = false;
        mobileArriba = false;
        mobileAbajo = false;
        mobileSaltoPendiente = false;
        mobileAtaquePendiente = false;
        mobilePausaPendiente = false;
        mobileRuedaPendiente = false;
        mobileSubmitPendiente = false;
        mobileCancelPendiente = false;
        mobileMenuArribaPendiente = false;
        mobileMenuAbajoPendiente = false;
        mobileMenuIzquierdaPendiente = false;
        mobileMenuDerechaPendiente = false;
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

    private static float VerticalMobile()
    {
        if (mobileArriba == mobileAbajo) return 0f;
        return mobileArriba ? 1f : -1f;
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

    private static bool ConsumirMobileRueda()
    {
        if (!mobileRuedaPendiente) return false;
        mobileRuedaPendiente = false;
        return true;
    }

    private static bool ConsumirMobileSubmit()
    {
        if (!mobileSubmitPendiente) return false;
        mobileSubmitPendiente = false;
        return true;
    }

    private static bool ConsumirMobileCancel()
    {
        if (!mobileCancelPendiente) return false;
        mobileCancelPendiente = false;
        return true;
    }

    private static bool ConsumirMobileMenuArriba()
    {
        if (!mobileMenuArribaPendiente) return false;
        mobileMenuArribaPendiente = false;
        return true;
    }

    private static bool ConsumirMobileMenuAbajo()
    {
        if (!mobileMenuAbajoPendiente) return false;
        mobileMenuAbajoPendiente = false;
        return true;
    }

    private static bool ConsumirMobileMenuIzquierda()
    {
        if (!mobileMenuIzquierdaPendiente) return false;
        mobileMenuIzquierdaPendiente = false;
        return true;
    }

    private static bool ConsumirMobileMenuDerecha()
    {
        if (!mobileMenuDerechaPendiente) return false;
        mobileMenuDerechaPendiente = false;
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

    private static float HorizontalJoystick()
    {
        float mejorValor = 0f;

        foreach (Joystick joystick in Joystick.all)
        {
            Vector2 direccion = LeerDireccionJoystick(joystick);
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

    private static Vector2 LeerDireccionJoystick(Joystick joystick)
    {
        Vector2 direccion = joystick.stick.ReadValue();

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

    private static bool CualquierJoystickPresiono(System.Func<ButtonControl, bool> filtro = null)
    {
        foreach (Joystick joystick in Joystick.all)
        {
            ButtonControl trigger = joystick.trigger;
            if (trigger != null && (filtro == null || filtro(trigger)) && trigger.wasPressedThisFrame)
                return true;

            foreach (InputControl control in joystick.allControls)
            {
                ButtonControl button = control as ButtonControl;
                if (button == null) continue;
                if (filtro != null && !filtro(button)) continue;
                if (button.wasPressedThisFrame)
                    return true;
            }
        }

        return false;
    }

    private static bool EsBotonPrincipalJoystick(ButtonControl boton)
    {
        return NombreBotonContiene(boton, "trigger", "button0", "button 0", "button south", "primary");
    }

    private static bool EsBotonAtaqueJoystick(ButtonControl boton)
    {
        return NombreBotonContiene(boton, "trigger", "button2", "button 2", "button west", "fire");
    }

    private static bool EsBotonCancelJoystick(ButtonControl boton)
    {
        return NombreBotonContiene(boton, "button1", "button 1", "button east", "cancel", "back");
    }

    private static bool EsBotonRuedaJoystick(ButtonControl boton)
    {
        return NombreBotonContiene(boton, "button3", "button 3", "button north", "left shoulder", "select");
    }

    private static bool EsBotonPausaJoystick(ButtonControl boton)
    {
        return NombreBotonContiene(boton, "menu", "start", "pause");
    }

    private static bool NombreBotonContiene(ButtonControl boton, params string[] partes)
    {
        string nombre = boton.name.ToLowerInvariant();
        string display = boton.displayName != null ? boton.displayName.ToLowerInvariant() : string.Empty;

        foreach (string parte in partes)
        {
            if (nombre.Contains(parte) || display.Contains(parte))
                return true;
        }

        return false;
    }

    private static void MarcarControl(RuliTipoControl tipoControl)
    {
        ultimoTipoControl = tipoControl;
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

    private static bool EjeJoystickPresionado(Vector2 direccion)
    {
        ActualizarDireccionMando();

        if (direccion == Vector2.up)
            return direccionJoystickActual.y > MenuPressThreshold && direccionJoystickAnterior.y <= MenuReleaseThreshold;
        if (direccion == Vector2.down)
            return direccionJoystickActual.y < -MenuPressThreshold && direccionJoystickAnterior.y >= -MenuReleaseThreshold;
        if (direccion == Vector2.right)
            return direccionJoystickActual.x > MenuPressThreshold && direccionJoystickAnterior.x <= MenuReleaseThreshold;
        if (direccion == Vector2.left)
            return direccionJoystickActual.x < -MenuPressThreshold && direccionJoystickAnterior.x >= -MenuReleaseThreshold;

        return false;
    }

    private static void ActualizarDireccionMando()
    {
        if (ultimoFrameMando == Time.frameCount) return;

        ultimoFrameMando = Time.frameCount;
        direccionMandoAnterior = direccionMandoActual;
        direccionJoystickAnterior = direccionJoystickActual;
        direccionMandoActual = Vector2.zero;
        direccionJoystickActual = Vector2.zero;

        foreach (Gamepad mando in Gamepad.all)
        {
            Vector2 direccion = LeerDireccionMando(mando);
            if (Mathf.Abs(direccion.x) > Mathf.Abs(direccionMandoActual.x))
                direccionMandoActual.x = direccion.x;
            if (Mathf.Abs(direccion.y) > Mathf.Abs(direccionMandoActual.y))
                direccionMandoActual.y = direccion.y;
        }

        foreach (Joystick joystick in Joystick.all)
        {
            Vector2 direccion = LeerDireccionJoystick(joystick);
            if (Mathf.Abs(direccion.x) > Mathf.Abs(direccionJoystickActual.x))
                direccionJoystickActual.x = direccion.x;
            if (Mathf.Abs(direccion.y) > Mathf.Abs(direccionJoystickActual.y))
                direccionJoystickActual.y = direccion.y;
        }
    }
}
