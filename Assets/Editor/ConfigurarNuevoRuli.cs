using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ConfigurarNuevoRuli
{
    public static void Execute()
    {
        var scene = EditorSceneManager.GetActiveScene();

        GameObject viejoRuli = null;
        GameObject nuevo = null;
        GameObject camara = null;

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "Ruli") viejoRuli = root;
            else if (root.name == "ruliSprite__0") nuevo = root;
            else if (root.name == "Main Camera") camara = root;
        }

        if (nuevo == null) { Debug.LogError("No se encontro ruliSprite__0"); return; }

        Vector3 posicion = new Vector3(-4.12f, -0.89f, 0f);
        float escala = 0.6670506f;
        if (viejoRuli != null)
        {
            posicion = viejoRuli.transform.position;
            escala = viejoRuli.transform.localScale.x;
            Undo.DestroyObjectImmediate(viejoRuli);
        }

        Undo.RegisterFullObjectHierarchyUndo(nuevo, "Configurar Ruli");
        nuevo.name = "Ruli";
        nuevo.transform.position = posicion;
        nuevo.transform.localScale = new Vector3(escala, escala, escala);
        nuevo.transform.rotation = Quaternion.identity;

        var rb = nuevo.GetComponent<Rigidbody2D>();
        if (rb == null) rb = nuevo.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var caps = nuevo.GetComponent<CapsuleCollider2D>();
        if (caps == null) caps = nuevo.AddComponent<CapsuleCollider2D>();
        caps.direction = CapsuleDirection2D.Vertical;
        var sr = nuevo.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var sz = sr.size;
            caps.size = new Vector2(sz.x * 0.5f, sz.y * 0.85f);
            caps.offset = new Vector2(0f, 0f);
        }
        caps.isTrigger = false;

        var anim = nuevo.GetComponent<Animator>();
        if (anim == null) anim = nuevo.AddComponent<Animator>();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/animations/idle.controller");
        if (controller != null) anim.runtimeAnimatorController = controller;
        else Debug.LogWarning("No se cargo idle.controller");

        var movScript = nuevo.GetComponent<RuliMovimiento>();
        if (movScript == null) movScript = nuevo.AddComponent<RuliMovimiento>();
        movScript.velocidad = 3f;
        movScript.fuerzaSalto = 7f;

        if (camara != null)
        {
            var follow = camara.GetComponent<CameraFollow>();
            if (follow != null)
            {
                var so = new SerializedObject(follow);
                so.FindProperty("target").objectReferenceValue = nuevo.transform;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(follow);
            }
        }

        EditorUtility.SetDirty(nuevo);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Ruli configurado en pos={posicion} escala={escala}");
    }
}
