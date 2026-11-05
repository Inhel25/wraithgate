using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Place on an object in the character selection panel.
/// Hook ChooseMale/ChooseFemale into the card buttons, and Confirm into Confirm button.
/// If you already have a selection script, instead copy the PlayerPrefs lines into
/// its existing confirmation function. Do not register two scene-loading handlers.
/// </summary>
public class CharacterSelectionConfirm : MonoBehaviour
{
    private const string SelectedCharacterKey = "WG_SelectedCharacter";

    [Header("Scene")]
    [SerializeField] private string villageSceneName = "Village";

    [Header("Optional UI")]
    [SerializeField] private Button confirmButton;

    private int selectedCharacter = -1; // 0 = male, 1 = female

    private void Start()
    {
        if (confirmButton != null)
            confirmButton.interactable = false;
    }

    public void ChooseMale()
    {
        SelectCharacter(0);
    }

    public void ChooseFemale()
    {
        SelectCharacter(1);
    }

    private void SelectCharacter(int characterId)
    {
        selectedCharacter = characterId;
        if (confirmButton != null)
            confirmButton.interactable = true;

        Debug.Log(characterId == 0 ? "Male character selected" : "Female character selected");
    }

    public void Confirm()
    {
        if (selectedCharacter < 0)
        {
            Debug.LogWarning("Choose a character before confirming.");
            return;
        }

        PlayerPrefs.SetInt(SelectedCharacterKey, selectedCharacter);
        PlayerPrefs.Save();
        SceneManager.LoadScene(villageSceneName);
    }
}
