using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class InspeccionarColliders
{
    public static void Execute()
    {
        Debug.Log("=== Inspeccion v2 ===");
        var scene = EditorSceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "marea 3_0" || root.name == "barril" || root.name == "carro" || root.name == "llanta" || root.name == "llanta (1)")
            {
                var poly = root.GetComponent<PolygonCollider2D>();
                if (poly != null)
                    Debug.Log($"{root.name}: pathCount={poly.pathCount} totalPoints={poly.GetTotalPointCount()} isTrigger={poly.isTrigger} tag={root.tag}");
                else
                    Debug.Log($"{root.name}: SIN PolygonCollider2D");
            }
        }
    }
}
