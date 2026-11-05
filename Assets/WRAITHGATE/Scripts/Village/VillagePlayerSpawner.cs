using UnityEngine;

/// <summary>
/// Place on PlayerSystem/VillagePlayerSpawner in Village scene.
/// Assign the two player prefab assets and a SpawnPoint transform via Inspector.
/// </summary>
public class VillagePlayerSpawner : MonoBehaviour
{
    private const string SelectedCharacterKey = "WG_SelectedCharacter";

    [Header("Player Prefabs")]
    [SerializeField] private GameObject malePlayerPrefab;
    [SerializeField] private GameObject femalePlayerPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        if (malePlayerPrefab == null || femalePlayerPrefab == null || spawnPoint == null)
        {
            Debug.LogError("VillagePlayerSpawner: assign both prefabs and Spawn Point in Inspector.", this);
            return;
        }

        if (!PlayerPrefs.HasKey(SelectedCharacterKey))
            Debug.LogWarning("No character selection found. Defaulting to Male for direct Village testing.");

        int characterId = PlayerPrefs.GetInt(SelectedCharacterKey, 0);
        GameObject chosenPrefab;

        switch (characterId)
        {
            case 0:
                chosenPrefab = malePlayerPrefab;
                break;
            case 1:
                chosenPrefab = femalePlayerPrefab;
                break;
            default:
                Debug.LogError("Unknown character ID: " + characterId);
                return;
        }

        GameObject player = Instantiate(chosenPrefab, spawnPoint.position, Quaternion.identity);
        player.name = characterId == 0 ? "Player_Male" : "Player_Female";

        // Optional: works if the scene uses the simple CameraFollow2D script.
        if (Camera.main != null)
        {
            CameraFollow2D cameraFollow = Camera.main.GetComponent<CameraFollow2D>();
            if (cameraFollow != null)
                cameraFollow.SetTarget(player.transform);
        }
    }
}
