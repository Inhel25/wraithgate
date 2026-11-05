using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Mengikuti WorldTimeManager yang sama dengan OutdoorLighting2D.
/// Dipasang pada "08_Lamps_Added" di Village_Decorated_NightLights.
/// Setiap lentera mempunyai Point Light 2D tersendiri, termasuk 4 lentera
/// pada 2 gerbang Village. Tidak menyalakan lampu pada siang hari.
/// </summary>
[DisallowMultipleComponent]
public sealed class VillageNightLights2D : MonoBehaviour
{
    [Header("Lampu Village - jangan ubah urutan tanpa mengubah intensity")]
    [SerializeField] private Light2D[] lampLights;
    [SerializeField] private float[] nightIntensities;

    [Header("Transisi mengikuti OutdoorLighting2D")]
    [Range(0f, 24f)] [SerializeField] private float duskBegins = 18f;
    [Range(0f, 24f)] [SerializeField] private float nightBegins = 20f;
    [Range(0f, 24f)] [SerializeField] private float sunriseBegins = 5f;
    [Range(0f, 24f)] [SerializeField] private float dayBegins = 7f;

    [Header("Pengujian opsional")]
    [Tooltip("Aktifkan hanya untuk melihat cahaya tanpa menunggu malam. Matikan untuk memakai jam game.")]
    [SerializeField] private bool usePreviewHour = false;
    [Range(0f, 24f)] [SerializeField] private float previewHour = 22f;

    private void OnEnable() { ApplyLights(); }
    private void LateUpdate() { ApplyLights(); }

    private void ApplyLights()
    {
        if (lampLights == null) return;
        // Bila Scene sedang tidak punya pengatur waktu, default siang.
        float hour = usePreviewHour ? previewHour :
            (WorldTimeManager.Instance != null ? WorldTimeManager.Instance.CurrentHour : 12f);
        float strength = GetNightStrength(hour);
        for (int i = 0; i < lampLights.Length; i++)
        {
            Light2D light2D = lampLights[i];
            if (light2D == null) continue;
            float target = (nightIntensities != null && i < nightIntensities.Length)
                ? Mathf.Max(0f, nightIntensities[i]) : 1f;
            light2D.intensity = target * strength;
        }
    }

    private float GetNightStrength(float hour)
    {
        hour = Mathf.Repeat(hour, 24f);
        if (hour >= nightBegins || hour < sunriseBegins) return 1f;
        if (hour >= duskBegins && hour < nightBegins)
        {
            float t = Mathf.InverseLerp(duskBegins, nightBegins, hour);
            return Mathf.SmoothStep(0f, 1f, t);
        }
        if (hour >= sunriseBegins && hour < dayBegins)
        {
            float t = Mathf.InverseLerp(sunriseBegins, dayBegins, hour);
            return 1f - Mathf.SmoothStep(0f, 1f, t);
        }
        return 0f;
    }
}
