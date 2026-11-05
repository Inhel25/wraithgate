using UnityEngine;

public class WorldTimeManager : MonoBehaviour
{
    public static WorldTimeManager Instance { get; private set; }

    [Header("WORLD TIME SETTINGS")]
    [Range(0f, 23.99f)]
    [SerializeField]
    private float startingHour = 7f;

    [Min(0.1f)]
    [SerializeField]
    private float fullDayMinutes = 30f;

    [Header("CONTROLS")]
    [SerializeField]
    private bool pauseTime = false;

    [SerializeField]
    private bool showDebugClock = true;

    public float CurrentHour { get; private set; }

    public bool IsNight =>
        CurrentHour >= 20f || CurrentHour < 5f;

    private void Awake()
    {
        // Cegah manager ganda ketika berpindah scene.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Manager tidak hancur ketika scene berganti.
        DontDestroyOnLoad(gameObject);

        CurrentHour = startingHour;
    }

    private void Update()
    {
        if (pauseTime)
            return;

        float fullDaySeconds = fullDayMinutes * 60f;

        // 24 jam game dibagi durasi satu hari nyata.
        float gameHoursPerSecond = 24f / fullDaySeconds;

        CurrentHour +=
            Time.deltaTime * gameHoursPerSecond;

        // Setelah 24:00, kembali ke 00:00.
        CurrentHour = Mathf.Repeat(CurrentHour, 24f);
    }

    public void ResetToMorning()
    {
        CurrentHour = startingHour;
    }

    public void SetPauseTime(bool paused)
    {
        pauseTime = paused;
    }

    private void OnGUI()
    {
        if (!showDebugClock)
            return;

        int hours = Mathf.FloorToInt(CurrentHour);

        int minutes = Mathf.FloorToInt(
            (CurrentHour - hours) * 60f
        );

        string timeText = string.Format(
            "WRAITHGATE\nTime: {0:00}:{1:00}",
            hours,
            minutes
        );

        GUI.Box(
            new Rect(10, 10, 190, 60),
            timeText
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}