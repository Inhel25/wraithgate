using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// WRAITHGATE - SETTINGS MANAGER
///
/// Cocok dengan MainMenuManager.cs final.
/// - TIDAK membuat Settings_Frame atau Tabs baru.
/// - Menggunakan SettingsPanel, Settings_Frame, dan 4 tab yang SUDAH ADA.
/// - Bisa membuat isi General / Graphics / Audio / Controls otomatis.
/// - Menggunakan sprite WRAITHGATE yang di-drag sekali melalui Inspector.
/// - BACK terhubung ke MainMenuManager.BackFromSettings().
/// - Kompatibel dengan New Input System.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("MAIN MENU CONNECTION")]
    [SerializeField] private MainMenuManager mainMenuManager;

    [Header("EXISTING SETTINGS OBJECTS")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private RectTransform settingsFrame;
    [SerializeField] private Transform tabsRoot;

    [Header("EXISTING TAB BUTTONS")]
    [SerializeField] private Button generalButton;
    [SerializeField] private Button graphicsButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private Button controlsButton;

    [Header("TAB SPRITES")]
    [SerializeField] private Sprite tabNormal;
    [SerializeField] private Sprite tabHighlighted;
    [SerializeField] private Sprite tabSelected;
    [SerializeField] private Sprite tabDisabled;

    [Header("TAB ICONS")]
    [SerializeField] private Sprite iconGeneral;
    [SerializeField] private Sprite iconGraphics;
    [SerializeField] private Sprite iconAudio;
    [SerializeField] private Sprite iconControls;

    [Header("SMALL BUTTON SPRITES")]
    [SerializeField] private Sprite buttonSmallNormal;
    [SerializeField] private Sprite buttonSmallHighlighted;
    [SerializeField] private Sprite buttonSmallPressed;
    [SerializeField] private Sprite buttonSmallSelected;
    [SerializeField] private Sprite buttonSmallDisabled;

    [Header("SLIDER SPRITES")]
    [SerializeField] private Sprite sliderBackground;
    [SerializeField] private Sprite sliderFill;
    [SerializeField] private Sprite sliderHandleNormal;
    [SerializeField] private Sprite sliderHandleHighlighted;
    [SerializeField] private Sprite sliderHandleSelected;
    [SerializeField] private Sprite sliderHandleDisabled;

    [Header("TOGGLE SPRITES")]
    [SerializeField] private Sprite toggleOffNormal;
    [SerializeField] private Sprite toggleOnNormal;

    [Header("TEXT")]
    [SerializeField] private TMP_FontAsset pixelFont;
    [SerializeField] private Color textColor = new Color(0.95f, 0.90f, 0.78f, 1f);

    [Header("AUTO BUILD")]
    [Tooltip("ON = isi General/Graphics/Audio/Controls dibuat otomatis saat Play.")]
    [SerializeField] private bool buildSettingsContentAutomatically = true;

    [Header("TAB TEXT COLORS")]
    [SerializeField] private Color tabNormalTextColor =
        new Color(0.95f, 0.90f, 0.78f, 1f); // Cream #F2E5C7
    [SerializeField] private Color tabSelectedTextColor =
        new Color(0.09f, 0.14f, 0.20f, 1f); // Dark Navy #172333

    // Generated content
    private GameObject contentRoot;
    private GameObject generalPanel;
    private GameObject graphicsPanel;
    private GameObject audioPanel;
    private GameObject controlsPanel;

    // Tab Images
    private Image generalTabImage;
    private Image graphicsTabImage;
    private Image audioTabImage;
    private Image controlsTabImage;

    // General
    private TMP_Dropdown languageDropdown;
    private Slider dialogueSpeedSlider;
    private Slider uiScaleSlider;
    private Toggle screenShakeToggle;
    private Toggle tutorialToggle;

    // Graphics
    private TMP_Dropdown resolutionDropdown;
    private TMP_Dropdown qualityDropdown;
    private Toggle fullscreenToggle;
    private Toggle vsyncToggle;

    // Audio
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private TMP_Text masterPercent;
    private TMP_Text musicPercent;
    private TMP_Text sfxPercent;

    // Bottom
    private Button resetButton;
    private Button applyButton;
    private Button backButton;

    private readonly List<Resolution> availableResolutions = new List<Resolution>();
    private bool setupComplete;

    private readonly Color fallbackNavy = new Color(0.035f, 0.07f, 0.11f, 1f);
    private readonly Color fallbackBlue = new Color(0.08f, 0.15f, 0.22f, 1f);
    private readonly Color fallbackGold = new Color(0.85f, 0.72f, 0.43f, 1f);

    private const string KEY_LANGUAGE = "WG_Language";
    private const string KEY_DIALOGUE = "WG_DialogueSpeed";
    private const string KEY_UI_SCALE = "WG_UIScale";
    private const string KEY_SHAKE = "WG_ScreenShake";
    private const string KEY_TUTORIAL = "WG_Tutorial";
    private const string KEY_RESOLUTION = "WG_Resolution";
    private const string KEY_QUALITY = "WG_Quality";
    private const string KEY_FULLSCREEN = "WG_Fullscreen";
    private const string KEY_VSYNC = "WG_VSync";
    private const string KEY_MASTER = "WG_MasterVolume";
    private const string KEY_MUSIC = "WG_MusicVolume";
    private const string KEY_SFX = "WG_SFXVolume";

    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        if (setupComplete)
            return;

        if (settingsPanel == null)
        {
            Debug.LogError("WRAITHGATE SETTINGS: SettingsPanel belum diisi pada Inspector.");
            return;
        }

        FindExistingTabs();

        if (buildSettingsContentAutomatically)
            BuildSettingsContent();

        ConnectTabs();
        ConfigureTabs();

        if (generalPanel != null)
        {
            SetupResolutionOptions();
            SetupQualityOptions();
            LoadSettings();
            ShowTab(0);
        }

        // MainMenuManager akan membuka Settings ketika tombol SETTINGS ditekan.
        settingsPanel.SetActive(false);
        setupComplete = true;
    }

    // =========================================================
    // FIND EXISTING SETTINGS UI
    // =========================================================

    private void FindExistingTabs()
    {
        if (tabsRoot == null)
        {
            Transform found = settingsPanel.transform.Find("Tabs");
            if (found != null)
                tabsRoot = found;
        }

        if (tabsRoot == null)
        {
            Debug.LogWarning("WRAITHGATE SETTINGS: Tabs belum dihubungkan.");
            return;
        }

        if (generalButton == null)
            generalButton = FindButton(tabsRoot, "General_Button");

        if (graphicsButton == null)
            graphicsButton = FindButton(tabsRoot, "Graphics_Button");

        if (audioButton == null)
            audioButton = FindButton(tabsRoot, "Audio_Button");

        if (controlsButton == null)
            controlsButton = FindButton(tabsRoot, "Controls_Button");

        generalTabImage = GetButtonImage(generalButton);
        graphicsTabImage = GetButtonImage(graphicsButton);
        audioTabImage = GetButtonImage(audioButton);
        controlsTabImage = GetButtonImage(controlsButton);

        SetupTabIcon(generalButton, iconGeneral);
        SetupTabIcon(graphicsButton, iconGraphics);
        SetupTabIcon(audioButton, iconAudio);
        SetupTabIcon(controlsButton, iconControls);
    }

    private Button FindButton(Transform root, string objectName)
    {
        Transform target = root.Find(objectName);
        return target != null ? target.GetComponent<Button>() : null;
    }

    private Image GetButtonImage(Button button)
    {
        return button != null ? button.GetComponent<Image>() : null;
    }

    private void SetupTabIcon(Button button, Sprite sprite)
    {
        if (button == null || sprite == null)
            return;

        Transform iconTransform = button.transform.Find("Icon");
        Image image;

        if (iconTransform == null)
        {
            GameObject iconObject = CreateUIObject("Icon", button.transform);
            image = iconObject.AddComponent<Image>();

            RectTransform rt = iconObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(28f, 28f);
            rt.anchoredPosition = new Vector2(42f, 0f);
        }
        else
        {
            image = iconTransform.GetComponent<Image>();
            if (image == null)
                image = iconTransform.gameObject.AddComponent<Image>();
        }

        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    // =========================================================
    // TAB BUTTONS
    // =========================================================

    private void ConnectTabs()
    {
        ConnectButton(generalButton, ShowGeneral);
        ConnectButton(graphicsButton, ShowGraphics);
        ConnectButton(audioButton, ShowAudio);
        ConnectButton(controlsButton, ShowControls);
    }

    private void ConnectButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void ConfigureTabs()
    {
        ConfigureTab(generalButton);
        ConfigureTab(graphicsButton);
        ConfigureTab(audioButton);
        ConfigureTab(controlsButton);
    }

    private void ConfigureTab(Button button)
    {
        if (button == null || tabNormal == null)
            return;

        Image image = button.GetComponent<Image>();
        if (image == null)
            return;

        image.sprite = tabNormal;
        image.color = Color.white;

        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;

        button.spriteState = new SpriteState
        {
            highlightedSprite = tabHighlighted != null ? tabHighlighted : tabNormal,
            pressedSprite = tabSelected != null ? tabSelected : tabNormal,
            selectedSprite = tabSelected != null ? tabSelected : tabNormal,
            disabledSprite = tabDisabled != null ? tabDisabled : tabNormal
        };
    }

    // =========================================================
    // AUTO BUILD CONTENT
    // =========================================================

    private void BuildSettingsContent()
    {
        Transform oldRoot = settingsPanel.transform.Find("Settings_Content");

        if (oldRoot != null)
            Destroy(oldRoot.gameObject);

        contentRoot = CreateUIObject("Settings_Content", settingsPanel.transform);
        Stretch(contentRoot.GetComponent<RectTransform>());

        // Content area assumes your existing Settings_Frame is ~1500 x 820.
        // X is shifted right because tabs are on the left.
        generalPanel = CreateContentPanel("GeneralPanel");
        graphicsPanel = CreateContentPanel("GraphicsPanel");
        audioPanel = CreateContentPanel("AudioPanel");
        controlsPanel = CreateContentPanel("ControlsPanel");

        BuildGeneralPanel();
        BuildGraphicsPanel();
        BuildAudioPanel();
        BuildControlsPanel();
        BuildBottomButtons();
    }

    private GameObject CreateContentPanel(string objectName)
    {
        GameObject panel = CreateUIObject(objectName, contentRoot.transform);
        SetRect(panel.GetComponent<RectTransform>(), 800f, 480f, 245f, 20f);
        return panel;
    }

    // =========================================================
    // GENERAL
    // =========================================================

    private void BuildGeneralPanel()
    {
        CreateSectionTitle(generalPanel.transform, "GENERAL", 205f);

        CreateLabel(generalPanel.transform, "LANGUAGE", 135f);
        languageDropdown = CreateDropdown(
            generalPanel.transform,
            "Language_Dropdown",
            170f,
            135f
        );
        languageDropdown.AddOptions(
            new List<string> { "English", "Indonesia" }
        );

        CreateLabel(generalPanel.transform, "DIALOGUE SPEED", 50f);
        dialogueSpeedSlider = CreateSlider(
            generalPanel.transform,
            "DialogueSpeed_Slider",
            170f,
            50f,
            0f,
            1f,
            0.5f
        );

        CreateLabel(generalPanel.transform, "UI SCALE", -35f);
        uiScaleSlider = CreateSlider(
            generalPanel.transform,
            "UIScale_Slider",
            170f,
            -35f,
            0.75f,
            1.25f,
            1f
        );

        CreateLabel(generalPanel.transform, "SCREEN SHAKE", -120f);
        screenShakeToggle = CreateToggle(
            generalPanel.transform,
            "ScreenShake_Toggle",
            295f,
            -120f,
            true
        );

        CreateLabel(generalPanel.transform, "TUTORIALS", -195f);
        tutorialToggle = CreateToggle(
            generalPanel.transform,
            "Tutorial_Toggle",
            295f,
            -195f,
            true
        );
    }

    // =========================================================
    // GRAPHICS
    // =========================================================

    private void BuildGraphicsPanel()
    {
        CreateSectionTitle(graphicsPanel.transform, "GRAPHICS", 205f);

        CreateLabel(graphicsPanel.transform, "RESOLUTION", 125f);
        resolutionDropdown = CreateDropdown(
            graphicsPanel.transform,
            "Resolution_Dropdown",
            170f,
            125f
        );

        CreateLabel(graphicsPanel.transform, "QUALITY", 30f);
        qualityDropdown = CreateDropdown(
            graphicsPanel.transform,
            "Quality_Dropdown",
            170f,
            30f
        );

        CreateLabel(graphicsPanel.transform, "FULLSCREEN", -75f);
        fullscreenToggle = CreateToggle(
            graphicsPanel.transform,
            "Fullscreen_Toggle",
            295f,
            -75f,
            Screen.fullScreen
        );

        CreateLabel(graphicsPanel.transform, "V-SYNC", -170f);
        vsyncToggle = CreateToggle(
            graphicsPanel.transform,
            "VSync_Toggle",
            295f,
            -170f,
            QualitySettings.vSyncCount > 0
        );
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void BuildAudioPanel()
    {
        CreateSectionTitle(audioPanel.transform, "AUDIO", 205f);

        CreateLabel(audioPanel.transform, "MASTER VOLUME", 110f);
        masterVolumeSlider = CreateSlider(
            audioPanel.transform,
            "MasterVolume_Slider",
            145f,
            110f,
            0f,
            1f,
            1f
        );
        masterPercent = CreateText(
            "Master_Percent",
            audioPanel.transform,
            "100%",
            80f,
            44f,
            380f,
            110f,
            21f,
            TextAlignmentOptions.Center
        );

        CreateLabel(audioPanel.transform, "MUSIC VOLUME", 0f);
        musicVolumeSlider = CreateSlider(
            audioPanel.transform,
            "MusicVolume_Slider",
            145f,
            0f,
            0f,
            1f,
            0.8f
        );
        musicPercent = CreateText(
            "Music_Percent",
            audioPanel.transform,
            "80%",
            80f,
            44f,
            380f,
            0f,
            21f,
            TextAlignmentOptions.Center
        );

        CreateLabel(audioPanel.transform, "SFX VOLUME", -110f);
        sfxVolumeSlider = CreateSlider(
            audioPanel.transform,
            "SFXVolume_Slider",
            145f,
            -110f,
            0f,
            1f,
            0.8f
        );
        sfxPercent = CreateText(
            "SFX_Percent",
            audioPanel.transform,
            "80%",
            80f,
            44f,
            380f,
            -110f,
            21f,
            TextAlignmentOptions.Center
        );

        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(
            value => SetPercent(musicPercent, value)
        );
        sfxVolumeSlider.onValueChanged.AddListener(
            value => SetPercent(sfxPercent, value)
        );
    }

    // =========================================================
    // CONTROLS
    // =========================================================

    private void BuildControlsPanel()
    {
        CreateSectionTitle(controlsPanel.transform, "CONTROLS", 205f);

        CreateControlRow("MOVE", "WASD", 125f);
        CreateControlRow("ATTACK", "Mouse 1", 65f);
        CreateControlRow("DODGE", "Space", 5f);
        CreateControlRow("PARRY", "Q", -55f);
        CreateControlRow("PURIFY", "E", -115f);
        CreateControlRow("INTERACT", "F", -175f);
    }

    private void CreateControlRow(string actionName, string keyName, float y)
    {
        CreateText(
            actionName + "_Label",
            controlsPanel.transform,
            actionName,
            260f,
            44f,
            -225f,
            y,
            22f,
            TextAlignmentOptions.MidlineLeft
        );

        GameObject keyBox = CreateImageObject(
            actionName + "_Key",
            controlsPanel.transform,
            buttonSmallNormal,
            buttonSmallNormal != null ? Color.white : fallbackBlue
        );

        SetRect(
            keyBox.GetComponent<RectTransform>(),
            250f,
            48f,
            205f,
            y
        );

        CreateText(
            "Text (TMP)",
            keyBox.transform,
            keyName,
            230f,
            40f,
            0f,
            0f,
            20f,
            TextAlignmentOptions.Center
        );
    }

    // =========================================================
    // BOTTOM BUTTONS
    // =========================================================

    private void BuildBottomButtons()
    {
        resetButton = CreateSmallButton(
            "Reset_Button",
            "RESET",
            25f,
            -325f
        );

        applyButton = CreateSmallButton(
            "Apply_Button",
            "APPLY",
            250f,
            -325f
        );

        backButton = CreateSmallButton(
            "Back_Button",
            "BACK",
            475f,
            -325f
        );

        resetButton.onClick.AddListener(ResetToDefault);
        applyButton.onClick.AddListener(ApplySettings);
        backButton.onClick.AddListener(Back);
    }

    private Button CreateSmallButton(
        string objectName,
        string label,
        float x,
        float y
    )
    {
        GameObject buttonObject = CreateImageObject(
            objectName,
            contentRoot.transform,
            buttonSmallNormal,
            buttonSmallNormal != null ? Color.white : fallbackBlue
        );

        SetRect(
            buttonObject.GetComponent<RectTransform>(),
            190f,
            58f,
            x,
            y
        );

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();

        ConfigureButtonSprites(
            button,
            buttonSmallNormal,
            buttonSmallHighlighted,
            buttonSmallPressed,
            buttonSmallSelected,
            buttonSmallDisabled
        );

        CreateText(
            "Text (TMP)",
            buttonObject.transform,
            label,
            170f,
            48f,
            0f,
            0f,
            22f,
            TextAlignmentOptions.Center
        );

        return button;
    }

    // =========================================================
    // OPEN / CLOSE
    // =========================================================

    public void OpenSettings()
    {
        if (!setupComplete)
            Setup();

        if (!setupComplete)
            return;

        settingsPanel.SetActive(true);

        LoadSettings();
        ShowTab(0);
    }

    public void Back()
    {
        ApplySettings();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuManager != null)
            mainMenuManager.BackFromSettings();
        else
            Debug.LogWarning(
                "WRAITHGATE SETTINGS: MainMenuManager belum dihubungkan."
            );
    }

    // =========================================================
    // TAB SWITCHING
    // =========================================================

    public void ShowGeneral()
    {
        ShowTab(0);
    }

    public void ShowGraphics()
    {
        ShowTab(1);
    }

    public void ShowAudio()
    {
        ShowTab(2);
    }

    public void ShowControls()
    {
        ShowTab(3);
    }

    private void ShowTab(int index)
    {
        if (generalPanel == null)
            return;

        generalPanel.SetActive(index == 0);
        graphicsPanel.SetActive(index == 1);
        audioPanel.SetActive(index == 2);
        controlsPanel.SetActive(index == 3);

        SetTabVisual(generalTabImage, index == 0);
        SetTabVisual(graphicsTabImage, index == 1);
        SetTabVisual(audioTabImage, index == 2);
        SetTabVisual(controlsTabImage, index == 3);
    }

    private void SetTabVisual(Image image, bool selected)
    {
        if (image == null)
            return;

        // =========================
        // TAB SPRITE
        // =========================
        if (selected && tabSelected != null)
        {
            image.sprite = tabSelected;
            image.color = Color.white;
        }
        else if (tabNormal != null)
        {
            image.sprite = tabNormal;
            image.color = Color.white;
        }

        // =========================
        // TAB TEXT COLOR
        // =========================
        TMP_Text tabText = image.GetComponentInChildren<TMP_Text>(true);

        if (tabText != null)
        {
            tabText.color =
                selected
                    ? tabSelectedTextColor
                    : tabNormalTextColor;
        }
    }

    // =========================================================
    // SAVE / LOAD SETTINGS
    // =========================================================

    public void ApplySettings()
    {
        if (generalPanel == null)
            return;

        PlayerPrefs.SetInt(KEY_LANGUAGE, languageDropdown.value);
        PlayerPrefs.SetFloat(KEY_DIALOGUE, dialogueSpeedSlider.value);
        PlayerPrefs.SetFloat(KEY_UI_SCALE, uiScaleSlider.value);
        PlayerPrefs.SetInt(
            KEY_SHAKE,
            screenShakeToggle.isOn ? 1 : 0
        );
        PlayerPrefs.SetInt(
            KEY_TUTORIAL,
            tutorialToggle.isOn ? 1 : 0
        );

        PlayerPrefs.SetInt(
            KEY_RESOLUTION,
            resolutionDropdown.value
        );
        PlayerPrefs.SetInt(
            KEY_QUALITY,
            qualityDropdown.value
        );
        PlayerPrefs.SetInt(
            KEY_FULLSCREEN,
            fullscreenToggle.isOn ? 1 : 0
        );
        PlayerPrefs.SetInt(
            KEY_VSYNC,
            vsyncToggle.isOn ? 1 : 0
        );

        PlayerPrefs.SetFloat(
            KEY_MASTER,
            masterVolumeSlider.value
        );
        PlayerPrefs.SetFloat(
            KEY_MUSIC,
            musicVolumeSlider.value
        );
        PlayerPrefs.SetFloat(
            KEY_SFX,
            sfxVolumeSlider.value
        );

        PlayerPrefs.Save();

        ApplyGraphicsSettings();

        AudioListener.volume = masterVolumeSlider.value;

        // UI Scale is saved here.
        // Applying it to your complete HUD can be connected later
        // once the gameplay HUD exists.
    }

    private void ApplyGraphicsSettings()
    {
        if (availableResolutions.Count > 0)
        {
            int index = Mathf.Clamp(
                resolutionDropdown.value,
                0,
                availableResolutions.Count - 1
            );

            Resolution resolution =
                availableResolutions[index];

            Screen.SetResolution(
                resolution.width,
                resolution.height,
                fullscreenToggle.isOn
            );
        }

        if (QualitySettings.names.Length > 0)
        {
            int qualityIndex = Mathf.Clamp(
                qualityDropdown.value,
                0,
                QualitySettings.names.Length - 1
            );

            QualitySettings.SetQualityLevel(
                qualityIndex,
                true
            );
        }

        QualitySettings.vSyncCount =
            vsyncToggle.isOn ? 1 : 0;
    }

    private void LoadSettings()
    {
        if (generalPanel == null)
            return;

        languageDropdown.value =
            PlayerPrefs.GetInt(KEY_LANGUAGE, 0);

        dialogueSpeedSlider.value =
            PlayerPrefs.GetFloat(KEY_DIALOGUE, 0.5f);

        uiScaleSlider.value =
            PlayerPrefs.GetFloat(KEY_UI_SCALE, 1f);

        screenShakeToggle.isOn =
            PlayerPrefs.GetInt(KEY_SHAKE, 1) == 1;

        tutorialToggle.isOn =
            PlayerPrefs.GetInt(KEY_TUTORIAL, 1) == 1;

        resolutionDropdown.value = Mathf.Clamp(
            PlayerPrefs.GetInt(
                KEY_RESOLUTION,
                FindCurrentResolution()
            ),
            0,
            Mathf.Max(0, availableResolutions.Count - 1)
        );

        qualityDropdown.value = Mathf.Clamp(
            PlayerPrefs.GetInt(
                KEY_QUALITY,
                QualitySettings.GetQualityLevel()
            ),
            0,
            Mathf.Max(0, QualitySettings.names.Length - 1)
        );

        fullscreenToggle.isOn =
            PlayerPrefs.GetInt(
                KEY_FULLSCREEN,
                Screen.fullScreen ? 1 : 0
            ) == 1;

        vsyncToggle.isOn =
            PlayerPrefs.GetInt(
                KEY_VSYNC,
                QualitySettings.vSyncCount > 0 ? 1 : 0
            ) == 1;

        masterVolumeSlider.value =
            PlayerPrefs.GetFloat(KEY_MASTER, 1f);

        musicVolumeSlider.value =
            PlayerPrefs.GetFloat(KEY_MUSIC, 0.8f);

        sfxVolumeSlider.value =
            PlayerPrefs.GetFloat(KEY_SFX, 0.8f);

        SetPercent(
            masterPercent,
            masterVolumeSlider.value
        );

        SetPercent(
            musicPercent,
            musicVolumeSlider.value
        );

        SetPercent(
            sfxPercent,
            sfxVolumeSlider.value
        );
    }

    public void ResetToDefault()
    {
        languageDropdown.value = 0;
        dialogueSpeedSlider.value = 0.5f;
        uiScaleSlider.value = 1f;

        screenShakeToggle.isOn = true;
        tutorialToggle.isOn = true;

        resolutionDropdown.value =
            FindCurrentResolution();

        qualityDropdown.value =
            QualitySettings.GetQualityLevel();

        fullscreenToggle.isOn = true;
        vsyncToggle.isOn = true;

        masterVolumeSlider.value = 1f;
        musicVolumeSlider.value = 0.8f;
        sfxVolumeSlider.value = 0.8f;
    }

    // =========================================================
    // RESOLUTION / QUALITY
    // =========================================================

    private void SetupResolutionOptions()
    {
        if (resolutionDropdown == null)
            return;

        availableResolutions.Clear();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        HashSet<string> duplicateCheck =
            new HashSet<string>();

        Resolution[] resolutions =
            Screen.resolutions;

        foreach (Resolution resolution in resolutions)
        {
            string option =
                resolution.width +
                " × " +
                resolution.height;

            if (duplicateCheck.Add(option))
            {
                availableResolutions.Add(resolution);
                options.Add(option);
            }
        }

        if (options.Count == 0)
        {
            Resolution current =
                Screen.currentResolution;

            availableResolutions.Add(current);

            options.Add(
                Screen.width +
                " × " +
                Screen.height
            );
        }

        resolutionDropdown.AddOptions(options);
    }

    private void SetupQualityOptions()
    {
        if (qualityDropdown == null)
            return;

        qualityDropdown.ClearOptions();

        qualityDropdown.AddOptions(
            new List<string>(
                QualitySettings.names
            )
        );
    }

    private int FindCurrentResolution()
    {
        for (
            int i = 0;
            i < availableResolutions.Count;
            i++
        )
        {
            if (
                availableResolutions[i].width ==
                Screen.width &&
                availableResolutions[i].height ==
                Screen.height
            )
            {
                return i;
            }
        }

        return Mathf.Max(
            0,
            availableResolutions.Count - 1
        );
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void OnMasterVolumeChanged(float value)
    {
        SetPercent(masterPercent, value);
        AudioListener.volume = value;
    }

    private void SetPercent(
        TMP_Text target,
        float value
    )
    {
        if (target != null)
        {
            target.text =
                Mathf.RoundToInt(value * 100f) +
                "%";
        }
    }

    // =========================================================
    // NEW INPUT SYSTEM
    // =========================================================

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (
            settingsPanel != null &&
            settingsPanel.activeSelf &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame
        )
        {
            Back();
        }
#endif
    }

    // =========================================================
    // UI BUILD HELPERS
    // =========================================================

    private void CreateSectionTitle(
        Transform parent,
        string title,
        float y
    )
    {
        CreateText(
            title + "_Title",
            parent,
            title,
            500f,
            50f,
            0f,
            y,
            30f,
            TextAlignmentOptions.Center
        );
    }

    private void CreateLabel(
        Transform parent,
        string label,
        float y
    )
    {
        CreateText(
            label + "_Label",
            parent,
            label,
            280f,
            46f,
            -235f,
            y,
            21f,
            TextAlignmentOptions.MidlineLeft
        );
    }

    private Slider CreateSlider(
        Transform parent,
        string objectName,
        float x,
        float y,
        float minValue,
        float maxValue,
        float startingValue
    )
    {
        GameObject root =
            CreateUIObject(objectName, parent);

        SetRect(
            root.GetComponent<RectTransform>(),
            320f,
            48f,
            x,
            y
        );

        Slider slider =
            root.AddComponent<Slider>();

        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = startingValue;

        GameObject background =
            CreateImageObject(
                "Background",
                root.transform,
                sliderBackground,
                sliderBackground != null
                    ? Color.white
                    : fallbackBlue
            );

        RectTransform backgroundRT =
            background.GetComponent<RectTransform>();

        backgroundRT.anchorMin =
            new Vector2(0f, 0.5f);

        backgroundRT.anchorMax =
            new Vector2(1f, 0.5f);

        backgroundRT.sizeDelta =
            new Vector2(0f, 16f);

        backgroundRT.anchoredPosition =
            Vector2.zero;

        GameObject fillArea =
            CreateUIObject(
                "Fill Area",
                root.transform
            );

        Stretch(
            fillArea.GetComponent<RectTransform>(),
            12f,
            12f,
            15f,
            15f
        );

        GameObject fill =
            CreateImageObject(
                "Fill",
                fillArea.transform,
                sliderFill,
                sliderFill != null
                    ? Color.white
                    : fallbackGold
            );

        Stretch(
            fill.GetComponent<RectTransform>()
        );

        GameObject handleArea =
            CreateUIObject(
                "Handle Slide Area",
                root.transform
            );

        Stretch(
            handleArea.GetComponent<RectTransform>(),
            18f,
            18f,
            0f,
            0f
        );

        GameObject handle =
            CreateImageObject(
                "Handle",
                handleArea.transform,
                sliderHandleNormal,
                sliderHandleNormal != null
                    ? Color.white
                    : textColor
            );

        RectTransform handleRT =
            handle.GetComponent<RectTransform>();

        handleRT.sizeDelta =
            new Vector2(36f, 36f);

        slider.fillRect =
            fill.GetComponent<RectTransform>();

        slider.handleRect = handleRT;

        slider.targetGraphic =
            handle.GetComponent<Image>();

        if (sliderHandleNormal != null)
        {
            slider.transition =
                Selectable.Transition.SpriteSwap;

            slider.spriteState =
                new SpriteState
                {
                    highlightedSprite =
                        sliderHandleHighlighted != null
                            ? sliderHandleHighlighted
                            : sliderHandleNormal,

                    pressedSprite =
                        sliderHandleSelected != null
                            ? sliderHandleSelected
                            : sliderHandleNormal,

                    selectedSprite =
                        sliderHandleSelected != null
                            ? sliderHandleSelected
                            : sliderHandleNormal,

                    disabledSprite =
                        sliderHandleDisabled != null
                            ? sliderHandleDisabled
                            : sliderHandleNormal
                };
        }

        return slider;
    }

    private Toggle CreateToggle(
        Transform parent,
        string objectName,
        float x,
        float y,
        bool startingValue
    )
    {
        GameObject root =
            CreateUIObject(objectName, parent);

        SetRect(
            root.GetComponent<RectTransform>(),
            105f,
            44f,
            x,
            y
        );

        Toggle toggle =
            root.AddComponent<Toggle>();

        GameObject background =
            CreateImageObject(
                "Background",
                root.transform,
                toggleOffNormal,
                toggleOffNormal != null
                    ? Color.white
                    : fallbackBlue
            );

        Stretch(
            background.GetComponent<RectTransform>()
        );

        GameObject checkmark =
            CreateImageObject(
                "Checkmark",
                background.transform,
                toggleOnNormal,
                toggleOnNormal != null
                    ? Color.white
                    : fallbackGold
            );

        Stretch(
            checkmark.GetComponent<RectTransform>()
        );

        toggle.targetGraphic =
            background.GetComponent<Image>();

        toggle.graphic =
            checkmark.GetComponent<Image>();

        toggle.isOn = startingValue;

        return toggle;
    }

    private TMP_Dropdown CreateDropdown(
        Transform parent,
        string objectName,
        float x,
        float y
    )
    {
        GameObject root =
            CreateImageObject(
                objectName,
                parent,
                null,
                fallbackBlue
            );

        SetRect(
            root.GetComponent<RectTransform>(),
            320f,
            54f,
            x,
            y
        );

        TMP_Dropdown dropdown =
            root.AddComponent<TMP_Dropdown>();

        dropdown.targetGraphic =
            root.GetComponent<Image>();

        TMP_Text caption =
            CreateText(
                "Label",
                root.transform,
                "OPTION",
                250f,
                44f,
                -20f,
                0f,
                20f,
                TextAlignmentOptions.MidlineLeft
            );

        dropdown.captionText = caption;

        CreateText(
            "Arrow",
            root.transform,
            "▼",
            36f,
            36f,
            140f,
            0f,
            17f,
            TextAlignmentOptions.Center
        );

        GameObject template =
            CreateImageObject(
                "Template",
                root.transform,
                null,
                fallbackNavy
            );

        RectTransform templateRT =
            template.GetComponent<RectTransform>();

        templateRT.anchorMin =
            new Vector2(0f, 0f);

        templateRT.anchorMax =
            new Vector2(1f, 0f);

        templateRT.pivot =
            new Vector2(0.5f, 1f);

        templateRT.sizeDelta =
            new Vector2(0f, 180f);

        templateRT.anchoredPosition =
            new Vector2(0f, -3f);

        GameObject viewport =
            CreateImageObject(
                "Viewport",
                template.transform,
                null,
                Color.white
            );

        Stretch(
            viewport.GetComponent<RectTransform>()
        );

        Mask mask =
            viewport.AddComponent<Mask>();

        mask.showMaskGraphic = false;

        GameObject content =
            CreateUIObject(
                "Content",
                viewport.transform
            );

        RectTransform contentRT =
            content.GetComponent<RectTransform>();

        contentRT.anchorMin =
            new Vector2(0f, 1f);

        contentRT.anchorMax =
            new Vector2(1f, 1f);

        contentRT.pivot =
            new Vector2(0.5f, 1f);

        contentRT.sizeDelta =
            new Vector2(0f, 34f);

        GameObject item =
            CreateImageObject(
                "Item",
                content.transform,
                null,
                fallbackBlue
            );

        RectTransform itemRT =
            item.GetComponent<RectTransform>();

        itemRT.anchorMin =
            new Vector2(0f, 0.5f);

        itemRT.anchorMax =
            new Vector2(1f, 0.5f);

        itemRT.sizeDelta =
            new Vector2(0f, 34f);

        Toggle itemToggle =
            item.AddComponent<Toggle>();

        itemToggle.targetGraphic =
            item.GetComponent<Image>();

        TMP_Text itemLabel =
            CreateText(
                "Item Label",
                item.transform,
                "OPTION",
                285f,
                30f,
                0f,
                0f,
                19f,
                TextAlignmentOptions.MidlineLeft
            );

        ScrollRect scrollRect =
            template.AddComponent<ScrollRect>();

        scrollRect.horizontal = false;
        scrollRect.viewport =
            viewport.GetComponent<RectTransform>();
        scrollRect.content = contentRT;

        dropdown.template = templateRT;
        dropdown.itemText = itemLabel;

        template.SetActive(false);

        return dropdown;
    }

    private GameObject CreateUIObject(
        string objectName,
        Transform parent
    )
    {
        GameObject go =
            new GameObject(
                objectName,
                typeof(RectTransform)
            );

        go.transform.SetParent(
            parent,
            false
        );

        return go;
    }

    private GameObject CreateImageObject(
        string objectName,
        Transform parent,
        Sprite sprite,
        Color color
    )
    {
        GameObject go =
            CreateUIObject(
                objectName,
                parent
            );

        Image image =
            go.AddComponent<Image>();

        image.sprite = sprite;
        image.color = color;
        image.type = Image.Type.Simple;

        return go;
    }

    private TMP_Text CreateText(
        string objectName,
        Transform parent,
        string value,
        float width,
        float height,
        float x,
        float y,
        float fontSize,
        TextAlignmentOptions alignment
    )
    {
        GameObject go =
            CreateUIObject(
                objectName,
                parent
            );

        TextMeshProUGUI text =
            go.AddComponent<TextMeshProUGUI>();

        if (pixelFont != null)
            text.font = pixelFont;

        text.text = value;
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = alignment;
        text.overflowMode =
            TextOverflowModes.Overflow;
        text.raycastTarget = false;

        SetRect(
            text.rectTransform,
            width,
            height,
            x,
            y
        );

        return text;
    }

    private void SetRect(
        RectTransform rt,
        float width,
        float height,
        float x,
        float y
    )
    {
        rt.anchorMin =
            new Vector2(0.5f, 0.5f);

        rt.anchorMax =
            new Vector2(0.5f, 0.5f);

        rt.pivot =
            new Vector2(0.5f, 0.5f);

        rt.sizeDelta =
            new Vector2(width, height);

        rt.anchoredPosition =
            new Vector2(x, y);

        rt.localScale =
            Vector3.one;
    }

    private void Stretch(
        RectTransform rt,
        float left = 0f,
        float right = 0f,
        float top = 0f,
        float bottom = 0f
    )
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        rt.offsetMin =
            new Vector2(
                left,
                bottom
            );

        rt.offsetMax =
            new Vector2(
                -right,
                -top
            );

        rt.localScale =
            Vector3.one;
    }

    private void ConfigureButtonSprites(
        Button button,
        Sprite normal,
        Sprite highlighted,
        Sprite pressed,
        Sprite selected,
        Sprite disabled
    )
    {
        if (button == null || normal == null)
            return;

        Image image =
            button.GetComponent<Image>();

        if (image != null)
        {
            image.sprite = normal;
            image.color = Color.white;
            button.targetGraphic = image;
        }

        button.transition =
            Selectable.Transition.SpriteSwap;

        button.spriteState =
            new SpriteState
            {
                highlightedSprite =
                    highlighted != null
                        ? highlighted
                        : normal,

                pressedSprite =
                    pressed != null
                        ? pressed
                        : normal,

                selectedSprite =
                    selected != null
                        ? selected
                        : normal,

                disabledSprite =
                    disabled != null
                        ? disabled
                        : normal
            };
    }
}
