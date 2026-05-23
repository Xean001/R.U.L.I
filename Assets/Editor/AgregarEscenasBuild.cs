using UnityEditor;
using System.Collections.Generic;

public static class AgregarEscenasBuild
{
    public static void Execute()
    {
        var lista = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string[] aIncluir = {
            "Assets/Scenes/Nivel1.unity",
            "Assets/Scenes/Nivel2.unity",
            "Assets/Scenes/Nivel3.unity",
        };

        foreach (var ruta in aIncluir)
        {
            bool yaEsta = lista.Exists(s => s.path == ruta);
            if (!yaEsta) lista.Add(new EditorBuildSettingsScene(ruta, true));
        }

        EditorBuildSettings.scenes = lista.ToArray();
        UnityEngine.Debug.Log($"Build settings actualizado. Escenas: {lista.Count}");
    }
}
