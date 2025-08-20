using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Game UI")]
    public Text scoreText;
    public Text timeText;
    public Text distanceText;
    public Slider healthBar;
    public Text livesText;
    
    [Header("HUD Elements")]
    public GameObject miniMap;
    public Image speedometer;
    public Text speedText;
    public GameObject collectibleCounter;
    public Text collectibleText;
    
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject settingsPanel;
    public GameObject educationalPanel;
    public GameObject culturalPanel;
    
    [Header("Educational UI")]
    public Text educationalTitle;
    public Text educationalContent;
    public Button educationalCloseButton;
    public Image educationalImage;
    
    [Header("Cultural UI")]
    public Text culturalTitle;
    public Text culturalContent;
    public Button culturalCloseButton;
    public Image culturalImage;
    
    [Header("Progress UI")]
    public Slider levelProgressBar;
    public Text checkpointText;
    public GameObject[] checkpointMarkers;
    
    [Header("Mobile UI")]
    public Button pauseButton;
    public Button jumpButton;
    public Button leftButton;
    public Button rightButton;
    public GameObject touchControls;
    
    [Header("Settings UI")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle vibrationToggle;
    public Dropdown qualityDropdown;
    public Button backButton;
    
    [Header("Game Over UI")]
    public Text finalScoreText;
    public Text highScoreText;
    public Text gameOverMessage;
    public Button restartButton;
    public Button mainMenuButton;
    
    [Header("Achievement UI")]
    public GameObject achievementPopup;
    public Text achievementTitle;
    public Text achievementDescription;
    public Image achievementIcon;
    
    private bool isPaused = false;
    private bool isEducationalActive = false;
    private Coroutine achievementCoroutine;
    
    void Start()
    {
        InitializeUI();
        SetupMobileControls();
        SetupEventListeners();
    }
    
    void InitializeUI()
    {
        // Hide all panels initially
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (educationalPanel) educationalPanel.SetActive(false);
        if (culturalPanel) culturalPanel.SetActive(false);
        if (achievementPopup) achievementPopup.SetActive(false);
        
        // Initialize progress bar
        if (levelProgressBar)
        {
            levelProgressBar.minValue = 0f;
            levelProgressBar.maxValue = 2000f; // Level length
            levelProgressBar.value = 0f;
        }
        
        // Setup checkpoint markers
        SetupCheckpointMarkers();
        
        // Load saved settings
        LoadSettings();
    }
    
    void SetupMobileControls()
    {
        // Show touch controls on mobile
        if (touchControls)
        {
            #if UNITY_ANDROID || UNITY_IOS
                touchControls.SetActive(true);
            #else
                touchControls.SetActive(false);
            #endif
        }
        
        // Setup mobile button listeners
        if (pauseButton) pauseButton.onClick.AddListener(TogglePause);
        if (jumpButton) jumpButton.onClick.AddListener(OnJumpPressed);
        if (leftButton) leftButton.onClick.AddListener(OnLeftPressed);
        if (rightButton) rightButton.onClick.AddListener(OnRightPressed);
    }
    
    void SetupEventListeners()
    {
        // Educational panel
        if (educationalCloseButton) educationalCloseButton.onClick.AddListener(CloseEducationalPanel);
        
        // Cultural panel
        if (culturalCloseButton) culturalCloseButton.onClick.AddListener(CloseCulturalPanel);
        
        // Settings panel
        if (backButton) backButton.onClick.AddListener(CloseSettingsPanel);
        if (musicVolumeSlider) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        if (vibrationToggle) vibrationToggle.onValueChanged.AddListener(SetVibration);
        if (qualityDropdown) qualityDropdown.onValueChanged.AddListener(SetQuality);
        
        // Game over panel
        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(LoadMainMenu);
    }
    
    void SetupCheckpointMarkers()
    {
        if (checkpointMarkers != null)
        {
            for (int i = 0; i < checkpointMarkers.Length; i++)
            {
                if (checkpointMarkers[i])
                {
                    checkpointMarkers[i].SetActive(false);
                }
            }
        }
    }
    
    void Update()
    {
        UpdateGameUI();
        HandleInput();
    }
    
    void UpdateGameUI()
    {
        if (GameManager.Instance)
        {
            // Update score
            if (scoreText)
                scoreText.text = "Score: " + GameManager.Instance.currentScore.ToString();
            
            // Update time
            if (timeText)
                timeText.text = "Time: " + GameManager.Instance.gameTime.ToString("F1") + "s";
            
            // Update distance
            if (distanceText)
                distanceText.text = "Distance: " + GameManager.Instance.currentDistance.ToString("F0") + "m";
            
            // Update progress bar
            if (levelProgressBar)
            {
                levelProgressBar.value = GameManager.Instance.currentDistance;
                
                // Update checkpoint markers
                UpdateCheckpointMarkers();
            }
            
            // Update checkpoint text
            if (checkpointText)
                checkpointText.text = "Checkpoints: " + GameManager.Instance.checkpointsCompleted + "/5";
        }
        
        // Update player-specific UI
        UpdatePlayerUI();
    }
    
    void UpdatePlayerUI()
    {
        BoomerController player = FindObjectOfType<BoomerController>();
        if (player)
        {
            // Update speed indicator
            if (speedometer || speedText)
            {
                float speed = player.GetComponent<Rigidbody>().velocity.magnitude;
                
                if (speedometer)
                    speedometer.fillAmount = speed / 20f; // Normalize to max speed
                
                if (speedText)
                    speedText.text = speed.ToString("F1") + " m/s";
            }
        }
    }
    
    void UpdateCheckpointMarkers()
    {
        if (checkpointMarkers == null || GameManager.Instance == null) return;
        
        float progress = GameManager.Instance.currentDistance / 2000f;
        int completedCheckpoints = Mathf.FloorToInt(progress * 5);
        
        for (int i = 0; i < checkpointMarkers.Length && i < 5; i++)
        {
            if (checkpointMarkers[i])
            {
                checkpointMarkers[i].SetActive(i < completedCheckpoints);
            }
        }
    }
    
    void HandleInput()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        // Handle mobile back button
        if (Input.GetKeyDown(KeyCode.Escape) && Application.platform == RuntimePlatform.Android)
        {
            if (isEducationalActive)
            {
                CloseEducationalPanel();
            }
            else if (isPaused)
            {
                TogglePause();
            }
            else
            {
                TogglePause();
            }
        }
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        }
    }
    
    public void ShowEducationalContent(string title, string content, Sprite image = null)
    {
        if (educationalPanel)
        {
            educationalPanel.SetActive(true);
            isEducationalActive = true;
            
            if (educationalTitle) educationalTitle.text = title;
            if (educationalContent) educationalContent.text = content;
            if (educationalImage && image) educationalImage.sprite = image;
            
            // Pause the game
            Time.timeScale = 0f;
        }
    }
    
    public void CloseEducationalPanel()
    {
        if (educationalPanel)
        {
            educationalPanel.SetActive(false);
            isEducationalActive = false;
            
            // Resume the game
            Time.timeScale = 1f;
        }
    }
    
    public void ShowCulturalContent(string title, string content, Sprite image = null)
    {
        if (culturalPanel)
        {
            culturalPanel.SetActive(true);
            
            if (culturalTitle) culturalTitle.text = title;
            if (culturalContent) culturalContent.text = content;
            if (culturalImage && image) culturalImage.sprite = image;
            
            // Pause the game
            Time.timeScale = 0f;
        }
    }
    
    public void CloseCulturalPanel()
    {
        if (culturalPanel)
        {
            culturalPanel.SetActive(false);
            
            // Resume the game
            Time.timeScale = 1f;
        }
    }
    
    public void ShowGameOver(int finalScore, int highScore, string message)
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText) finalScoreText.text = "Final Score: " + finalScore.ToString();
            if (highScoreText) highScoreText.text = "High Score: " + highScore.ToString();
            if (gameOverMessage) gameOverMessage.text = message;
        }
    }
    
    public void ShowAchievement(string title, string description, Sprite icon)
    {
        if (achievementPopup)
        {
            if (achievementCoroutine != null)
                StopCoroutine(achievementCoroutine);
            
            achievementCoroutine = StartCoroutine(ShowAchievementCoroutine(title, description, icon));
        }
    }
    
    IEnumerator ShowAchievementCoroutine(string title, string description, Sprite icon)
    {
        achievementPopup.SetActive(true);
        
        if (achievementTitle) achievementTitle.text = title;
        if (achievementDescription) achievementDescription.text = description;
        if (achievementIcon && icon) achievementIcon.sprite = icon;
        
        // Animate in
        Vector3 startPos = achievementPopup.transform.position + Vector3.up * 100f;
        Vector3 endPos = achievementPopup.transform.position;
        
        float animTime = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < animTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animTime;
            achievementPopup.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        // Wait
        yield return new WaitForSecondsRealtime(3f);
        
        // Animate out
        elapsed = 0f;
        while (elapsed < animTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animTime;
            achievementPopup.transform.position = Vector3.Lerp(endPos, startPos, t);
            yield return null;
        }
        
        achievementPopup.SetActive(false);
    }
    
    public void OpenSettingsPanel()
    {
        if (settingsPanel) settingsPanel.SetActive(true);
    }
    
    public void CloseSettingsPanel()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        SaveSettings();
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    // Mobile control methods
    void OnJumpPressed()
    {
        BoomerController player = FindObjectOfType<BoomerController>();
        if (player)
        {
            // Trigger jump through player controller
            player.SendMessage("Hop", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void OnLeftPressed()
    {
        BoomerController player = FindObjectOfType<BoomerController>();
        if (player && player.currentLane > 0)
        {
            player.SendMessage("ChangeLane", -1, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void OnRightPressed()
    {
        BoomerController player = FindObjectOfType<BoomerController>();
        if (player && player.currentLane < 2)
        {
            player.SendMessage("ChangeLane", 1, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    // Settings methods
    void SetMusicVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    void SetSFXVolume(float volume)
    {
        // Set SFX volume for all audio sources with SFX tag
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.gameObject.CompareTag("SFX"))
            {
                source.volume = volume;
            }
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    void SetVibration(bool enabled)
    {
        PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
    }
    
    void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
    }
    
    void SaveSettings()
    {
        PlayerPrefs.Save();
    }
    
    void LoadSettings()
    {
        // Load music volume
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        if (musicVolumeSlider) musicVolumeSlider.value = musicVolume;
        AudioListener.volume = musicVolume;
        
        // Load SFX volume
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        if (sfxVolumeSlider) sfxVolumeSlider.value = sfxVolume;
        
        // Load vibration setting
        bool vibration = PlayerPrefs.GetInt("Vibration", 1) == 1;
        if (vibrationToggle) vibrationToggle.isOn = vibration;
        
        // Load quality setting
        int quality = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        if (qualityDropdown) qualityDropdown.value = quality;
        QualitySettings.SetQualityLevel(quality);
    }
    
    // Utility methods for other systems
    public void UpdateCollectibleCount(int count)
    {
        if (collectibleText)
            collectibleText.text = "Collectibles: " + count.ToString();
    }
    
    public void UpdateHealth(float health, float maxHealth)
    {
        if (healthBar)
        {
            healthBar.value = health / maxHealth;
        }
    }
    
    public void UpdateLives(int lives)
    {
        if (livesText)
            livesText.text = "Lives: " + lives.ToString();
    }
    
    // Mobile-specific methods
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && !isPaused)
        {
            TogglePause();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !isPaused)
        {
            TogglePause();
        }
    }
}
