using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// WRAITHGATE - MAIN MENU MANAGER
///
/// Menggunakan UI dan PNG yang sudah ada di Hierarchy.
/// Tidak membuat ulang Main Menu, Settings, atau Character Selection.
///
/// NEW GAME  -> Character Selection
/// CONTINUE  -> Save/Load (nanti)
/// SETTINGS  -> Settings Panel
/// QUIT      -> Keluar game
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    // =====================================================
    // PANEL REFERENCES
    // =====================================================

    [Header("PANEL REFERENCES")]

    [Tooltip("Drag MainMenuPanels dari Canvas.")]
    [SerializeField]
    private GameObject mainMenuPanels;

    [Tooltip("Drag SettingsPanel dari Canvas.")]
    [SerializeField]
    private GameObject settingsPanel;

    [Tooltip("Drag CharacterSelectionPanel dari Canvas.")]
    [SerializeField]
    private GameObject characterSelectionPanel;


    // =====================================================
    // CHARACTER SELECTION MANAGER
    // =====================================================

    [Header("CHARACTER SELECTION")]

    [Tooltip("Drag GameObject CharacterSelectionManager dari Hierarchy.")]
    [SerializeField]
    private CharacterSelectionManager characterSelectionManager;


    // =====================================================
    // EXISTING MAIN MENU BUTTONS
    // =====================================================

    [Header("EXISTING MAIN MENU BUTTONS")]

    [SerializeField]
    private Button newGameButton;

    [SerializeField]
    private Button continueButton;

    [SerializeField]
    private Button settingsButton;

    [SerializeField]
    private Button quitButton;


    // =====================================================
    // OPTIONAL BUTTON PNG / SPRITES
    // =====================================================

    [Header("OPTIONAL - BUTTON PNG / SPRITES")]

    [Tooltip("Kosongkan jika PNG tombol sudah benar.")]
    [SerializeField]
    private Sprite buttonNormal;

    [SerializeField]
    private Sprite buttonHighlighted;

    [SerializeField]
    private Sprite buttonPressed;

    [SerializeField]
    private Sprite buttonSelected;

    [SerializeField]
    private Sprite buttonDisabled;


    // =====================================================
    // STARTUP
    // =====================================================

    [Header("STARTUP")]

    [SerializeField]
    private bool showMainMenuOnStart = true;


    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        ConnectButtons();

        ApplyOptionalButtonSprites();

        if (showMainMenuOnStart)
        {
            ShowMainMenu();
        }
    }


    // =====================================================
    // CONNECT BUTTONS
    // =====================================================

    private void ConnectButtons()
    {
        if (newGameButton != null)
        {
            newGameButton.onClick.RemoveListener(NewGame);
            newGameButton.onClick.AddListener(NewGame);
        }
        else
        {
            Debug.LogWarning(
                "WRAITHGATE: NewGame_Button belum dihubungkan."
            );
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(ContinueGame);
            continueButton.onClick.AddListener(ContinueGame);
        }
        else
        {
            Debug.LogWarning(
                "WRAITHGATE: Continue_Button belum dihubungkan."
            );
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(OpenSettings);
            settingsButton.onClick.AddListener(OpenSettings);
        }
        else
        {
            Debug.LogWarning(
                "WRAITHGATE: Settings_Button belum dihubungkan."
            );
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }
        else
        {
            Debug.LogWarning(
                "WRAITHGATE: Quit_Button belum dihubungkan."
            );
        }
    }


    // =====================================================
    // OPTIONAL BUTTON SPRITES
    // =====================================================

    private void ApplyOptionalButtonSprites()
    {
        // Jika tidak ada sprite baru,
        // PNG tombol lama tidak akan diubah.

        if (buttonNormal == null)
            return;

        ConfigureButton(newGameButton);
        ConfigureButton(continueButton);
        ConfigureButton(settingsButton);
        ConfigureButton(quitButton);
    }


    private void ConfigureButton(Button button)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogWarning(
                "WRAITHGATE: " + button.name +
                " tidak memiliki komponen Image."
            );
            return;
        }

        image.sprite = buttonNormal;
        image.color = Color.white;
        image.preserveAspect = true;

        button.targetGraphic = image;
        button.transition =
            Selectable.Transition.SpriteSwap;

        SpriteState spriteState = new SpriteState
        {
            highlightedSprite =
                buttonHighlighted != null
                    ? buttonHighlighted
                    : buttonNormal,

            pressedSprite =
                buttonPressed != null
                    ? buttonPressed
                    : (buttonSelected != null
                        ? buttonSelected
                        : buttonNormal),

            selectedSprite =
                buttonSelected != null
                    ? buttonSelected
                    : buttonNormal,

            disabledSprite =
                buttonDisabled != null
                    ? buttonDisabled
                    : buttonNormal
        };

        button.spriteState = spriteState;
    }


    // =====================================================
    // SHOW MAIN MENU
    // =====================================================

    public void ShowMainMenu()
    {
        if (mainMenuPanels != null)
        {
            mainMenuPanels.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(false);
        }
    }


    // =====================================================
    // NEW GAME
    // =====================================================

    public void NewGame()
    {
        if (characterSelectionManager == null)
        {
            Debug.LogError(
                "WRAITHGATE: CharacterSelectionManager " +
                "belum dihubungkan di Inspector!"
            );
            return;
        }

        // CharacterSelectionManager akan
        // membuka panel pemilihan karakter.

        characterSelectionManager.OpenCharacterSelection();
    }


    // =====================================================
    // CONTINUE GAME
    // =====================================================

    public void ContinueGame()
    {
        Debug.Log(
            "WRAITHGATE: CONTINUE ditekan. " +
            "Sistem Save/Load akan dibuat nanti."
        );
    }


    // =====================================================
    // OPEN SETTINGS
    // =====================================================

    public void OpenSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning(
                "WRAITHGATE: SettingsPanel belum dihubungkan."
            );
            return;
        }

        if (mainMenuPanels != null)
        {
            mainMenuPanels.SetActive(false);
        }

        if (characterSelectionPanel != null)
        {
            characterSelectionPanel.SetActive(false);
        }

        settingsPanel.SetActive(true);
    }


    // =====================================================
    // BACK FROM SETTINGS
    // =====================================================

    public void BackFromSettings()
    {
        ShowMainMenu();
    }


    // =====================================================
    // BACK FROM CHARACTER SELECTION
    // =====================================================

    public void BackFromCharacterSelection()
    {
        ShowMainMenu();
    }


    // =====================================================
    // QUIT GAME
    // =====================================================

    public void QuitGame()
    {
#if UNITY_EDITOR

        Debug.Log(
            "WRAITHGATE: QUIT ditekan. " +
            "Game hanya akan tertutup setelah di-build."
        );

#else

        Application.Quit();

#endif
    }


    // =====================================================
    // CLEANUP
    // =====================================================

    private void OnDestroy()
    {
        if (newGameButton != null)
            newGameButton.onClick.RemoveListener(NewGame);

        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueGame);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OpenSettings);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);
    }
}