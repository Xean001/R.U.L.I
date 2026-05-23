using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LimpiarRuli
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        GameObject ruli = null;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Ruli") { ruli = root; break; }
        if (ruli == null) { Debug.LogError("Ruli no encontrado"); return; }

        Undo.RegisterFullObjectHierarchyUndo(ruli, "Limpiar Ruli");

        var box = ruli.GetComponent<BoxCollider2D>();
        if (box != null) Object.DestroyImmediate(box);

        var caps = ruli.GetComponent<CapsuleCollider2D>();
        if (caps == null) caps = ruli.AddComponent<CapsuleCollider2D>();
        caps.direction = CapsuleDirection2D.Vertical;
        caps.isTrigger = false;
        var sr = ruli.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            var sb = sr.sprite.bounds.size;
            caps.size = new Vector2(sb.x * 0.55f, sb.y * 0.95f);
            caps.offset = new Vector2(0f, 0f);
        }

        var rb = ruli.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;
            rb.simulated = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        EditorUtility.SetDirty(ruli);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Ruli limpio. capsule size={caps.size} offset={caps.offset}");
    }
}
