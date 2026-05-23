using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ExtenderNivel1
{
    public static void Execute()
    {
        const int repeticiones = 2;

        var scene = EditorSceneManager.GetActiveScene();

        Transform background = null;
        Transform limiteDerecha = null;
        Transform mainCam = null;
        Tilemap tilemap = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Background") background = root.transform;
            if (root.name == "LimiteDerecha") limiteDerecha = root.transform;
            if (root.name == "Main Camera") mainCam = root.transform;
            if (root.name == "Grid")
            {
                var tm = root.GetComponentInChildren<Tilemap>();
                if (tm != null) tilemap = tm;
            }
        }

        if (background == null || limiteDerecha == null || mainCam == null || tilemap == null)
        {
            Debug.LogError($"Faltan refs: bg={background} limR={limiteDerecha} cam={mainCam} tm={tilemap}");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(background.gameObject, "Extender background");

        int originalChildCount = background.childCount;
        var originales = new Transform[originalChildCount];
        for (int i = 0; i < originalChildCount; i++) originales[i] = background.GetChild(i);

        foreach (var orig in originales)
        {
            var rend = orig.GetComponent<Renderer>();
            if (rend == null) continue;
            float worldWidth = rend.bounds.size.x;

            for (int n = 1; n <= repeticiones; n++)
            {
                var copia = Object.Instantiate(orig.gameObject, background);
                copia.name = orig.name + "_dup" + n;
                copia.transform.position = orig.position + new Vector3(worldWidth * n, 0f, 0f);
            }
        }

        Undo.RegisterCompleteObjectUndo(tilemap, "Extender tilemap");
        var bounds = tilemap.cellBounds;
        int anchoCeldas = bounds.size.x;

        for (int rep = 1; rep <= repeticiones; rep++)
        {
            int offsetX = anchoCeldas * rep;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var tile = tilemap.GetTile(pos);
                    if (tile == null) continue;
                    var destino = new Vector3Int(x + offsetX, y, 0);
                    tilemap.SetTile(destino, tile);
                    var matrix = tilemap.GetTransformMatrix(pos);
                    tilemap.SetTransformMatrix(destino, matrix);
                }
            }
        }
        tilemap.RefreshAllTiles();

        var compCol = tilemap.GetComponent<CompositeCollider2D>();
        if (compCol != null) compCol.GenerateGeometry();

        Undo.RegisterCompleteObjectUndo(limiteDerecha, "Mover LimiteDerecha");
        var bgRend = background.GetComponentInChildren<Renderer>();
        float maxXBg = float.NegativeInfinity;
        foreach (var r in background.GetComponentsInChildren<Renderer>())
        {
            if (r.bounds.max.x > maxXBg) maxXBg = r.bounds.max.x;
        }
        limiteDerecha.position = new Vector3(maxXBg - 2f, limiteDerecha.position.y, limiteDerecha.position.z);

        var follow = mainCam.GetComponent<CameraFollow>();
        if (follow != null)
        {
            var so = new SerializedObject(follow);
            float cellSize = (tilemap.GetComponentInParent<Grid>()?.cellSize.x) ?? 1f;
            float aspectHalfWidth = 5f * (16f / 9f);
            float nuevoMaxX = maxXBg - aspectHalfWidth;
            so.FindProperty("maxX").floatValue = nuevoMaxX;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(follow);
            Debug.Log($"CameraFollow.maxX = {nuevoMaxX}");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Nivel extendido. Ancho fondo derecha = {maxXBg}. Tilemap ancho celdas = {anchoCeldas * (repeticiones + 1)}");
    }
}
