using UnityEngine;

public class ParallaxN1 : MonoBehaviour
{
    [SerializeField] private Transform cam;
    [SerializeField] private float parallaxMultiplier = 1f;
    [SerializeField] private Transform[] layers;
    [SerializeField] private float[] layerSpeeds;

    private Vector3 camStartPos;
    private Vector3[] layerStartPos;

    private void Start()
    {
        if (cam == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                enabled = false;
                return;
            }

            cam = mainCam.transform;
        }

        if (layers == null || layers.Length == 0)
        {
            int childCount = transform.childCount;
            layers = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                layers[i] = transform.GetChild(i);
            }
        }

        if (layerSpeeds == null || layerSpeeds.Length != layers.Length)
        {
            layerSpeeds = new float[layers.Length];
            for (int i = 0; i < layerSpeeds.Length; i++)
            {
                float t = layerSpeeds.Length <= 1 ? 1f : (float)i / (layerSpeeds.Length - 1);
                layerSpeeds[i] = Mathf.Lerp(0.2f, 1f, t);
            }
        }

        camStartPos = cam.position;
        layerStartPos = new Vector3[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            layerStartPos[i] = layers[i] != null ? layers[i].position : Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (cam == null || layers == null || layerStartPos == null)
        {
            return;
        }

        Vector3 camDelta = cam.position - camStartPos;

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] == null)
            {
                continue;
            }

            float speed = layerSpeeds[i] * parallaxMultiplier;
            Vector3 targetPos = layerStartPos[i] + new Vector3(camDelta.x * speed, 0f, 0f);
            layers[i].position = targetPos;
        }
    }
}
