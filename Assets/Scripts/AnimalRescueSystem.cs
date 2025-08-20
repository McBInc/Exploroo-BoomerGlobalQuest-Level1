using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class AnimalRescueSystem : MonoBehaviour
{
    [Header("Rescue Settings")]
    public int totalAnimalsToRescue = 15;
    public int animalsRescued = 0;
    public float rescueRadius = 3f;
    public float rescueTime = 2f;
    
    [Header("Animal Prefabs")]
    public GameObject[] kangarooPrefabs;
    public GameObject[] koalaPrefabs;
    public GameObject[] wombatPrefabs;
    public GameObject[] echidnaPrefabs;
    public GameObject[] possumPrefabs;
    
    [Header("Rescue UI")]
    public Slider rescueProgressBar;
    public Text rescueCountText;
    public GameObject rescuePrompt;
    public Text rescuePromptText;
    public Image rescueProgressCircle;
    
    [Header("Animal States")]
    public Material normalAnimalMaterial;
    public Material rescuableAnimalMaterial;
    public Material rescuedAnimalMaterial;
    
    [Header("Effects")]
    public GameObject rescueEffect;
    public GameObject healingEffect;
    public AudioClip rescueSound;
    public AudioClip animalHappySound;
    public AudioSource rescueAudioSource;
    
    [Header("Rewards")]
    public int rescuePoints = 100;
    public int bonusPointsPerAnimal = 50;
    public GameObject[] unlockableContent;
    
    private List<RescuableAnimal> spawnedAnimals = new List<RescuableAnimal>();
    private RescuableAnimal currentRescueTarget;
    private bool isRescuing = false;
    private Coroutine rescueCoroutine;
    
    [System.Serializable]
    public class AnimalSpawnPoint
    {
        public Vector3 position;
        public AnimalType animalType;
        public RescueCondition condition;
        public bool hasBeenRescued = false;
    }
    
    public enum AnimalType
    {
        Kangaroo,
        Koala,
        Wombat,
        Echidna,
        Possum
    }
    
    public enum RescueCondition
    {
        Injured,
        Trapped,
        Lost,
        Sick,
        Orphaned
    }
    
    void Start()
    {
        InitializeRescueSystem();
        SpawnAnimalsInNeed();
        SetupUI();
    }
    
    void InitializeRescueSystem()
    {
        if (rescueAudioSource == null)
        {
            rescueAudioSource = gameObject.AddComponent<AudioSource>();
            rescueAudioSource.volume = 0.8f;
            rescueAudioSource.spatialBlend = 0f;
        }
        
        if (rescuePrompt) rescuePrompt.SetActive(false);
    }
    
    void SpawnAnimalsInNeed()
    {
        for (int i = 0; i < totalAnimalsToRescue; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            AnimalType animalType = GetRandomAnimalType();
            RescueCondition condition = GetRandomRescueCondition();
            
            SpawnRescuableAnimal(spawnPosition, animalType, condition);
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // Spawn animals along the path but slightly off to the sides
        float z = Random.Range(200f, 1800f); // Along the level length
        float x = Random.Range(-50f, 50f); // To the sides of the path
        float y = 0f;
        
        // Sample terrain height if terrain exists
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain)
        {
            y = terrain.SampleHeight(new Vector3(x, 0, z));
        }
        
        return new Vector3(x, y + 1f, z);
    }
    
    AnimalType GetRandomAnimalType()
    {
        return (AnimalType)Random.Range(0, System.Enum.GetValues(typeof(AnimalType)).Length);
    }
    
    RescueCondition GetRandomRescueCondition()
    {
        return (RescueCondition)Random.Range(0, System.Enum.GetValues(typeof(RescueCondition)).Length);
    }
    
    void SpawnRescuableAnimal(Vector3 position, AnimalType type, RescueCondition condition)
    {
        GameObject animalPrefab = GetAnimalPrefab(type);
        if (animalPrefab)
        {
            GameObject animal = Instantiate(animalPrefab, position, GetRandomRotation());
            RescuableAnimal rescuableComponent = animal.AddComponent<RescuableAnimal>();
            
            rescuableComponent.Initialize(type, condition, this);
            spawnedAnimals.Add(rescuableComponent);
            
            // Add visual indicators for rescue state
            SetupAnimalVisuals(animal, condition);
        }
    }
    
    GameObject GetAnimalPrefab(AnimalType type)
    {
        switch (type)
        {
            case AnimalType.Kangaroo:
                return kangarooPrefabs.Length > 0 ? kangarooPrefabs[Random.Range(0, kangarooPrefabs.Length)] : null;
            case AnimalType.Koala:
                return koalaPrefabs.Length > 0 ? koalaPrefabs[Random.Range(0, koalaPrefabs.Length)] : null;
            case AnimalType.Wombat:
                return wombatPrefabs.Length > 0 ? wombatPrefabs[Random.Range(0, wombatPrefabs.Length)] : null;
            case AnimalType.Echidna:
                return echidnaPrefabs.Length > 0 ? echidnaPrefabs[Random.Range(0, echidnaPrefabs.Length)] : null;
            case AnimalType.Possum:
                return possumPrefabs.Length > 0 ? possumPrefabs[Random.Range(0, possumPrefabs.Length)] : null;
            default:
                return null;
        }
    }
    
    void SetupAnimalVisuals(GameObject animal, RescueCondition condition)
    {
        // Add highlight effect for rescuable animals
        Renderer renderer = animal.GetComponent<Renderer>();
        if (renderer && rescuableAnimalMaterial)
        {
            renderer.material = rescuableAnimalMaterial;
        }
        
        // Add condition-specific visual effects
        AddConditionEffects(animal, condition);
        
        // Add rescue trigger collider
        SphereCollider rescueCollider = animal.AddComponent<SphereCollider>();
        rescueCollider.isTrigger = true;
        rescueCollider.radius = rescueRadius;
    }
    
    void AddConditionEffects(GameObject animal, RescueCondition condition)
    {
        switch (condition)
        {
            case RescueCondition.Injured:
                // Add bandage or injury visual
                CreateStatusIndicator(animal, "Injured", Color.red);
                break;
            case RescueCondition.Trapped:
                // Add trap or cage visual
                CreateStatusIndicator(animal, "Trapped", Color.yellow);
                break;
            case RescueCondition.Lost:
                // Add confused animation or lost indicator
                CreateStatusIndicator(animal, "Lost", Color.blue);
                break;
            case RescueCondition.Sick:
                // Add sick visual effects
                CreateStatusIndicator(animal, "Sick", Color.green);
                break;
            case RescueCondition.Orphaned:
                // Add orphaned indicator
                CreateStatusIndicator(animal, "Orphaned", Color.purple);
                break;
        }
    }
    
    void CreateStatusIndicator(GameObject animal, string status, Color color)
    {
        // Create a simple text indicator above the animal
        GameObject indicator = new GameObject("StatusIndicator");
        indicator.transform.SetParent(animal.transform);
        indicator.transform.localPosition = Vector3.up * 2f;
        
        Canvas canvas = indicator.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        Text statusText = indicator.AddComponent<Text>();
        statusText.text = status;
        statusText.color = color;
        statusText.fontSize = 14;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        
        RectTransform rectTransform = indicator.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 30);
        
        // Make it face the camera
        indicator.AddComponent<Billboard>();
    }
    
    Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }
    
    void SetupUI()
    {
        UpdateRescueUI();
    }
    
    void Update()
    {
        CheckForNearbyAnimals();
        HandleRescueInput();
        UpdateRescueUI();
    }
    
    void CheckForNearbyAnimals()
    {
        if (isRescuing) return;
        
        BoomerController player = FindObjectOfType<BoomerController>();
        if (!player) return;
        
        RescuableAnimal nearestAnimal = null;
        float nearestDistance = float.MaxValue;
        
        foreach (RescuableAnimal animal in spawnedAnimals)
        {
            if (animal && !animal.isRescued)
            {
                float distance = Vector3.Distance(player.transform.position, animal.transform.position);
                if (distance <= rescueRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestAnimal = animal;
                }
            }
        }
        
        if (nearestAnimal != currentRescueTarget)
        {
            currentRescueTarget = nearestAnimal;
            
            if (currentRescueTarget)
            {
                ShowRescuePrompt(currentRescueTarget);
            }
            else
            {
                HideRescuePrompt();
            }
        }
    }
    
    void ShowRescuePrompt(RescuableAnimal animal)
    {
        if (rescuePrompt)
        {
            rescuePrompt.SetActive(true);
            
            if (rescuePromptText)
            {
                string conditionText = GetConditionDescription(animal.condition);
                rescuePromptText.text = $"Rescue {animal.animalType}\n{conditionText}\nHold to rescue";
            }
        }
    }
    
    void HideRescuePrompt()
    {
        if (rescuePrompt)
        {
            rescuePrompt.SetActive(false);
        }
    }
    
    string GetConditionDescription(RescueCondition condition)
    {
        switch (condition)
        {
            case RescueCondition.Injured: return "This animal is injured and needs medical attention";
            case RescueCondition.Trapped: return "This animal is trapped and needs to be freed";
            case RescueCondition.Lost: return "This animal is lost and needs to find its family";
            case RescueCondition.Sick: return "This animal is sick and needs care";
            case RescueCondition.Orphaned: return "This orphaned animal needs a safe home";
            default: return "This animal needs help";
        }
    }
    
    void HandleRescueInput()
    {
        if (!currentRescueTarget || currentRescueTarget.isRescued) return;
        
        bool rescueInput = false;
        
        // Check for rescue input (touch hold or key hold)
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space))
        {
            rescueInput = true;
        }
        
        // Check for touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                rescueInput = true;
            }
        }
        
        if (rescueInput && !isRescuing)
        {
            StartRescue(currentRescueTarget);
        }
        else if (!rescueInput && isRescuing)
        {
            CancelRescue();
        }
    }
    
    void StartRescue(RescuableAnimal animal)
    {
        if (isRescuing) return;
        
        isRescuing = true;
        rescueCoroutine = StartCoroutine(RescueProcess(animal));
    }
    
    void CancelRescue()
    {
        if (rescueCoroutine != null)
        {
            StopCoroutine(rescueCoroutine);
            rescueCoroutine = null;
        }
        
        isRescuing = false;
        
        if (rescueProgressCircle)
            rescueProgressCircle.fillAmount = 0f;
    }
    
    IEnumerator RescueProcess(RescuableAnimal animal)
    {
        float elapsed = 0f;
        
        while (elapsed < rescueTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / rescueTime;
            
            if (rescueProgressCircle)
                rescueProgressCircle.fillAmount = progress;
            
            yield return null;
        }
        
        // Complete rescue
        CompleteRescue(animal);
    }
    
    void CompleteRescue(RescuableAnimal animal)
    {
        if (!animal || animal.isRescued) return;
        
        animal.isRescued = true;
        animalsRescued++;
        
        // Visual and audio feedback
        PlayRescueEffects(animal);
        
        // Award points
        int points = rescuePoints + (bonusPointsPerAnimal * animalsRescued);
        if (GameManager.Instance)
        {
            GameManager.Instance.AddScore(points);
        }
        
        // Update animal visual state
        UpdateAnimalVisuals(animal.gameObject, true);
        
        // Educational content
        ShowRescueEducation(animal);
        
        // Check for completion
        CheckRescueCompletion();
        
        // Reset rescue state
        isRescuing = false;
        currentRescueTarget = null;
        HideRescuePrompt();
        
        if (rescueProgressCircle)
            rescueProgressCircle.fillAmount = 0f;
    }
    
    void PlayRescueEffects(RescuableAnimal animal)
    {
        // Play rescue sound
        if (rescueAudioSource && rescueSound)
        {
            rescueAudioSource.clip = rescueSound;
            rescueAudioSource.Play();
        }
        
        // Play animal happy sound
        if (animalHappySound)
        {
            AudioSource.PlayClipAtPoint(animalHappySound, animal.transform.position);
        }
        
        // Spawn rescue effect
        if (rescueEffect)
        {
            Instantiate(rescueEffect, animal.transform.position, Quaternion.identity);
        }
        
        // Spawn healing effect
        if (healingEffect)
        {
            GameObject healing = Instantiate(healingEffect, animal.transform.position, Quaternion.identity);
            Destroy(healing, 3f);
        }
    }
    
    void UpdateAnimalVisuals(GameObject animal, bool rescued)
    {
        Renderer renderer = animal.GetComponent<Renderer>();
        if (renderer)
        {
            renderer.material = rescued ? rescuedAnimalMaterial : rescuableAnimalMaterial;
        }
        
        // Remove status indicator
        Transform statusIndicator = animal.transform.Find("StatusIndicator");
        if (statusIndicator)
        {
            Destroy(statusIndicator.gameObject);
        }
        
        // Add rescued indicator
        if (rescued)
        {
            CreateStatusIndicator(animal, "Rescued!", Color.green);
        }
    }
    
    void ShowRescueEducation(RescuableAnimal animal)
    {
        string educationalContent = GetEducationalContent(animal.animalType, animal.condition);
        
        if (GameManager.Instance)
        {
            GameManager.Instance.ShowEducationalContent(educationalContent);
        }
    }
    
    string GetEducationalContent(AnimalType type, RescueCondition condition)
    {
        string baseInfo = GetAnimalInfo(type);
        string conditionInfo = GetConditionInfo(condition);
        
        return $"{baseInfo}\n\n{conditionInfo}\n\nGreat job helping this animal! Wildlife rescue is important for conservation.";
    }
    
    string GetAnimalInfo(AnimalType type)
    {
        switch (type)
        {
            case AnimalType.Kangaroo:
                return "Kangaroos are Australia's largest marsupials. They can hop at speeds up to 60 km/h and are excellent swimmers.";
            case AnimalType.Koala:
                return "Koalas are not bears but marsupials. They sleep 18-22 hours a day and eat only eucalyptus leaves.";
            case AnimalType.Wombat:
                return "Wombats are powerful diggers with cube-shaped droppings. Their burrows can be up to 30 meters long.";
            case AnimalType.Echidna:
                return "Echidnas are one of only two egg-laying mammals. They use their long snouts to find ants and termites.";
            case AnimalType.Possum:
                return "Possums are nocturnal marsupials that play dead when threatened. They're excellent climbers.";
            default:
                return "This is a unique Australian animal that needs our protection.";
        }
    }
    
    string GetConditionInfo(RescueCondition condition)
    {
        switch (condition)
        {
            case RescueCondition.Injured:
                return "Injured animals need immediate veterinary care. Wildlife hospitals help thousands of animals each year.";
            case RescueCondition.Trapped:
                return "Animals can get trapped in fences, buildings, or human-made structures. Quick rescue prevents further injury.";
            case RescueCondition.Lost:
                return "Young animals sometimes get separated from their families. Reuniting them is crucial for their survival.";
            case RescueCondition.Sick:
                return "Sick animals need medical treatment and rehabilitation before they can return to the wild.";
            case RescueCondition.Orphaned:
                return "Orphaned animals need special care and feeding until they're old enough to survive independently.";
            default:
                return "Every animal rescue makes a difference in conservation efforts.";
        }
    }
    
    void CheckRescueCompletion()
    {
        if (animalsRescued >= totalAnimalsToRescue)
        {
            // All animals rescued!
            CompleteAllRescues();
        }
    }
    
    void CompleteAllRescues()
    {
        // Award completion bonus
        int completionBonus = 1000;
        if (GameManager.Instance)
        {
            GameManager.Instance.AddScore(completionBonus);
        }
        
        // Show completion message
        string completionMessage = $"Amazing! You've rescued all {totalAnimalsToRescue} animals!\n\n" +
                                 "You're a true wildlife hero. Thanks to your efforts, these animals can return to their natural habitat safely.";
        
        if (GameManager.Instance)
        {
            GameManager.Instance.ShowEducationalContent(completionMessage);
        }
        
        // Unlock special content
        UnlockRescueContent();
    }
    
    void UnlockRescueContent()
    {
        foreach (GameObject content in unlockableContent)
        {
            if (content)
            {
                content.SetActive(true);
            }
        }
    }
    
    void UpdateRescueUI()
    {
        if (rescueCountText)
            rescueCountText.text = $"Animals Rescued: {animalsRescued}/{totalAnimalsToRescue}";
        
        if (rescueProgressBar)
        {
            rescueProgressBar.value = (float)animalsRescued / totalAnimalsToRescue;
        }
    }
    
    public int GetAnimalsRescued()
    {
        return animalsRescued;
    }
    
    public float GetRescueProgress()
    {
        return (float)animalsRescued / totalAnimalsToRescue;
    }
}

// Component for individual rescuable animals
public class RescuableAnimal : MonoBehaviour
{
    public AnimalRescueSystem.AnimalType animalType;
    public AnimalRescueSystem.RescueCondition condition;
    public bool isRescued = false;
    
    private AnimalRescueSystem rescueSystem;
    
    public void Initialize(AnimalRescueSystem.AnimalType type, AnimalRescueSystem.RescueCondition cond, AnimalRescueSystem system)
    {
        animalType = type;
        condition = cond;
        rescueSystem = system;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isRescued)
        {
            // Player is near - rescue system will handle the interaction
        }
    }
}

// Simple billboard component to make UI elements face camera
public class Billboard : MonoBehaviour
{
    void Update()
    {
        if (Camera.main)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // Face the camera properly
        }
    }
}
