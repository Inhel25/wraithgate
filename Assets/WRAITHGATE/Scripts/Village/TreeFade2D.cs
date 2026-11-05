using UnityEngine;

/// <summary>
/// Pasang pada anak Tree_FadeZone yang memiliki BoxCollider2D (Is Trigger = true).
/// Pohon menjadi transparan hanya jika pemain ada di belakang batang
/// dan kaki pemain memasuki area FadeZone.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class TreeFade2D : MonoBehaviour
{
    [Header("Tampilan pohon")]
    [SerializeField] private SpriteRenderer[] treeRenderers;
    [SerializeField] private Transform treeFootPoint;

    [Header("Transparansi")]
    [Range(0.1f, 1f)]
    [SerializeField] private float hiddenAlphaMultiplier = 0.45f;
    [Min(0.1f)]
    [SerializeField] private float alphaChangePerSecond = 3f;
    [SerializeField] private bool onlyWhenBehind = true;

    private Collider2D playerCollider;
    private float[] originalAlphas;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (treeRenderers == null)
            treeRenderers = new SpriteRenderer[0];

        originalAlphas = new float[treeRenderers.Length];
        for (int i = 0; i < treeRenderers.Length; i++)
        {
            if (treeRenderers[i] != null)
                originalAlphas[i] = treeRenderers[i].color.a;
        }

        if (treeFootPoint == null)
            treeFootPoint = transform.parent != null ? transform.parent : transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement2D>() != null)
            playerCollider = other;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == playerCollider)
            playerCollider = null;
    }

    private void Update()
    {
        bool playerInside = playerCollider != null && playerCollider.enabled;
        bool shouldFade = playerInside;

        if (shouldFade && onlyWhenBehind)
        {
            // Y yang lebih besar berarti lebih atas / belakang dalam top-down.
            // bounds.min.y mewakili bagian bawah collider kaki pemain.
            shouldFade = playerCollider.bounds.min.y > treeFootPoint.position.y + 0.02f;
        }

        float multiplier = shouldFade ? hiddenAlphaMultiplier : 1f;
        for (int i = 0; i < treeRenderers.Length; i++)
        {
            SpriteRenderer rend = treeRenderers[i];
            if (rend == null)
                continue;

            Color color = rend.color;
            float desiredAlpha = originalAlphas[i] * multiplier;
            color.a = Mathf.MoveTowards(
                color.a, desiredAlpha, alphaChangePerSecond * Time.deltaTime);
            rend.color = color;
        }
    }

    private void OnDisable()
    {
        playerCollider = null;
        if (originalAlphas == null)
            return;

        for (int i = 0; i < treeRenderers.Length; i++)
        {
            SpriteRenderer rend = treeRenderers[i];
            if (rend == null)
                continue;
            Color color = rend.color;
            color.a = originalAlphas[i];
            rend.color = color;
        }
    }
}
