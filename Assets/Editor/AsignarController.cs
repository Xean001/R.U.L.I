using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AsignarController
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();
        GameObject ruli = null;
        foreach (var root in scene.GetRootGameObjects())
            if (root.name == "Ruli") { ruli = root; break; }
        if (ruli == null) { Debug.LogError("Ruli no encontrado"); return; }

        var anim = ruli.GetComponent<Animator>();
        if (anim == null) anim = ruli.AddComponent<Animator>();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/animations/idle.controller");
        if (controller == null) { Debug.LogError("Controller no cargado"); return; }
        anim.runtimeAnimatorController = controller;
        EditorUtility.SetDirty(anim);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Controller asignado a Ruli");
    }
}
