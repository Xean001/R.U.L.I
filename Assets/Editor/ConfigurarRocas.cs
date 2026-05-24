using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public static class ConfigurarRocas
{
    public static void Execute()
    {
        const string spritePath = "Assets/ObjetosN1/N1/Rocas.png";
        const string animPath = "Assets/ObjetosN1/N1/rocasdestroy.anim";

        var fileIdsAnim = ExtraerFileIdsDeAnim(animPath);
        Debug.Log($"FileIDs en rocasdestroy.anim: {fileIdsAnim.Count}");

        var todosLosSprites = new List<Sprite>();
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(spritePath))
            if (asset is Sprite s) todosLosSprites.Add(s);

        var spritesEnOrden = new List<Sprite>();
        foreach (var id in fileIdsAnim)
        {
            foreach (var s in todosLosSprites)
            {
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s, out _, out long localId) && localId == id)
                {
                    spritesEnOrden.Add(s);
                    break;
                }
            }
        }

        if (spritesEnOrden.Count == 0)
        {
            Debug.LogWarning("No se pudo mapear fileIDs. Usando todos los sprites.");
            spritesEnOrden = todosLosSprites;
        }

        Debug.Log($"Frames para Rocas: {spritesEnOrden.Count}");

        var scene = EditorSceneManager.GetActiveScene();
        GameObject rocas = null;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Rocas") { rocas = root; break; }
        if (rocas == null) { Debug.LogError("Rocas no encontrado"); return; }

        Undo.RegisterFullObjectHierarchyUndo(rocas, "Configurar Rocas");

        var anim = rocas.GetComponent<Animator>();
        if (anim != null) Object.DestroyImmediate(anim);

        var box = rocas.GetComponent<BoxCollider2D>();
        if (box != null) Object.DestroyImmediate(box);
        var circle = rocas.GetComponent<CircleCollider2D>();
        if (circle != null) Object.DestroyImmediate(circle);
        var polyExistente = rocas.GetComponent<PolygonCollider2D>();
        if (polyExistente != null) Object.DestroyImmediate(polyExistente);

        var poly = rocas.AddComponent<PolygonCollider2D>();
        poly.isTrigger = false;

        var sr = rocas.GetComponent<SpriteRenderer>();
        if (sr != null && spritesEnOrden.Count > 0) sr.sprite = spritesEnOrden[0];

        var script = rocas.GetComponent<Cilindro>();
        if (script == null) script = rocas.AddComponent<Cilindro>();
        var so = new SerializedObject(script);
        var framesProp = so.FindProperty("frames");
        framesProp.arraySize = spritesEnOrden.Count;
        for (int i = 0; i < spritesEnOrden.Count; i++)
            framesProp.GetArrayElementAtIndex(i).objectReferenceValue = spritesEnOrden[i];
        so.FindProperty("golpesPorFrame").intValue = 3;
        var golpesExtra = so.FindProperty("golpesExtraFinal");
        if (golpesExtra != null) golpesExtra.intValue = 3;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(rocas);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Rocas configurado con {spritesEnOrden.Count} frames.");
    }

    static List<long> ExtraerFileIdsDeAnim(string path)
    {
        var resultado = new List<long>();
        if (!File.Exists(path)) return resultado;

        var lineas = File.ReadAllLines(path);
        bool dentroDeKeyframes = false;
        var regex = new Regex(@"value:\s*\{fileID:\s*(-?\d+)");
        foreach (var l in lineas)
        {
            if (l.Contains("pptrCurveMapping:")) break;
            if (l.Contains("m_PPtrCurves:")) dentroDeKeyframes = true;
            if (!dentroDeKeyframes) continue;
            var m = regex.Match(l);
            if (m.Success) resultado.Add(long.Parse(m.Groups[1].Value));
        }
        return resultado;
    }
}
