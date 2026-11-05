using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Pasang pada Empty GameObject yang memiliki BoxCollider2D (Is Trigger = true).
/// Secara otomatis memuat scene tujuan saat player masuk area trigger.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ZoneTransition2D : MonoBehaviour
{
    [Header("TUJUAN")]
    [SerializeField] private string destinationSceneName = "Forest";

    [Tooltip("Harus sama persis dengan Spawn ID pada ZonePlayerSpawner2D di scene tujuan")]
    [SerializeField] private string destinationSpawnId = "FromVillage";

    private bool isTransitioning;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
            trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning)
            return;

        // Hanya karakter pemain, bukan NPC atau collider pohon.
        PlayerMovement2D player = other.GetComponentInParent<PlayerMovement2D>();
        if (player == null)
            return;

        if (string.IsNullOrWhiteSpace(destinationSceneName))
        {
            Debug.LogError("[ZoneTransition2D] Destination Scene Name belum diisi.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(destinationSceneName))
        {
            Debug.LogError("[ZoneTransition2D] Scene '" + destinationSceneName +
                           "' belum ada atau belum aktif di Build Profiles > Scene List.", this);
            return;
        }

        isTransitioning = true;
        SceneTravelData.SetDestination(destinationSceneName, destinationSpawnId);
        Debug.Log("[ZoneTransition2D] Berpindah ke " + destinationSceneName +
                  ", titik spawn: " + destinationSpawnId + ".", this);
        SceneManager.LoadScene(destinationSceneName);
    }
}
