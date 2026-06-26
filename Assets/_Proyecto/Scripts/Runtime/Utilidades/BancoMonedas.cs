using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Sistema de guardado de monedas en JSON (persistentDataPath/guardado.json).
///
/// - La billetera (monedas) = suma de tu RECORD por nivel - lo gastado.
/// - Anti-farmeo: cada nivel recuerda el maximo de monedas que recogiste en una
///   sola partida. Repetir el nivel NO suma de nuevo; solo suma la diferencia si
///   superas tu record (ej: tenias 10, recoges 11 -> +1).
/// </summary>
public static class BancoMonedas
{
    [Serializable]
    public class NivelRecord
    {
        public int nivel;
        public int mejor;
    }

    [Serializable]
    public class Datos
    {
        public int monedas = 0;
        public List<NivelRecord> niveles = new List<NivelRecord>();
    }

    private static Datos datos;

    private static string Ruta => Path.Combine(Application.persistentDataPath, "guardado.json");

    private static void Asegurar()
    {
        if (datos != null) return;
        Cargar();
    }

    public static void Cargar()
    {
        try
        {
            if (File.Exists(Ruta))
            {
                datos = JsonUtility.FromJson<Datos>(File.ReadAllText(Ruta)) ?? new Datos();
            }
            else
            {
                // Primera vez: migrar las monedas que hubiera en PlayerPrefs.
                datos = new Datos { monedas = PlayerPrefs.GetInt("Monedas", 0) };
                Guardar();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("BancoMonedas: no se pudo cargar el guardado, se crea uno nuevo. " + e.Message);
            datos = new Datos();
        }
    }

    public static void Guardar()
    {
        Asegurar();
        try
        {
            File.WriteAllText(Ruta, JsonUtility.ToJson(datos, true));
            // Espejo en PlayerPrefs para cualquier UI antigua que aun lo lea.
            PlayerPrefs.SetInt("Monedas", datos.monedas);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError("BancoMonedas: no se pudo guardar. " + e.Message);
        }
    }

    public static int Monedas
    {
        get { Asegurar(); return datos.monedas; }
    }

    public static int MejorDeNivel(int nivel)
    {
        Asegurar();
        var r = datos.niveles.Find(n => n.nivel == nivel);
        return r != null ? r.mejor : 0;
    }

    /// <summary>
    /// Registra las monedas recogidas en una partida de un nivel. Solo suma a la
    /// billetera la diferencia si superas tu record previo. Devuelve cuanto sumo.
    /// </summary>
    public static int RegistrarNivel(int nivel, int recolectadas)
    {
        Asegurar();
        var r = datos.niveles.Find(n => n.nivel == nivel);
        int mejorPrevio = r != null ? r.mejor : 0;
        int delta = Mathf.Max(0, recolectadas - mejorPrevio);

        if (delta > 0)
        {
            datos.monedas += delta;
            if (r != null) r.mejor = recolectadas;
            else datos.niveles.Add(new NivelRecord { nivel = nivel, mejor = recolectadas });
            Guardar();
        }
        return delta;
    }

    /// <summary>Intenta gastar monedas. Devuelve true si alcanzaba el saldo.</summary>
    public static bool Gastar(int costo)
    {
        Asegurar();
        if (datos.monedas < costo) return false;
        datos.monedas -= costo;
        Guardar();
        return true;
    }
}
