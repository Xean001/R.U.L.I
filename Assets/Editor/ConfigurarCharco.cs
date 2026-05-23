using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

public static class ConfigurarCharco
{
    public static void Execute()
    {
        const string tagMortal = "Mortal";
        AsegurarTag(tagMortal);

        var scene = EditorSceneManager.GetActiveScene();
        GameObject charco = null;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "charco") { charco = root; break; }
        if (charco == null) { Debug.LogError("charco no encontrado"); return; }

        Undo.RegisterFullObjectHierarchyUndo(charco, "Configurar charco");

        var box = charco.GetComponent<BoxCollider2D>();
        if (box != null) Object.DestroyImmediate(box);
        var circle = charco.GetComponent<CircleCollider2D>();
        if (circle != null) Object.DestroyImmediate(circle);
        var polyExistente = charco.GetComponent<PolygonCollider2D>();
        if (polyExistente != null) Object.DestroyImmediate(polyExistente);

        var poly = charco.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.offset = Vector2.zero;

        var sr = charco.GetComponent<SpriteRenderer>();
        if (poly.GetTotalPointCount() == 0 && sr != null && sr.sprite != null)
        {
            int shapeCount = sr.sprite.GetPhysicsShapeCount();
            if (shapeCount > 0)
            {
                poly.pathCount = shapeCount;
                var temp = new List<Vector2>();
                for (int i = 0; i < shapeCount; i++)
                {
                    sr.sprite.GetPhysicsShape(i, temp);
                    poly.SetPath(i, temp.ToArray());
                }
            }
        }

        if (poly.GetTotalPointCount() == 0 && sr != null)
        {
            var size = sr.size;
            float hw = size.x * 0.5f;
            float hh = size.y * 0.5f;
            poly.pathCount = 1;
            poly.SetPath(0, new[] {
                new Vector2(-hw, -hh),
                new Vector2( hw, -hh),
                new Vector2( hw,  hh),
                new Vector2(-hw,  hh),
            });
        }

        charco.tag = tagMortal;

        EditorUtility.SetDirty(charco);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Charco configurado: trigger=true tag={tagMortal} puntos={poly.GetTotalPointCount()}");
    }

    static void AsegurarTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
