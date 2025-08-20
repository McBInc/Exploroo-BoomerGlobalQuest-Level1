using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EducationalSystem : MonoBehaviour
{
    [Header("Educational Content")]
    public EducationalTopic[] topics;
    public GameObject quizPanelPrefab;
    public Transform educationalUIParent;
    
    [Header("Quiz System")]
    public QuizQuestion[] quizQuestions;
    public int questionsPerQuiz = 3;
    public float quizTimeLimit = 30f;
    
    [Header("Learning Progress")]
    public int topicsCompleted = 0;
    public int correctAnswers = 0;
    public int totalQuestions = 0;
    
    [Header("Rewards")]
    public int correctAnswerPoints = 50;
    public int topicCompletionPoints = 200;
    public GameObject[] unlockableContent;
    
    [Header("Audio")]
    public AudioClip correctAnswerSound;
    public AudioClip incorrectAnswerSound;
    public AudioClip topicCompleteSound;
    public AudioSource educationalAudioSource;
    
    private List<EducationalCheckpoint> activeCheckpoints = new List<EducationalCheckpoint>();
    private QuizPanel currentQuizPanel;
    
    [System.Serializable]
    public class EducationalTopic
    {
        public string topicName;
        public EducationalCategory category;
        [TextArea(3, 5)]
        public string description;
        public Sprite topicImage;
        public QuizQuestion[] relatedQuestions;
        public bool isCompleted = false;
    }
    
    [System.Serializable]
    public class QuizQuestion
    {
        [TextArea(2, 3)]
        public string question;
        public string[] answers;
        public int correctAnswerIndex;
        public EducationalCategory category;
        [TextArea(2, 3)]
        public string explanation;
    }
    
    public enum EducationalCategory
    {
        Wildlife,
        Conservation,
        Geography,
        Culture,
        History,
        Environment,
        Science,
        Sustainability
    }
    
    void Start()
    {
        InitializeEducationalSystem();
        CreateDefaultContent();
        SetupEducationalCheckpoints();
    }
    
    void InitializeEducationalSystem()
    {
        if (educationalAudioSource == null)
        {
            educationalAudioSource = gameObject.AddComponent<AudioSource>();
            educationalAudioSource.volume = 0.8f;
            educationalAudioSource.spatialBlend = 0f; // 2D audio for UI
        }
    }
    
    void CreateDefaultContent()
    {
        if (topics == null || topics.Length == 0)
        {
            CreateDefaultTopics();
        }
        
        if (quizQuestions == null || quizQuestions.Length == 0)
        {
            CreateDefaultQuizQuestions();
        }
    }
    
    void CreateDefaultTopics()
    {
        topics = new EducationalTopic[]
        {
            new EducationalTopic
            {
                topicName = "Australian Wildlife",
                category = EducationalCategory.Wildlife,
                description = "Learn about Australia's unique animals including kangaroos, koalas, and wombats. Discover how they've adapted to the Australian environment."
            },
            new EducationalTopic
            {
                topicName = "Conservation Efforts",
                category = EducationalCategory.Conservation,
                description = "Understand the importance of protecting Australian wildlife and their habitats. Learn about conservation programs and how you can help."
            },
            new EducationalTopic
            {
                topicName = "Outback Geography",
                category = EducationalCategory.Geography,
                description = "Explore the vast Australian outback, its climate, landforms, and how it shapes the lives of people and animals."
            },
            new EducationalTopic
            {
                topicName = "Aboriginal Culture",
                category = EducationalCategory.Culture,
                description = "Learn about the rich cultural heritage of Aboriginal Australians and their deep connection to the land."
            },
            new EducationalTopic
            {
                topicName = "Ecosystem Balance",
                category = EducationalCategory.Environment,
                description = "Understand how different species work together in Australian ecosystems and why biodiversity is important."
            }
        };
    }
    
    void CreateDefaultQuizQuestions()
    {
        quizQuestions = new QuizQuestion[]
        {
            new QuizQuestion
            {
                question = "What is the fastest speed a kangaroo can hop?",
                answers = new string[] { "30 km/h", "45 km/h", "60 km/h", "75 km/h" },
                correctAnswerIndex = 2,
                category = EducationalCategory.Wildlife,
                explanation = "Kangaroos can hop at speeds up to 60 km/h and are also excellent swimmers!"
            },
            new QuizQuestion
            {
                question = "How long have Aboriginal Australians lived in Australia?",
                answers = new string[] { "20,000 years", "40,000 years", "65,000+ years", "100,000 years" },
                correctAnswerIndex = 2,
                category = EducationalCategory.Culture,
                explanation = "Aboriginal Australians have the world's oldest continuous culture, spanning over 65,000 years."
            },
            new QuizQuestion
            {
                question = "What percentage of Australia is covered by the outback?",
                answers = new string[] { "50%", "60%", "70%", "80%" },
                correctAnswerIndex = 2,
                category = EducationalCategory.Geography,
                explanation = "The Australian outback covers approximately 70% of the continent's landmass."
            },
            new QuizQuestion
            {
                question = "Which of these is NOT a marsupial?",
                answers = new string[] { "Kangaroo", "Koala", "Wombat", "Dingo" },
                correctAnswerIndex = 3,
                category = EducationalCategory.Wildlife,
                explanation = "Dingoes are placental mammals, while kangaroos, koalas, and wombats are all marsupials."
            },
            new QuizQuestion
            {
                question = "What is the main threat to koala populations?",
                answers = new string[] { "Predators", "Disease", "Habitat loss", "Climate change" },
                correctAnswerIndex = 2,
                category = EducationalCategory.Conservation,
                explanation = "Habitat loss due to deforestation and urban development is the primary threat to koala populations."
            },
            new QuizQuestion
            {
                question = "What do wombats use their strong claws for?",
                answers = new string[] { "Climbing trees", "Digging burrows", "Catching prey", "Defense only" },
                correctAnswerIndex = 1,
                category = EducationalCategory.Wildlife,
                explanation = "Wombats use their powerful claws to dig extensive burrow systems that can be up to 30 meters long."
            },
            new QuizQuestion
            {
                question = "Which traditional Aboriginal practice helped manage the landscape?",
                answers = new string[] { "Controlled burning", "Tree planting", "Dam building", "Mining" },
                correctAnswerIndex = 0,
                category = EducationalCategory.Culture,
                explanation = "Aboriginal people used controlled burning for thousands of years to manage vegetation and prevent large wildfires."
            },
            new QuizQuestion
            {
                question = "What makes the Australian ecosystem unique?",
                answers = new string[] { "High rainfall", "Many predators", "Geographic isolation", "Cold climate" },
                correctAnswerIndex = 2,
                category = EducationalCategory.Environment,
                explanation = "Australia's geographic isolation led to the evolution of unique species found nowhere else on Earth."
            }
        };
    }
    
    void SetupEducationalCheckpoints()
    {
        EducationalCheckpoint[] checkpoints = FindObjectsOfType<EducationalCheckpoint>();
        
        foreach (EducationalCheckpoint checkpoint in checkpoints)
        {
            checkpoint.Initialize(this);
            activeCheckpoints.Add(checkpoint);
        }
    }
    
    public void TriggerEducationalContent(EducationalCategory category, Vector3 position)
    {
        EducationalTopic topic = GetTopicByCategory(category);
        if (topic != null && !topic.isCompleted)
        {
            ShowEducationalContent(topic);
            StartQuiz(category);
        }
    }
    
    EducationalTopic GetTopicByCategory(EducationalCategory category)
    {
        foreach (EducationalTopic topic in topics)
        {
            if (topic.category == category && !topic.isCompleted)
            {
                return topic;
            }
        }
        return null;
    }
    
    void ShowEducationalContent(EducationalTopic topic)
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.ShowEducationalContent(topic.description);
        }
    }
    
    public void StartQuiz(EducationalCategory category)
    {
        List<QuizQuestion> categoryQuestions = GetQuestionsByCategory(category);
        
        if (categoryQuestions.Count >= questionsPerQuiz)
        {
            // Select random questions from the category
            List<QuizQuestion> selectedQuestions = new List<QuizQuestion>();
            for (int i = 0; i < questionsPerQuiz; i++)
            {
                QuizQuestion question = categoryQuestions[Random.Range(0, categoryQuestions.Count)];
                if (!selectedQuestions.Contains(question))
                {
                    selectedQuestions.Add(question);
                }
            }
            
            ShowQuizPanel(selectedQuestions);
        }
    }
    
    List<QuizQuestion> GetQuestionsByCategory(EducationalCategory category)
    {
        List<QuizQuestion> categoryQuestions = new List<QuizQuestion>();
        
        foreach (QuizQuestion question in quizQuestions)
        {
            if (question.category == category)
            {
                categoryQuestions.Add(question);
            }
        }
        
        return categoryQuestions;
    }
    
    void ShowQuizPanel(List<QuizQuestion> questions)
    {
        if (quizPanelPrefab && educationalUIParent)
        {
            GameObject quizObj = Instantiate(quizPanelPrefab, educationalUIParent);
            currentQuizPanel = quizObj.GetComponent<QuizPanel>();
            
            if (currentQuizPanel)
            {
                currentQuizPanel.Initialize(questions, this);
            }
        }
    }
    
    public void SubmitAnswer(int questionIndex, int answerIndex, QuizQuestion question)
    {
        totalQuestions++;
        
        bool isCorrect = (answerIndex == question.correctAnswerIndex);
        
        if (isCorrect)
        {
            correctAnswers++;
            PlaySound(correctAnswerSound);
            
            if (GameManager.Instance)
            {
                GameManager.Instance.AddScore(correctAnswerPoints);
            }
        }
        else
        {
            PlaySound(incorrectAnswerSound);
        }
        
        // Show explanation
        ShowAnswerExplanation(question, isCorrect);
    }
    
    void ShowAnswerExplanation(QuizQuestion question, bool wasCorrect)
    {
        if (currentQuizPanel)
        {
            currentQuizPanel.ShowExplanation(question.explanation, wasCorrect);
        }
    }
    
    public void CompleteQuiz(EducationalCategory category)
    {
        // Mark topic as completed
        foreach (EducationalTopic topic in topics)
        {
            if (topic.category == category && !topic.isCompleted)
            {
                topic.isCompleted = true;
                topicsCompleted++;
                
                PlaySound(topicCompleteSound);
                
                if (GameManager.Instance)
                {
                    GameManager.Instance.AddScore(topicCompletionPoints);
                }
                
                // Unlock content if available
                UnlockContent(topicsCompleted - 1);
                break;
            }
        }
        
        // Close quiz panel
        if (currentQuizPanel)
        {
            currentQuizPanel.CloseQuiz();
            currentQuizPanel = null;
        }
        
        // Resume game
        if (GameManager.Instance)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    void UnlockContent(int contentIndex)
    {
        if (unlockableContent != null && contentIndex < unlockableContent.Length)
        {
            if (unlockableContent[contentIndex])
            {
                unlockableContent[contentIndex].SetActive(true);
                Debug.Log("Unlocked new content: " + unlockableContent[contentIndex].name);
            }
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (educationalAudioSource && clip)
        {
            educationalAudioSource.clip = clip;
            educationalAudioSource.Play();
        }
    }
    
    public float GetLearningProgress()
    {
        return topics.Length > 0 ? (float)topicsCompleted / topics.Length : 0f;
    }
    
    public float GetQuizAccuracy()
    {
        return totalQuestions > 0 ? (float)correctAnswers / totalQuestions : 0f;
    }
    
    public string GetProgressSummary()
    {
        return $"Topics Completed: {topicsCompleted}/{topics.Length}\n" +
               $"Quiz Accuracy: {GetQuizAccuracy():P0}\n" +
               $"Total Questions: {totalQuestions}";
    }
}

// Component for educational checkpoints
public class EducationalCheckpoint : MonoBehaviour
{
    public EducationalSystem.EducationalCategory category;
    public float triggerRadius = 5f;
    public bool hasBeenTriggered = false;
    
    [Header("Visual Feedback")]
    public GameObject highlightEffect;
    public Color checkpointColor = Color.blue;
    
    private EducationalSystem educationalSystem;
    private SphereCollider triggerCollider;
    
    public void Initialize(EducationalSystem system)
    {
        educationalSystem = system;
        
        // Setup trigger collider
        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius;
        
        // Setup visual feedback
        SetupVisualFeedback();
    }
    
    void SetupVisualFeedback()
    {
        if (!highlightEffect)
        {
            // Create a simple highlight effect
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            highlight.transform.SetParent(transform);
            highlight.transform.localPosition = Vector3.zero;
            highlight.transform.localScale = new Vector3(triggerRadius * 2, 0.1f, triggerRadius * 2);
            
            Renderer renderer = highlight.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = checkpointColor;
            mat.SetFloat("_Mode", 3); // Transparent mode
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            
            Color color = mat.color;
            color.a = 0.3f;
            mat.color = color;
            
            renderer.material = mat;
            
            // Remove collider from highlight
            Destroy(highlight.GetComponent<Collider>());
            
            highlightEffect = highlight;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered && educationalSystem)
        {
            hasBeenTriggered = true;
            educationalSystem.TriggerEducationalContent(category, transform.position);
            
            // Hide highlight effect
            if (highlightEffect)
            {
                highlightEffect.SetActive(false);
            }
        }
    }
    
    public void SetupCheckpoint(string content)
    {
        // This method can be called to setup checkpoint with specific content
        // Implementation depends on specific requirements
    }
}

// UI component for quiz panel
public class QuizPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Text questionText;
    public Button[] answerButtons;
    public Text explanationText;
    public GameObject explanationPanel;
    public Button nextButton;
    public Button closeButton;
    public Text progressText;
    
    private List<EducationalSystem.QuizQuestion> questions;
    private EducationalSystem educationalSystem;
    private int currentQuestionIndex = 0;
    private bool showingExplanation = false;
    
    public void Initialize(List<EducationalSystem.QuizQuestion> quizQuestions, EducationalSystem system)
    {
        questions = quizQuestions;
        educationalSystem = system;
        currentQuestionIndex = 0;
        
        SetupButtons();
        ShowCurrentQuestion();
    }
    
    void SetupButtons()
    {
        if (nextButton)
            nextButton.onClick.AddListener(NextQuestion);
        
        if (closeButton)
            closeButton.onClick.AddListener(CloseQuiz);
        
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = i;
            answerButtons[i].onClick.AddListener(() => SelectAnswer(answerIndex));
        }
    }
    
    void ShowCurrentQuestion()
    {
        if (currentQuestionIndex < questions.Count)
        {
            EducationalSystem.QuizQuestion question = questions[currentQuestionIndex];
            
            if (questionText)
                questionText.text = question.question;
            
            // Setup answer buttons
            for (int i = 0; i < answerButtons.Length && i < question.answers.Length; i++)
            {
                answerButtons[i].GetComponentInChildren<Text>().text = question.answers[i];
                answerButtons[i].gameObject.SetActive(true);
                answerButtons[i].interactable = true;
            }
            
            // Hide unused buttons
            for (int i = question.answers.Length; i < answerButtons.Length; i++)
            {
                answerButtons[i].gameObject.SetActive(false);
            }
            
            if (progressText)
                progressText.text = $"Question {currentQuestionIndex + 1} of {questions.Count}";
            
            if (explanationPanel)
                explanationPanel.SetActive(false);
            
            showingExplanation = false;
        }
    }
    
    void SelectAnswer(int answerIndex)
    {
        if (showingExplanation) return;
        
        EducationalSystem.QuizQuestion question = questions[currentQuestionIndex];
        
        // Disable all answer buttons
        foreach (Button button in answerButtons)
        {
            button.interactable = false;
        }
        
        // Highlight correct answer
        answerButtons[question.correctAnswerIndex].GetComponent<Image>().color = Color.green;
        
        // Highlight selected answer if wrong
        if (answerIndex != question.correctAnswerIndex)
        {
            answerButtons[answerIndex].GetComponent<Image>().color = Color.red;
        }
        
        // Submit answer to educational system
        if (educationalSystem)
        {
            educationalSystem.SubmitAnswer(currentQuestionIndex, answerIndex, question);
        }
    }
    
    public void ShowExplanation(string explanation, bool wasCorrect)
    {
        if (explanationPanel && explanationText)
        {
            explanationPanel.SetActive(true);
            explanationText.text = explanation;
            showingExplanation = true;
            
            if (nextButton)
                nextButton.gameObject.SetActive(true);
        }
    }
    
    void NextQuestion()
    {
        currentQuestionIndex++;
        
        if (currentQuestionIndex < questions.Count)
        {
            // Reset button colors
            foreach (Button button in answerButtons)
            {
                button.GetComponent<Image>().color = Color.white;
            }
            
            ShowCurrentQuestion();
        }
        else
        {
            // Quiz completed
            CompleteQuiz();
        }
    }
    
    void CompleteQuiz()
    {
        if (educationalSystem && questions.Count > 0)
        {
            educationalSystem.CompleteQuiz(questions[0].category);
        }
    }
    
    public void CloseQuiz()
    {
        Destroy(gameObject);
    }
}
