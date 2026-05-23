using UnityEditor;
using UnityEditor.SceneManagement;

public static class GuardarEscena
{
    public static void Execute()
    {
        var s = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(s);
        EditorSceneManager.SaveScene(s);
    }
}
