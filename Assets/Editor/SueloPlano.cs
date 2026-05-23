using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class SueloPlano
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        Tilemap tilemap = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Grid")
            {
                var tm = root.GetComponentInChildren<Tilemap>();
                if (tm != null) { tilemap = tm; break; }
            }
        }
        if (tilemap == null) { Debug.LogError("No se encontro Tilemap"); return; }

        var tmCol = tilemap.GetComponent<TilemapCollider2D>();
        if (tmCol != null) { Undo.RecordObject(tmCol, "x"); tmCol.enabled = false; EditorUtility.SetDirty(tmCol); }
        var compCol = tilemap.GetComponent<CompositeCollider2D>();
        if (compCol != null) { Undo.RecordObject(compCol, "x"); compCol.enabled = false; EditorUtility.SetDirty(compCol); }

        tilemap.CompressBounds();
        var cellBounds = tilemap.cellBounds;
        int xMin = int.MaxValue, xMax = int.MinValue, yMax = int.MinValue;
        for (int x = cellBounds.xMin; x < cellBounds.xMax; x++)
        {
            for (int y = cellBounds.yMin; y < cellBounds.yMax; y++)
            {
                if (tilemap.GetTile(new Vector3Int(x, y, 0)) != null)
                {
                    if (x < xMin) xMin = x;
                    if (x > xMax) xMax = x;
                    if (y > yMax) yMax = y;
                }
            }
        }
        if (xMin == int.MaxValue) { Debug.LogError("Tilemap vacio"); return; }

        var grid = tilemap.layoutGrid;
        var topRightCell = new Vector3Int(xMax, yMax, 0);
        var bottomLeftCell = new Vector3Int(xMin, yMax, 0);
        Vector3 wBL = tilemap.CellToWorld(bottomLeftCell);
        Vector3 wTR = tilemap.CellToWorld(topRightCell) + new Vector3(grid.cellSize.x, grid.cellSize.y, 0);

        float worldXMin = wBL.x - 12f;
        float worldXMax = wTR.x + 5f;
        float topeY = wTR.y;

        GameObject suelo = null;
        foreach (var r in scene.GetRootGameObjects())
            if (r.name == "SueloPlano") { suelo = r; break; }
        if (suelo == null)
        {
            suelo = new GameObject("SueloPlano");
            Undo.RegisterCreatedObjectUndo(suelo, "Crear suelo plano");
        }

        float alturaCollider = 2.0f;
        float anchoMundo = worldXMax - worldXMin;
        float centroX = (worldXMin + worldXMax) * 0.5f;
        float centroY = topeY - alturaCollider * 0.5f;

        suelo.transform.position = new Vector3(centroX, centroY, 0f);
        suelo.transform.rotation = Quaternion.identity;
        suelo.transform.localScale = Vector3.one;

        var box = suelo.GetComponent<BoxCollider2D>();
        if (box == null) box = suelo.AddComponent<BoxCollider2D>();
        box.size = new Vector2(anchoMundo, alturaCollider);
        box.offset = Vector2.zero;
        box.isTrigger = false;

        EditorUtility.SetDirty(suelo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Suelo plano: ancho={anchoMundo:F2} xMin={worldXMin:F2} xMax={worldXMax:F2} topeY={topeY:F2} centroY={centroY:F2}");
    }
}
