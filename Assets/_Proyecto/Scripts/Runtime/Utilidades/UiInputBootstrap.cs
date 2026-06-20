using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class UiInputBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        SceneManager.sceneLoaded += (_, __) => EnsureEventSystem();
        EnsureEventSystem();
    }

    public static void EnsureEventSystem(bool force = false)
    {
        if (!force && !HayUiEnEscena()) return;

        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject go = new GameObject("EventSystem");
            eventSystem = go.AddComponent<EventSystem>();
        }

        InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModule == null)
            inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

        inputModule.enabled = true;
        AsignarAccionesPorDefecto(inputModule);
        DesactivarModulosUiViejos(eventSystem, inputModule);
    }

    private static bool HayUiEnEscena()
    {
        return Object.FindAnyObjectByType<Canvas>() != null ||
               Object.FindAnyObjectByType<UnityEngine.UI.Selectable>() != null;
    }

    private static void AsignarAccionesPorDefecto(InputSystemUIInputModule inputModule)
    {
        PropertyInfo actionsAsset = typeof(InputSystemUIInputModule).GetProperty(
            "actionsAsset",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (actionsAsset?.GetValue(inputModule) != null) return;

        MethodInfo method = typeof(InputSystemUIInputModule).GetMethod(
            "AssignDefaultActions",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        method?.Invoke(inputModule, null);
    }

    private static void DesactivarModulosUiViejos(EventSystem eventSystem, InputSystemUIInputModule inputModule)
    {
        BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
        foreach (BaseInputModule module in modules)
        {
            if (module == null || module == inputModule) continue;

            string typeName = module.GetType().Name;
            if (typeName == "StandaloneInputModule" || typeName == "TouchInputModule")
                module.enabled = false;
        }
    }
}
