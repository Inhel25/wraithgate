using UnityEngine;
using UnityEngine.Rendering.Universal;

public class OutdoorLighting2D : MonoBehaviour
{
    [Header("GLOBAL LIGHT")]
    [SerializeField]
    private Light2D globalLight;

    [Header("MORNING LIGHT")]
    [SerializeField]
    private Color morningColor =
        new Color(1f, 0.85f, 0.68f);

    [SerializeField]
    private float morningIntensity = 0.85f;

    [Header("NOON LIGHT")]
    [SerializeField]
    private Color noonColor = Color.white;

    [SerializeField]
    private float noonIntensity = 1f;

    [Header("EVENING LIGHT")]
    [SerializeField]
    private Color eveningColor =
        new Color(1f, 0.62f, 0.42f);

    [SerializeField]
    private float eveningIntensity = 0.7f;

    [Header("NIGHT LIGHT")]
    [SerializeField]
    private Color nightColor =
        new Color(0.38f, 0.46f, 0.67f);

    [SerializeField]
    private float nightIntensity = 0.3f;

    private void Start()
    {
        if (globalLight == null)
        {
            Debug.LogError(
                "[OutdoorLighting2D] Global Light belum diisi!",
                this
            );
        }

        UpdateLighting();
    }

    private void LateUpdate()
    {
        UpdateLighting();
    }

    private void UpdateLighting()
    {
        if (globalLight == null)
            return;

        // Nilai default ketika pengatur waktu
        // belum tersedia.
        float hour = 7f;

        if (WorldTimeManager.Instance != null)
        {
            hour = WorldTimeManager.Instance.CurrentHour;
        }

        // MALAM (20.00 - 05.00)
        if (hour >= 20f || hour < 5f)
        {
            ApplyLight(nightColor, nightIntensity);
        }

        // MALAM MENUJU PAGI (05.00 - 07.00)
        else if (hour < 7f)
        {
            BlendLight(
                hour, 5f, 7f,
                nightColor, morningColor,
                nightIntensity, morningIntensity
            );
        }

        // PAGI MENUJU SIANG (07.00 - 10.00)
        else if (hour < 10f)
        {
            BlendLight(
                hour, 7f, 10f,
                morningColor, noonColor,
                morningIntensity, noonIntensity
            );
        }

        // SIANG CERAH (10.00 - 16.00)
        else if (hour < 16f)
        {
            ApplyLight(noonColor, noonIntensity);
        }

        // SIANG MENUJU SORE (16.00 - 18.00)
        else if (hour < 18f)
        {
            BlendLight(
                hour, 16f, 18f,
                noonColor, eveningColor,
                noonIntensity, eveningIntensity
            );
        }

        // SORE MENUJU MALAM (18.00 - 20.00)
        else
        {
            BlendLight(
                hour, 18f, 20f,
                eveningColor, nightColor,
                eveningIntensity, nightIntensity
            );
        }
    }

    private void ApplyLight(
        Color color,
        float intensity)
    {
        globalLight.color = color;
        globalLight.intensity = intensity;
    }

    private void BlendLight(
        float hour,
        float startHour,
        float endHour,
        Color startColor,
        Color endColor,
        float startIntensity,
        float endIntensity)
    {
        float t = Mathf.InverseLerp(
            startHour,
            endHour,
            hour
        );

        // Transisi halus, tidak mendadak.
        t = Mathf.SmoothStep(0f, 1f, t);

        Color newColor = Color.Lerp(
            startColor,
            endColor,
            t
        );

        float newIntensity = Mathf.Lerp(
            startIntensity,
            endIntensity,
            t
        );

        ApplyLight(newColor, newIntensity);
    }
}