using UnityEngine;

// Uses the original 16 frames supplied with WRAITHGATE_Dungeon_Portal_Animated_Unity.zip.
// Does not use an imported Animator Controller, so the portal works immediately after importing.
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class WraithgatePortalSpriteLoop : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 12f;
    [SerializeField] private float timeOffset = 0f;
    private void OnEnable() { if (!target) target = GetComponent<SpriteRenderer>(); ApplyFrame(); }
    private void Update() { ApplyFrame(); }
    private void ApplyFrame()
    {
        if (!target || frames == null || frames.Length == 0) return;
        float t = Application.isPlaying ? Time.realtimeSinceStartup : Time.realtimeSinceStartup;
        int index = Mathf.FloorToInt((t + timeOffset) * Mathf.Max(1f, framesPerSecond)) % frames.Length;
        if (index < 0) index += frames.Length;
        if (target.sprite != frames[index]) target.sprite = frames[index];
    }
}
