using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CulturalSystem : MonoBehaviour
{
    [Header("Cultural Content")]
    public CulturalFact[] culturalFacts;
    public GameObject culturalPopupPrefab;
    public Transform culturalUIParent;
    
    [Header("Aboriginal Elements")]
    public GameObject[] dreamtimeArtifacts;
    public GameObject[] traditionalTools;
    public GameObject[] sacredSites;
    
    [Header("Audio")]
    public AudioClip[] aboriginalMusic;
    public AudioClip[] didgeridooSounds;
    public AudioSource culturalAudioSource;
    
    [Header("Visual Elements")]
    public Material[] aboriginalArtMaterials;
    public Texture2D[] dreamtimePaintings;
    public Color[] traditionalColors;
    
    private List<CulturalCheckpoint> activeCheckpoints = new List<CulturalCheckpoint>();
    private int factsDiscovered = 0;
    
    [System.Serializable]
    public class CulturalFact
    {
        public string title;
        [TextArea(3, 5)]
        public string description;
        public Sprite illustration;
        public AudioClip narration;
        public CulturalCategory category;
    }
    
    public enum CulturalCategory
    {
        History,
        Traditions,
        Wildlife,
        Landmarks,
        Conservation,
        Language,
        Art,
        Spirituality
    }
    
    void Start()
    {
        InitializeCulturalSystem();
        SetupCulturalCheckpoints();
    }
    
    void InitializeCulturalSystem()
    {
        // Initialize cultural facts if not set in inspector
        if (culturalFacts == null || culturalFacts.Length == 0)
        {
            CreateDefaultCulturalFacts();
        }
        
        // Setup cultural audio
        if (culturalAudioSource == null)
        {
            culturalAudioSource = gameObject.AddComponent<AudioSource>();
            culturalAudioSource.volume = 0.7f;
            culturalAudioSource.spatialBlend = 0f; // 2D audio for UI
        }
    }
    
    void CreateDefaultCulturalFacts()
    {
        culturalFacts = new CulturalFact[]
        {
            new CulturalFact
            {
                title = "Aboriginal Heritage",
                description = "Aboriginal Australians are the world's oldest continuous culture, with a history spanning over 65,000 years. Their deep connection to the land is expressed through Dreamtime stories, art, and traditional practices.",
                category = CulturalCategory.History
            },
            new CulturalFact
            {
                title = "Dreamtime Stories",
                description = "The Dreamtime is the Aboriginal understanding of the world's creation. These stories explain how ancestral spirits created the land, animals, and laws that govern Aboriginal society.",
                category = CulturalCategory.Spirituality
            },
            new CulturalFact
            {
                title = "Traditional Land Management",
                description = "Aboriginal people used controlled burning and sustainable hunting practices to manage the land for thousands of years, creating the landscapes we see today.",
                category = CulturalCategory.Conservation
            },
            new CulturalFact
            {
                title = "Sacred Sites",
                description = "Many locations across Australia hold deep spiritual significance for Aboriginal people. These sites are protected and respected as part of living culture.",
                category = CulturalCategory.Landmarks
            },
            new CulturalFact
            {
                title = "Wildlife Connection",
                description = "Aboriginal culture includes deep knowledge of native animals. Many animals feature in Dreamtime stories and are considered totems with special spiritual significance.",
                category = CulturalCategory.Wildlife
            },
            new CulturalFact
            {
                title = "Art and Symbols",
                description = "Aboriginal art uses symbols and patterns that have been passed down for thousands of years. Each symbol has meaning and tells stories of country, family, and spirituality.",
                category = CulturalCategory.Art
            },
            new CulturalFact
            {
                title = "Language Diversity",
                description = "Before European settlement, over 250 Aboriginal languages were spoken across Australia. Today, efforts continue to preserve and revitalize these important languages.",
                category = CulturalCategory.Language
            },
            new CulturalFact
            {
                title = "Traditional Tools",
                description = "Aboriginal people developed sophisticated tools like boomerangs, spears, and digging sticks. Each tool was perfectly adapted to its environment and purpose.",
                category = CulturalCategory.Traditions
            }
        };
    }
    
    void SetupCulturalCheckpoints()
    {
        // Find all cultural checkpoints in the scene
        CulturalCheckpoint[] checkpoints = FindObjectsOfType<CulturalCheckpoint>();
        
        foreach (CulturalCheckpoint checkpoint in checkpoints)
        {
            checkpoint.Initialize(this);
            activeCheckpoints.Add(checkpoint);
        }
    }
    
    public void TriggerCulturalContent(CulturalCategory category, Vector3 position)
    {
        CulturalFact fact = GetFactByCategory(category);
        if (fact != null)
        {
            ShowCulturalPopup(fact, position);
            PlayCulturalAudio(fact);
            factsDiscovered++;
            
            // Award points for cultural discovery
            if (GameManager.Instance)
            {
                GameManager.Instance.AddScore(100);
            }
        }
    }
    
    CulturalFact GetFactByCategory(CulturalCategory category)
    {
        List<CulturalFact> categoryFacts = new List<CulturalFact>();
        
        foreach (CulturalFact fact in culturalFacts)
        {
            if (fact.category == category)
            {
                categoryFacts.Add(fact);
            }
        }
        
        if (categoryFacts.Count > 0)
        {
            return categoryFacts[Random.Range(0, categoryFacts.Count)];
        }
        
        // Fallback to any random fact
        return culturalFacts[Random.Range(0, culturalFacts.Length)];
    }
    
    void ShowCulturalPopup(CulturalFact fact, Vector3 worldPosition)
    {
        if (culturalPopupPrefab && culturalUIParent)
        {
            GameObject popup = Instantiate(culturalPopupPrefab, culturalUIParent);
            CulturalPopup popupComponent = popup.GetComponent<CulturalPopup>();
            
            if (popupComponent)
            {
                popupComponent.DisplayFact(fact);
            }
            
            // Position popup near the trigger point
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            popup.transform.position = screenPos;
        }
    }
    
    void PlayCulturalAudio(CulturalFact fact)
    {
        if (culturalAudioSource)
        {
            // Play narration if available
            if (fact.narration)
            {
                culturalAudioSource.clip = fact.narration;
                culturalAudioSource.Play();
            }
            // Otherwise play ambient cultural music
            else if (aboriginalMusic.Length > 0)
            {
                culturalAudioSource.clip = aboriginalMusic[Random.Range(0, aboriginalMusic.Length)];
                culturalAudioSource.Play();
            }
        }
    }
    
    public void PlaceCulturalArtifacts()
    {
        // Place dreamtime artifacts throughout the level
        if (dreamtimeArtifacts.Length > 0)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 position = GetRandomCulturalPosition();
                GameObject artifact = Instantiate(dreamtimeArtifacts[Random.Range(0, dreamtimeArtifacts.Length)], position, Quaternion.identity);
                
                // Add cultural trigger
                CulturalTrigger trigger = artifact.AddComponent<CulturalTrigger>();
                trigger.category = CulturalCategory.Art;
                trigger.culturalSystem = this;
            }
        }
        
        // Place traditional tools
        if (traditionalTools.Length > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector3 position = GetRandomCulturalPosition();
                GameObject tool = Instantiate(traditionalTools[Random.Range(0, traditionalTools.Length)], position, Quaternion.identity);
                
                CulturalTrigger trigger = tool.AddComponent<CulturalTrigger>();
                trigger.category = CulturalCategory.Traditions;
                trigger.culturalSystem = this;
            }
        }
        
        // Mark sacred sites
        if (sacredSites.Length > 0)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 position = GetRandomCulturalPosition();
                GameObject site = Instantiate(sacredSites[Random.Range(0, sacredSites.Length)], position, Quaternion.identity);
                
                CulturalTrigger trigger = site.AddComponent<CulturalTrigger>();
                trigger.category = CulturalCategory.Spirituality;
                trigger.culturalSystem = this;
            }
        }
    }
    
    Vector3 GetRandomCulturalPosition()
    {
        // Get a random position away from the main path but visible
        float x = Random.Range(-200f, 200f);
        float z = Random.Range(100f, 1900f);
        float y = 0f;
        
        // Sample terrain height if terrain exists
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain)
        {
            y = terrain.SampleHeight(new Vector3(x, 0, z));
        }
        
        return new Vector3(x, y + 1f, z);
    }
    
    public int GetFactsDiscovered()
    {
        return factsDiscovered;
    }
    
    public float GetCulturalProgress()
    {
        return (float)factsDiscovered / culturalFacts.Length;
    }
    
    // Method to ensure cultural sensitivity
    public bool ValidateCulturalContent()
    {
        // This method would typically validate content with cultural consultants
        // For now, it returns true but in production should include proper validation
        Debug.Log("Cultural content validation required - consult with Aboriginal cultural advisors");
        return true;
    }
}

