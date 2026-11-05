using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Mengurutkan Sprite berdasarkan posisi kaki (Y) untuk game top-down 2D.
/// Pasang pada root Player / Tree / NPC. Pastikan semuanya menggunakan
/// Sorting Layer yang sama (misalnya World).
/// </summary>
[DisallowMultipleComponent]
public class YSort2D : MonoBehaviour
{
    [Header("Renderer yang akan diurutkan")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [SerializeField] private SortingGroup targetSortingGroup;

    [Header("Titik kaki / dasar objek")]
    [SerializeField] private Transform feetPoint;

    [Header("Sorting")]
    [Min(1)]
    [SerializeField] private int pixelsPerUnitSorting = 100;
    [SerializeField] private int orderOffset = 0;

    private void Awake()
    {
        if (targetSortingGroup == null)
            targetSortingGroup = GetComponent<SortingGroup>();

        if (targetSpriteRenderer == null && targetSortingGroup == null)
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (feetPoint == null)
            feetPoint = transform;
    }

    private void LateUpdate()
    {
        float y = feetPoint != null ? feetPoint.position.y : transform.position.y;
        int order = Mathf.Clamp(
            -Mathf.RoundToInt(y * pixelsPerUnitSorting) + orderOffset,
            -32768, 32767);

        if (targetSortingGroup != null)
            targetSortingGroup.sortingOrder = order;
        else if (targetSpriteRenderer != null)
            targetSpriteRenderer.sortingOrder = order;
    }
}
