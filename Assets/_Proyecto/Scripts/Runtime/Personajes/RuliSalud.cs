using UnityEngine;
using System;

public class RuliSalud : MonoBehaviour
{
    public int vidaMaxima = 8;
    public float tiempoInvulnerable = 1f;

    public int VidaActual { get; private set; }
    public event Action<int, int> OnVidaCambiada;

    private float timerInvul;
    private RuliMovimiento movimiento;
    private SpriteRenderer sr;

    [Header("Audio")]
    public AudioClip sonidoRecibirDaño;
    private AudioSource audioSource;

    void Awake()
    {
        VidaActual  = vidaMaxima;
        movimiento  = GetComponent<RuliMovimiento>();
        sr          = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (timerInvul > 0f)
        {
            timerInvul -= Time.deltaTime;
            // Parpadeo de invulnerabilidad
            if (sr != null)
                sr.enabled = Mathf.Sin(timerInvul * 20f) > 0f;
        }
        else if (sr != null)
            sr.enabled = true;
    }

    public void RecibirDaño(int cantidad = 1)
    {
        if (timerInvul > 0f) return;
        VidaActual = Mathf.Max(0, VidaActual - cantidad);
        timerInvul = tiempoInvulnerable;
        OnVidaCambiada?.Invoke(VidaActual, vidaMaxima);

        // SONIDO DE DAÑO
        if (audioSource != null && sonidoRecibirDaño != null)
        {
            audioSource.PlayOneShot(sonidoRecibirDaño);
        }


        if (VidaActual <= 0 && movimiento != null)
            movimiento.Morir();
    }
}
