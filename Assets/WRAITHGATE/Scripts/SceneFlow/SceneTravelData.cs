using UnityEngine;

/// <summary>
/// Menyimpan tujuan spawn saat berpindah scene.
/// Hanya menyimpan data perjalanan sementara; tidak membuat GameObject.
/// </summary>
public static class SceneTravelData
{
    private static string destinationScene;
    private static string destinationSpawnId;
    private static bool hasPendingTravel;

    // Bersihkan data lama saat memulai sesi Play baru, termasuk bila Domain Reload dimatikan.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        Clear();
    }

    public static void SetDestination(string sceneName, string spawnId)
    {
        destinationScene = sceneName;
        destinationSpawnId = spawnId;
        hasPendingTravel = true;
    }

    public static bool TryConsume(string currentSceneName, out string spawnId)
    {
        spawnId = null;
        if (!hasPendingTravel || destinationScene != currentSceneName)
            return false;

        spawnId = destinationSpawnId;
        Clear();
        return true;
    }

    public static void Clear()
    {
        destinationScene = null;
        destinationSpawnId = null;
        hasPendingTravel = false;
    }
}
