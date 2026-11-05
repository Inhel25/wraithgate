using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Satu spawner yang dipakai di Village, Forest, dan zona lain.
/// Menggantikan komponen VillagePlayerSpawner lama agar player tidak muncul dua kali.
/// </summary>
public class ZonePlayerSpawner2D : MonoBehaviour
{
    [Serializable]
    public class ArrivalPoint
    {
        [Tooltip("ID yang dikirim oleh ZoneTransition2D, contoh: FromForest")]
        public string spawnId;

        [Tooltip("Empty GameObject tempat pemain muncul")]
        public Transform spawnPoint;
    }

    [Header("PLAYER PREFABS")]
    [SerializeField] private GameObject malePlayerPrefab;
    [SerializeField] private GameObject femalePlayerPrefab;

    [Header("SPAWN DEFAULT (NEW GAME / TEST LANGSUNG)")]
    [SerializeField] private Transform defaultSpawnPoint;

    [Header("SPAWN KHUSUS DARI SCENE LAIN")]
    [SerializeField] private ArrivalPoint[] arrivalPoints = new ArrivalPoint[0];

    private const string CharacterKey = "WRAITHGATE_SelectedCharacter";
    private const string LegacyCharacterKey = "WG_SelectedCharacter";

    private void Start()
    {
        if (malePlayerPrefab == null || femalePlayerPrefab == null || defaultSpawnPoint == null)
        {
            Debug.LogError("[ZonePlayerSpawner2D] Isi Male Player Prefab, Female Player Prefab, dan Default Spawn Point!", this);
            return;
        }

        Transform spawnPoint = ChooseSpawnPoint();
        int characterId = PlayerPrefs.HasKey(CharacterKey)
            ? PlayerPrefs.GetInt(CharacterKey)
            : PlayerPrefs.GetInt(LegacyCharacterKey, 0);

        GameObject prefab;
        if (characterId == 0)
            prefab = malePlayerPrefab;
        else if (characterId == 1)
            prefab = femalePlayerPrefab;
        else
        {
            Debug.LogWarning("[ZonePlayerSpawner2D] ID karakter tidak valid, menggunakan Male untuk pengujian.", this);
            characterId = 0;
            prefab = malePlayerPrefab;
        }

        GameObject player = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        player.name = characterId == 0 ? "Player_Male" : "Player_Female";

        // Cocok dengan CameraFollow2D yang sudah dipakai di WRAITHGATE.
        if (Camera.main != null)
        {
            CameraFollow2D follow = Camera.main.GetComponent<CameraFollow2D>();
            if (follow != null)
                follow.SetTarget(player.transform);
        }
        else
        {
            Debug.LogWarning("[ZonePlayerSpawner2D] Main Camera tidak ditemukan. Periksa Tag MainCamera.", this);
        }

        Debug.Log("[ZonePlayerSpawner2D] " + player.name + " muncul di " +
                  spawnPoint.name + " pada scene " + SceneManager.GetActiveScene().name + ".", this);
    }

    private Transform ChooseSpawnPoint()
    {
        string spawnId;
        if (!SceneTravelData.TryConsume(SceneManager.GetActiveScene().name, out spawnId))
            return defaultSpawnPoint;

        foreach (ArrivalPoint entry in arrivalPoints)
        {
            if (entry != null && entry.spawnPoint != null &&
                string.Equals(entry.spawnId, spawnId, StringComparison.Ordinal))
                return entry.spawnPoint;
        }

        Debug.LogWarning("[ZonePlayerSpawner2D] Spawn ID '" + spawnId +
                         "' tidak ditemukan; menggunakan Default Spawn Point.", this);
        return defaultSpawnPoint;
    }
}
