using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ObjetoRompible : MonoBehaviour
{
    public int golpesPorDestruir = 3;
    public GameObject monedaPrefab;

    private static readonly Color[] tintes =
    {
        Color.white,
        new Color(1f, 0.65f, 0.3f),
        new Color(1f, 0.25f, 0.1f),
    };

    private Animator anim;
    private SpriteRenderer sr;
    private int golpes;
    private bool destruido;

    void Awake()
    {
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!destruido) return;
        var info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("destruir") && info.normalizedTime >= 1f)
            Destroy(gameObject);
    }

    public void Golpe()
    {
        if (destruido) return;
        golpes = Mathf.Clamp(golpes + 1, 0, golpesPorDestruir);
        sr.color = tintes[Mathf.Clamp(golpes, 0, tintes.Length - 1)];

        StopAllCoroutines();
        StartCoroutine(Sacudir());

        if (golpes >= golpesPorDestruir)
        {
            destruido = true;
            anim.SetTrigger("destruir");
            if (monedaPrefab != null)
                Instantiate(monedaPrefab,
                    transform.position + Vector3.up * 0.5f,
                    Quaternion.identity);
        }
    }

    IEnumerator Sacudir()
    {
        Vector3 pos = transform.localPosition;
        float t = 0f;
        while (t < 0.22f)
        {
            float o = Mathf.Sin(t * 80f) * 0.07f;
            transform.localPosition = pos + new Vector3(o, 0f, 0f);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = pos;
    }
}