// Component for cultural checkpoints
public class CulturalCheckpoint : MonoBehaviour
{
    public CulturalSystem.CulturalCategory category;
    public float triggerRadius = 5f;
    public bool hasBeenTriggered = false;
    
    private CulturalSystem culturalSystem;
    private SphereCollider triggerCollider;
    
    public void Initialize(CulturalSystem system)
    {
        culturalSystem = system;
        
        // Setup trigger collider
        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered && culturalSystem)
        {
            hasBeenTriggered = true;
            culturalSystem.TriggerCulturalContent(category, transform.position);
        }
    }
}

// Component for cultural artifacts and triggers
public class CulturalTrigger : MonoBehaviour
{
    public CulturalSystem.CulturalCategory category;
    public CulturalSystem culturalSystem;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && culturalSystem)
        {
            culturalSystem.TriggerCulturalContent(category, transform.position);
            
            // Remove the artifact after discovery
            Destroy(gameObject, 1f);
        }
    }
    
    void Start()
    {
        // Add trigger collider if not present
        if (!GetComponent<Collider>())
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 2f;
        }
    }
}

// UI component for displaying cultural facts
public class CulturalPopup : MonoBehaviour
{
    [Header("UI Elements")]
    public Text titleText;
    public Text descriptionText;
    public Image illustrationImage;
    public Button closeButton;
    
    public float displayDuration = 5f;
    
    void Start()
    {
        if (closeButton)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
        
        // Auto-close after duration
        Invoke(nameof(ClosePopup), displayDuration);
    }
    
    public void DisplayFact(CulturalSystem.CulturalFact fact)
    {
        if (titleText) titleText.text = fact.title;
        if (descriptionText) descriptionText.text = fact.description;
        if (illustrationImage && fact.illustration) illustrationImage.sprite = fact.illustration;
    }
    
    void ClosePopup()
    {
        Destroy(gameObject);
    }
}
