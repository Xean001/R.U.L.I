using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RecrearController
{
    public static void Execute()
    {
        const string path = "Assets/animations/idle.controller";
        var existente = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existente != null) AssetDatabase.DeleteAsset(path);

        var c = AnimatorController.CreateAnimatorControllerAtPath(path);

        c.AddParameter("velocidadX", AnimatorControllerParameterType.Float);
        c.AddParameter("en suelo", AnimatorControllerParameterType.Bool);
        c.AddParameter("muerto", AnimatorControllerParameterType.Trigger);
        c.AddParameter("atacar", AnimatorControllerParameterType.Trigger);

        var layer = c.layers[0];
        var sm = layer.stateMachine;

        var clipIdle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/animations/idle.anim");
        var clipRun = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/animations/run.anim");
        var clipJump = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/animations/jump.anim");
        var clipAttack = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/animations/attack.anim");
        var clipDead = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/animations/dead.anim");

        var idle = sm.AddState("idle"); idle.motion = clipIdle; idle.speed = 0.3f;
        var run = sm.AddState("run"); run.motion = clipRun;
        var jump = sm.AddState("jump"); jump.motion = clipJump;
        var attack = sm.AddState("attack"); attack.motion = clipAttack;
        var dead = sm.AddState("dead"); dead.motion = clipDead;

        sm.defaultState = idle;

        var t1 = idle.AddTransition(run);
        t1.hasExitTime = false; t1.duration = 0f;
        t1.AddCondition(AnimatorConditionMode.Greater, 0f, "velocidadX");

        var t2 = idle.AddTransition(run);
        t2.hasExitTime = false; t2.duration = 0f;
        t2.AddCondition(AnimatorConditionMode.Less, 0f, "velocidadX");

        var t3 = run.AddTransition(idle);
        t3.hasExitTime = false; t3.duration = 0f;
        t3.AddCondition(AnimatorConditionMode.Greater, -0.1f, "velocidadX");
        t3.AddCondition(AnimatorConditionMode.Less, 0.1f, "velocidadX");

        var t4 = jump.AddTransition(idle);
        t4.hasExitTime = false; t4.duration = 0f;
        t4.AddCondition(AnimatorConditionMode.If, 0f, "en suelo");

        var tDead = sm.AddAnyStateTransition(dead);
        tDead.hasExitTime = false; tDead.duration = 0f;
        tDead.AddCondition(AnimatorConditionMode.If, 0f, "muerto");

        var tAttack = sm.AddAnyStateTransition(attack);
        tAttack.hasExitTime = false; tAttack.duration = 0f;
        tAttack.AddCondition(AnimatorConditionMode.If, 0f, "atacar");

        var tJump = sm.AddAnyStateTransition(jump);
        tJump.hasExitTime = false; tJump.duration = 0f;
        tJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "en suelo");

        var tAttackExit = attack.AddTransition(idle);
        tAttackExit.hasExitTime = true; tAttackExit.exitTime = 0.95f; tAttackExit.duration = 0f;

        AssetDatabase.SaveAssets();

        var scene = EditorSceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name != "Ruli") continue;
            var anim = root.GetComponent<Animator>();
            if (anim == null) anim = root.AddComponent<Animator>();
            anim.runtimeAnimatorController = c;
            EditorUtility.SetDirty(anim);
            break;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Controller recreado en {path} y asignado a Ruli");
    }
}
