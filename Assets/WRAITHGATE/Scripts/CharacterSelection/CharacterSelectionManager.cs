using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// WRAITHGATE Character Selection.
/// Memakai UI yang sudah ada tanpa mengganti panel atau sprite PNG.
/// Pasang pada GameObject aktif bernama CharacterSelectionManager.
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    public enum CharacterChoice { None = -1, Male = 0, Female = 1 }

    [System.Serializable]
    public class CharacterCard
    {
        public Button button;
        public Image cardImage;
        public Image characterPreview;
        public TMP_Text characterName;
        public GameObject sparkleLarge;
        public GameObject sparkleSmall;
        public Sprite front;
        public Sprite left;
        public Sprite back;
        public Sprite right;
        public bool unlocked = true;

        [System.NonSerialized] public bool hovering;
        [System.NonSerialized] public int directionIndex;
        [System.NonSerialized] public float directionTimer;

        public Sprite GetDirection(int index)
        {
            switch (index)
            {
                case 0: return front;
                case 1: return left;
                case 2: return back;
                default: return right;
            }
        }
    }

    [Header("EXISTING PANELS")]
    [SerializeField] private GameObject mainMenuPanels;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject characterSelectionPanel;

    [Header("EXISTING CARDS")]
    [SerializeField] private CharacterCard maleCard = new CharacterCard();
    [SerializeField] private CharacterCard femaleCard = new CharacterCard();

    [Header("CARD PNG SPRITES")]
    [SerializeField] private Sprite cardNormal;
    [SerializeField] private Sprite cardHover;
    [SerializeField] private Sprite cardSelected;
    [SerializeField] private Sprite cardDisabled;

    [Header("EXISTING BUTTONS")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button confirmButton;

    [Header("PREVIEW")]
    [Min(0.1f)] [SerializeField] private float secondsPerDirection = 0.7f;
    [SerializeField] private bool rotateWhileHovering = true;

    [Header("CARD TEXT COLORS")]
    [SerializeField] private Color normalNameColor = new Color32(23, 35, 51, 255);
    [SerializeField] private Color hoverNameColor = new Color32(242, 229, 196, 255);
    [SerializeField] private Color selectedNameColor = new Color32(23, 35, 51, 255);
    [SerializeField] private Color disabledNameColor = new Color32(110, 110, 112, 255);

    [Header("SCENE")]
    [SerializeField] private string newGameSceneName = "Village";

    [Header("STARTUP")]
    [SerializeField] private bool hideSelectionOnStart = true;
    [SerializeField] private bool startWithNoSelection = true;

    // Kunci utama yang digunakan sistem Character Selection.
    public const string SavedCharacterKey = "WRAITHGATE_SelectedCharacter";

    // Kunci kompatibilitas untuk VillagePlayerSpawner yang dibuat sebelumnya.
    // Setelah spawner diubah memakai SavedCharacterKey, baris ini dapat dihapus.
    private const string PreviousSpawnerKey = "WG_SelectedCharacter";

    public CharacterChoice SelectedCharacter { get; private set; } = CharacterChoice.None;

    private bool sceneLoadRequested;

    private void Awake()
    {
        ConfigureCard(maleCard, CharacterChoice.Male);
        ConfigureCard(femaleCard, CharacterChoice.Female);

        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmSelection);

        SelectedCharacter = startWithNoSelection
            ? CharacterChoice.None
            : GetSavedCharacter();

        if (SelectedCharacter == CharacterChoice.Male && !maleCard.unlocked)
            SelectedCharacter = CharacterChoice.None;
        if (SelectedCharacter == CharacterChoice.Female && !femaleCard.unlocked)
            SelectedCharacter = CharacterChoice.None;

        RefreshAllVisuals();

        if (hideSelectionOnStart && characterSelectionPanel != null)
            characterSelectionPanel.SetActive(false);
    }

    private void Update()
    {
        if (characterSelectionPanel == null || !characterSelectionPanel.activeInHierarchy)
            return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            BackToMainMenu();
            return;
        }

        if (rotateWhileHovering)
        {
            UpdatePreview(maleCard);
            UpdatePreview(femaleCard);
        }
    }

    private void ConfigureCard(CharacterCard card, CharacterChoice choice)
    {
        if (card == null) return;

        if (card.button != null)
        {
            card.button.transition = Selectable.Transition.None;

            // Metode bernama memudahkan pelepasan listener saat objek dihancurkan.
            if (choice == CharacterChoice.Male)
                card.button.onClick.AddListener(SelectMale);
            else if (choice == CharacterChoice.Female)
                card.button.onClick.AddListener(SelectFemale);

            CharacterCardPointer pointer =
                card.button.GetComponent<CharacterCardPointer>();

            if (pointer == null)
                pointer = card.button.gameObject.AddComponent<CharacterCardPointer>();

            pointer.Initialize(this, choice);
        }

        if (card.characterPreview != null)
        {
            card.characterPreview.preserveAspect = true;
            card.characterPreview.raycastTarget = false;
        }

        if (card.characterName != null)
            card.characterName.raycastTarget = false;

        if (card.cardImage != null)
            card.cardImage.raycastTarget = true;

        ResetPreview(card);
    }

    public void OpenCharacterSelection()
    {
        if (characterSelectionPanel == null)
        {
            Debug.LogError("[CharacterSelectionManager] Character Selection Panel belum diisi di Inspector.");
            return;
        }

        if (mainMenuPanels != null) mainMenuPanels.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        characterSelectionPanel.SetActive(true);
        SelectedCharacter = CharacterChoice.None;

        maleCard.hovering = false;
        femaleCard.hovering = false;
        ResetPreview(maleCard);
        ResetPreview(femaleCard);
        RefreshAllVisuals();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void BackToMainMenu()
    {
        if (sceneLoadRequested) return;

        if (characterSelectionPanel != null) characterSelectionPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanels != null) mainMenuPanels.SetActive(true);

        maleCard.hovering = false;
        femaleCard.hovering = false;
        ResetPreview(maleCard);
        ResetPreview(femaleCard);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public void SelectMale() => SelectCharacter(CharacterChoice.Male);
    public void SelectFemale() => SelectCharacter(CharacterChoice.Female);

    public void SelectCharacter(CharacterChoice choice)
    {
        if (sceneLoadRequested) return;
        if (choice == CharacterChoice.Male && !maleCard.unlocked) return;
        if (choice == CharacterChoice.Female && !femaleCard.unlocked) return;
        if (choice == CharacterChoice.None) return;

        SelectedCharacter = choice;
        RefreshAllVisuals();
    }

    public void ConfirmSelection()
    {
        if (sceneLoadRequested) return;

        if (SelectedCharacter == CharacterChoice.None)
        {
            Debug.LogWarning("[CharacterSelectionManager] Pilih karakter sebelum CONFIRM.");
            return;
        }

        if (string.IsNullOrWhiteSpace(newGameSceneName))
        {
            Debug.LogError("[CharacterSelectionManager] New Game Scene Name belum diisi.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(newGameSceneName))
        {
            Debug.LogError("[CharacterSelectionManager] Scene '" + newGameSceneName +
                           "' belum tersedia di Build Profiles > Scene List / Build Settings.");
            return;
        }

        // Simpan ID: Male = 0, Female = 1.
        int characterId = (int)SelectedCharacter;
        PlayerPrefs.SetInt(SavedCharacterKey, characterId);

        // Kompatibel dengan spawner lama yang membaca WG_SelectedCharacter.
        PlayerPrefs.SetInt(PreviousSpawnerKey, characterId);
        PlayerPrefs.Save();

        // NEW GAME: jika pengatur waktu masih hidup dari sesi game sebelumnya,
        // mulai kembali dari jam pagi yang ditentukan pada WorldTimeManager.
        // Jika ini New Game pertama dan manager belum ada, Scene Village
        // akan membuat WorldTimeManager baru dengan startingHour = 7.
        if (WorldTimeManager.Instance != null)
        {
            WorldTimeManager.Instance.ResetToMorning();
            Debug.Log("[CharacterSelectionManager] Waktu dunia di-reset ke pagi untuk New Game.");
        }

        Debug.Log("[CharacterSelectionManager] Karakter tersimpan: " +
                  SelectedCharacter + " (ID " + characterId + "). Membuka " +
                  newGameSceneName + ".");

        sceneLoadRequested = true;
        if (confirmButton != null)
            confirmButton.interactable = false;

        SceneManager.LoadScene(newGameSceneName);
    }

    public static CharacterChoice GetSavedCharacter()
    {
        int saved = PlayerPrefs.HasKey(SavedCharacterKey)
            ? PlayerPrefs.GetInt(SavedCharacterKey, -1)
            : PlayerPrefs.GetInt(PreviousSpawnerKey, -1);

        return saved == 0 ? CharacterChoice.Male :
               saved == 1 ? CharacterChoice.Female :
               CharacterChoice.None;
    }

    public void SetMaleUnlocked(bool unlocked)
    {
        maleCard.unlocked = unlocked;
        if (!unlocked && SelectedCharacter == CharacterChoice.Male)
            SelectedCharacter = CharacterChoice.None;
        RefreshAllVisuals();
    }

    public void SetFemaleUnlocked(bool unlocked)
    {
        femaleCard.unlocked = unlocked;
        if (!unlocked && SelectedCharacter == CharacterChoice.Female)
            SelectedCharacter = CharacterChoice.None;
        RefreshAllVisuals();
    }

    internal void OnCardPointerEnter(CharacterChoice choice)
    {
        CharacterCard card = GetCard(choice);
        if (card == null || !card.unlocked) return;

        card.hovering = true;
        card.directionTimer = 0f;
        RefreshCard(card, choice);
    }

    internal void OnCardPointerExit(CharacterChoice choice)
    {
        CharacterCard card = GetCard(choice);
        if (card == null) return;

        card.hovering = false;
        ResetPreview(card);
        RefreshCard(card, choice);
    }

    private CharacterCard GetCard(CharacterChoice choice)
    {
        return choice == CharacterChoice.Male ? maleCard :
               choice == CharacterChoice.Female ? femaleCard : null;
    }

    private void UpdatePreview(CharacterCard card)
    {
        if (card == null || !card.unlocked || !card.hovering ||
            card.characterPreview == null)
            return;

        card.directionTimer += Time.unscaledDeltaTime;
        if (card.directionTimer < secondsPerDirection) return;

        card.directionTimer = 0f;
        card.directionIndex = (card.directionIndex + 1) % 4;
        Sprite next = card.GetDirection(card.directionIndex);

        if (next != null)
            card.characterPreview.sprite = next;
    }

    private void ResetPreview(CharacterCard card)
    {
        if (card == null) return;

        card.directionIndex = 0;
        card.directionTimer = 0f;

        if (card.characterPreview != null && card.front != null)
            card.characterPreview.sprite = card.front;
    }

    private void RefreshAllVisuals()
    {
        RefreshCard(maleCard, CharacterChoice.Male);
        RefreshCard(femaleCard, CharacterChoice.Female);

        if (confirmButton != null)
            confirmButton.interactable =
                SelectedCharacter != CharacterChoice.None && !sceneLoadRequested;
    }

    private void RefreshCard(CharacterCard card, CharacterChoice choice)
    {
        if (card == null) return;

        bool selected = SelectedCharacter == choice;
        Sprite desired = !card.unlocked ? cardDisabled :
                         selected ? cardSelected :
                         card.hovering ? cardHover : cardNormal;

        if (card.cardImage != null)
        {
            if (desired != null)
                card.cardImage.sprite = desired;
            card.cardImage.color = Color.white;
        }

        if (card.button != null)
            card.button.interactable = card.unlocked && !sceneLoadRequested;

        if (card.characterName != null)
            card.characterName.color = !card.unlocked ? disabledNameColor :
                                       selected ? selectedNameColor :
                                       card.hovering ? hoverNameColor : normalNameColor;

        if (card.sparkleLarge != null)
            card.sparkleLarge.SetActive(selected && card.unlocked);

        if (card.sparkleSmall != null)
            card.sparkleSmall.SetActive(selected && card.unlocked);
    }

    private void OnDestroy()
    {
        if (maleCard != null && maleCard.button != null)
            maleCard.button.onClick.RemoveListener(SelectMale);

        if (femaleCard != null && femaleCard.button != null)
            femaleCard.button.onClick.RemoveListener(SelectFemale);

        if (backButton != null)
            backButton.onClick.RemoveListener(BackToMainMenu);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(ConfirmSelection);
    }
}

/// <summary>
/// Pointer events untuk kartu karakter. Ditambahkan otomatis ke Button.
/// </summary>
public class CharacterCardPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CharacterSelectionManager manager;
    private CharacterSelectionManager.CharacterChoice choice;

    public void Initialize(
        CharacterSelectionManager owner,
        CharacterSelectionManager.CharacterChoice character)
    {
        manager = owner;
        choice = character;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (manager != null)
            manager.OnCardPointerEnter(choice);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (manager != null)
            manager.OnCardPointerExit(choice);
    }
}
