using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ReducirEscala
{
    public static void Execute()
    {
        const float factor = 0.55f;

        var scene = EditorSceneManager.GetActiveScene();
        string[] nombres = { "Ruli", "barril", "carro", "llanta", "llanta (1)", "marea 3_0" };

        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var n in nombres)
            {
                if (root.name != n) continue;
                Undo.RegisterCompleteObjectUndo(root.transform, "Reducir escala");
                var s = root.transform.localScale;
                root.transform.localScale = new Vector3(
                    Mathf.Sign(s.x) * Mathf.Abs(s.x) * factor,
                    Mathf.Sign(s.y) * Mathf.Abs(s.y) * factor,
                    s.z != 0 ? Mathf.Sign(s.z) * Mathf.Abs(s.z) * factor : s.z);
                EditorUtility.SetDirty(root);
                Debug.Log($"{n}: escala = {root.transform.localScale}");
            }
        }

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name != "Ruli") continue;
            var mov = root.GetComponent<RuliMovimiento>();
            if (mov != null)
            {
                Undo.RegisterCompleteObjectUndo(mov, "Ajustar velocidad");
                mov.velocidad = 3f;
                mov.fuerzaSalto = 7f;
                EditorUtility.SetDirty(mov);
                Debug.Log($"Ruli velocidad={mov.velocidad}, salto={mov.fuerzaSalto}");
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
