using UnityEngine;

// WRAITHGATE Dungeon - Simple pixel-art flowing water animation (Unity 6 compatible).
// Each waterfall segment owns an independent looping sprite sequence.
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class WraithgateWaterfallFlow : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Range(1f, 30f)] private float framesPerSecond = 8f;
    [SerializeField] private bool animateUnscaledTime = false;

    private SpriteRenderer spriteRenderer;
    private float elapsed;
    private int currentFrame;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        elapsed = 0f;
        currentFrame = 0;
        if (frames != null && frames.Length > 0 && frames[0] != null)
            spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length <= 1) return;
        elapsed += animateUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float interval = 1f / Mathf.Max(1f, framesPerSecond);
        if (elapsed < interval) return;
        int steps = Mathf.FloorToInt(elapsed / interval);
        elapsed -= steps * interval;
        currentFrame = (currentFrame + steps) % frames.Length;
        if (frames[currentFrame] != null) spriteRenderer.sprite = frames[currentFrame];
    }
}
