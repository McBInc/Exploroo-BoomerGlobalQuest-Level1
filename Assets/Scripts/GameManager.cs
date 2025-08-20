using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game State")]
    public bool isGameActive = true;
    public int currentScore = 0;
    public int highScore = 0;
    public float gameTime = 0f;
    
    [Header("UI References")]
    public Text scoreText;
    public Text timeText;
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject educationalPanel;
    
    [Header("Level Settings")]
    public float levelLength = 2000f;
    public float currentDistance = 0f;
    public Transform player;
    
    [Header("Educational System")]
    public EducationalCheckpoint[] checkpoints;
    public int checkpointsCompleted = 0;
    
    [Header("Audio")]
    public AudioSource backgroundMusic;
    public AudioSource uiSounds;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeGame();
        LoadHighScore();
        
        // Mobile optimization
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    
    void Update()
    {
        if (!isGameActive) return;
        
        UpdateGameTime();
        UpdateDistance();
        UpdateUI();
        CheckLevelCompletion();
    }
    
    void InitializeGame()
    {
        isGameActive = true;
        currentScore = 0;
        gameTime = 0f;
        currentDistance = 0f;
        checkpointsCompleted = 0;
        
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (educationalPanel) educationalPanel.SetActive(false);
        
        if (backgroundMusic) backgroundMusic.Play();
    }
    
    void UpdateGameTime()
    {
        gameTime += Time.deltaTime;
    }
    
    void UpdateDistance()
    {
        if (player)
        {
            currentDistance = player.position.z;
        }
    }
    
    void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + currentScore.ToString();
        if (timeText) timeText.text = "Time: " + gameTime.ToString("F1") + "s";
    }
    
    void CheckLevelCompletion()
    {
        if (currentDistance >= levelLength)
        {
            CompleteLevel();
        }
    }
    
    public void AddScore(int points)
    {
        currentScore += points;
        
        // Bonus for educational checkpoints
        if (checkpointsCompleted > 0)
        {
            currentScore += points * checkpointsCompleted;
        }
    }
    
    public void GameOver()
    {
        isGameActive = false;
        
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
        
        if (backgroundMusic) backgroundMusic.Stop();
        if (gameOverPanel) gameOverPanel.SetActive(true);
        
        // Analytics tracking
        TrackGameOver();
    }
    
    public void CompleteLevel()
    {
        isGameActive = false;
        
        // Completion bonus
        int completionBonus = 1000;
        int timeBonus = Mathf.RoundToInt((300f - gameTime) * 10f); // Bonus for fast completion
        int educationBonus = checkpointsCompleted * 500;
        
        currentScore += completionBonus + timeBonus + educationBonus;
        
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
        }
        
        // Show completion screen
        ShowLevelComplete();
        
        // Analytics tracking
        TrackLevelComplete();
    }
    
    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (pausePanel) pausePanel.SetActive(true);
        if (backgroundMusic) backgroundMusic.Pause();
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);
        if (backgroundMusic) backgroundMusic.UnPause();
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void ShowEducationalContent(string content)
    {
        if (educationalPanel)
        {
            educationalPanel.SetActive(true);
            // Display educational content
            Text contentText = educationalPanel.GetComponentInChildren<Text>();
            if (contentText) contentText.text = content;
            
            PauseGame();
        }
    }
    
    public void CloseEducationalContent()
    {
        if (educationalPanel) educationalPanel.SetActive(false);
        ResumeGame();
        checkpointsCompleted++;
    }
    
    void ShowLevelComplete()
    {
        // Implementation for level completion screen
        Debug.Log("Level Complete! Score: " + currentScore);
    }
    
    void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
    
    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    
    void TrackGameOver()
    {
        // Analytics implementation
        Debug.Log("Game Over - Score: " + currentScore + ", Time: " + gameTime);
    }
    
    void TrackLevelComplete()
    {
        // Analytics implementation
        Debug.Log("Level Complete - Score: " + currentScore + ", Time: " + gameTime + ", Checkpoints: " + checkpointsCompleted);
    }
    
    // Mobile-specific methods
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isGameActive)
        {
            PauseGame();
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isGameActive)
        {
            PauseGame();
        }
    }
}
