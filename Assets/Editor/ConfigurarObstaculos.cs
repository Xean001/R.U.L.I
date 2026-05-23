using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class ConfigurarObstaculos
{
    public static void Execute()
    {
        const string tagMortal = "Mortal";
        AsegurarTag(tagMortal);

        const string matPath = "Assets/PhysicsMaterials/Rebote.physicsMaterial2D";
        Directory.CreateDirectory("Assets/PhysicsMaterials");
        var matRebote = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
        if (matRebote == null)
        {
            matRebote = new PhysicsMaterial2D("Rebote") { bounciness = 1.2f, friction = 0.2f };
            AssetDatabase.CreateAsset(matRebote, matPath);
        }
        AssetDatabase.SaveAssets();

        var scene = EditorSceneManager.GetActiveScene();
        GameObject barril = null, carro = null, llanta1 = null, llanta2 = null, marea = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            switch (root.name)
            {
                case "barril": barril = root; break;
                case "carro": carro = root; break;
                case "llanta": llanta1 = root; break;
                case "llanta (1)": llanta2 = root; break;
                case "marea 3_0": marea = root; break;
            }
        }

        ConfigurarPoligono(barril, false, null, false);
        ConfigurarPoligono(carro, false, null, false);
        ConfigurarPoligono(llanta1, false, matRebote, false);
        ConfigurarPoligono(llanta2, false, matRebote, false);
        ConfigurarPoligono(marea, true, null, true);
        if (marea != null) marea.tag = tagMortal;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Polygon colliders aplicados.");
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

    static void ConfigurarPoligono(GameObject go, bool esTrigger, PhysicsMaterial2D mat, bool forzarRectangulo)
    {
        if (go == null) return;
        Undo.RegisterFullObjectHierarchyUndo(go, "Polygon collider");

        var sr = go.GetComponent<SpriteRenderer>();
        if (!forzarRectangulo) AsegurarPhysicsShape(sr);

        var box = go.GetComponent<BoxCollider2D>();
        if (box != null) Object.DestroyImmediate(box);
        var circle = go.GetComponent<CircleCollider2D>();
        if (circle != null) Object.DestroyImmediate(circle);
        var polyExistente = go.GetComponent<PolygonCollider2D>();
        if (polyExistente != null) Object.DestroyImmediate(polyExistente);

        var poly = go.AddComponent<PolygonCollider2D>();

        if (!forzarRectangulo && poly.GetTotalPointCount() == 0 && sr != null && sr.sprite != null)
        {
            ConstruirDesdeSprite(poly, sr.sprite);
        }

        if (forzarRectangulo || poly.GetTotalPointCount() == 0)
        {
            if (sr != null)
            {
                var size = sr.size;
                var hw = size.x * 0.5f;
                float anchoFrac = forzarRectangulo ? 0.95f : 1f;
                float altoFrac = forzarRectangulo ? 0.45f : 1f;
                float halfW = hw * anchoFrac;
                float halfH = size.y * altoFrac * 0.5f;
                float centroY = -size.y * 0.5f + halfH;
                poly.pathCount = 1;
                poly.SetPath(0, new[] {
                    new Vector2(-halfW, centroY - halfH),
                    new Vector2( halfW, centroY - halfH),
                    new Vector2( halfW, centroY + halfH),
                    new Vector2(-halfW, centroY + halfH),
                });
            }
        }

        poly.isTrigger = esTrigger;
        poly.sharedMaterial = mat;
        poly.offset = Vector2.zero;

        EditorUtility.SetDirty(go);
    }

    static void AsegurarPhysicsShape(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null) return;
        var assetPath = AssetDatabase.GetAssetPath(sr.sprite);
        if (string.IsNullOrEmpty(assetPath)) return;
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        var factory = new SpriteDataProviderFactories();
        factory.Init();
        var provider = factory.GetSpriteEditorDataProviderFromObject(importer);
        if (provider == null) return;
        provider.InitSpriteEditorDataProvider();

        var shapeProvider = provider.GetDataProvider<ISpritePhysicsOutlineDataProvider>();
        if (shapeProvider == null) return;

        bool huboCambios = false;
        foreach (var sd in provider.GetSpriteRects())
        {
            var outlines = shapeProvider.GetOutlines(sd.spriteID);
            if (outlines == null || outlines.Count == 0)
            {
                var generadas = SpriteUtilityGenerateOutline(sr.sprite);
                if (generadas != null && generadas.Count > 0)
                {
                    shapeProvider.SetOutlines(sd.spriteID, generadas);
                    huboCambios = true;
                }
            }
        }

        if (huboCambios)
        {
            provider.Apply();
            importer.SaveAndReimport();
        }
    }

    static List<Vector2[]> SpriteUtilityGenerateOutline(Sprite sprite)
    {
        var resultado = new List<Vector2[]>();
        try
        {
            var method = typeof(UnityEditor.Sprites.SpriteUtility).GetMethod(
                "GenerateOutline",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (method != null)
            {
                var args = new object[] { sprite.texture, sprite.rect, 0.05f, (byte)200, true, null };
                method.Invoke(null, args);
                var paths = args[5] as Vector2[][];
                if (paths != null) foreach (var p in paths) resultado.Add(p);
            }
        }
        catch { }
        return resultado;
    }

    static void ConstruirDesdeSprite(PolygonCollider2D poly, Sprite sprite)
    {
        int shapeCount = sprite.GetPhysicsShapeCount();
        if (shapeCount <= 0) return;
        poly.pathCount = shapeCount;
        var temp = new List<Vector2>();
        for (int i = 0; i < shapeCount; i++)
        {
            sprite.GetPhysicsShape(i, temp);
            poly.SetPath(i, temp.ToArray());
        }
    }
}
