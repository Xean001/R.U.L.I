using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class RestaurarTilemap
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        Tilemap tilemap = null;
        GameObject suelo = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Grid")
            {
                var tm = root.GetComponentInChildren<Tilemap>();
                if (tm != null) tilemap = tm;
            }
            if (root.name == "SueloPlano") suelo = root;
        }

        if (suelo != null) Undo.DestroyObjectImmediate(suelo);

        if (tilemap != null)
        {
            var tmCol = tilemap.GetComponent<TilemapCollider2D>();
            if (tmCol != null) { Undo.RecordObject(tmCol, "x"); tmCol.enabled = true; EditorUtility.SetDirty(tmCol); }
            var compCol = tilemap.GetComponent<CompositeCollider2D>();
            if (compCol != null) { Undo.RecordObject(compCol, "x"); compCol.enabled = true; EditorUtility.SetDirty(compCol); compCol.GenerateGeometry(); }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Tilemap collider restaurado. SueloPlano eliminado.");
    }
}
