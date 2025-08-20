using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Terrain Settings")]
    public Terrain terrain;
    public int terrainWidth = 500;
    public int terrainLength = 500;
    public int heightmapResolution = 513;
    public float maxHeight = 30f;
    
    [Header("Path Settings")]
    public float pathWidth = 12f;
    public float pathLength = 2000f;
    public AnimationCurve pathElevation;
    
    [Header("Vegetation")]
    public GameObject[] treePrefabs;
    public GameObject[] bushPrefabs;
    public GameObject[] rockPrefabs;
    public int maxTrees = 2000;
    public int maxBushes = 1000;
    public int maxRocks = 500;
    
    [Header("Australian Outback Elements")]
    public GameObject[] eucalyptusTrees;
    public GameObject[] acaciaTrees;
    public GameObject[] outbackRocks;
    public GameObject[] nativeGrasses;
    
    [Header("Landmarks")]
    public GameObject operaHousePrefab;
    public GameObject uluruPrefab;
    public Transform[] landmarkPositions;
    
    [Header("Wildlife")]
    public GameObject[] kangarooPrefabs;
    public GameObject[] koalaPrefabs;
    public GameObject[] wombatPrefabs;
    public int maxWildlife = 50;
    
    [Header("Collectibles")]
    public GameObject coinPrefab;
    public GameObject fruitPrefab;
    public GameObject culturalArtifactPrefab;
    public int coinsPerKm = 20;
    
    [Header("Educational Checkpoints")]
    public GameObject checkpointPrefab;
    public float checkpointInterval = 400f;
    
    private TerrainData terrainData;
    private List<Vector3> spawnedPositions = new List<Vector3>();
    
    void Start()
    {
        GenerateLevel();
    }
    
    void GenerateLevel()
    {
        CreateTerrain();
        GeneratePath();
        PlaceVegetation();
        PlaceLandmarks();
        SpawnWildlife();
        PlaceCollectibles();
        SetupEducationalCheckpoints();
        OptimizeForMobile();
    }
    
    void CreateTerrain()
    {
        if (!terrain)
        {
            GameObject terrainObj = Terrain.CreateTerrainGameObject(null);
            terrain = terrainObj.GetComponent<Terrain>();
        }
        
        terrainData = terrain.terrainData;
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = new Vector3(terrainWidth, maxHeight, terrainLength);
        
        // Generate heightmap for Australian outback terrain
        GenerateHeightmap();
        
        // Apply textures
        ApplyTerrainTextures();
    }
    
    void GenerateHeightmap()
    {
        float[,] heights = new float[heightmapResolution, heightmapResolution];
        
        for (int x = 0; x < heightmapResolution; x++)
        {
            for (int y = 0; y < heightmapResolution; y++)
            {
                float xCoord = (float)x / heightmapResolution * 5f;
                float yCoord = (float)y / heightmapResolution * 5f;
                
                // Create rolling hills typical of Australian outback
                float height = Mathf.PerlinNoise(xCoord, yCoord) * 0.1f;
                height += Mathf.PerlinNoise(xCoord * 2f, yCoord * 2f) * 0.05f;
                height += Mathf.PerlinNoise(xCoord * 4f, yCoord * 4f) * 0.025f;
                
                heights[x, y] = height;
            }
        }
        
        terrainData.SetHeights(0, 0, heights);
    }
    
    void ApplyTerrainTextures()
    {
        // This would typically load and apply terrain textures
        // For now, we'll set up the texture array structure
        TerrainLayer[] terrainLayers = new TerrainLayer[4];
        
        // Red dirt base
        terrainLayers[0] = new TerrainLayer();
        terrainLayers[0].tileSize = new Vector2(15, 15);
        
        // Grass patches
        terrainLayers[1] = new TerrainLayer();
        terrainLayers[1].tileSize = new Vector2(15, 15);
        
        // Rocky areas
        terrainLayers[2] = new TerrainLayer();
        terrainLayers[2].tileSize = new Vector2(15, 15);
        
        // Sand/dust
        terrainLayers[3] = new TerrainLayer();
        terrainLayers[3].tileSize = new Vector2(15, 15);
        
        terrainData.terrainLayers = terrainLayers;
    }
    
    void GeneratePath()
    {
        // Create the main running path through the outback
        Vector3 startPos = new Vector3(0, 0, 0);
        Vector3 endPos = new Vector3(0, 0, pathLength);
        
        // Smooth the terrain along the path
        SmoothPathTerrain(startPos, endPos);
        
        // Add path markers and lane dividers
        CreatePathMarkers();
    }
    
    void SmoothPathTerrain(Vector3 start, Vector3 end)
    {
        int pathWidthInHeightmap = Mathf.RoundToInt((pathWidth / terrainWidth) * heightmapResolution);
        int pathLengthInHeightmap = Mathf.RoundToInt((pathLength / terrainLength) * heightmapResolution);
        
        float[,] heights = terrainData.GetHeights(0, 0, heightmapResolution, heightmapResolution);
        
        for (int z = 0; z < pathLengthInHeightmap; z++)
        {
            int centerX = heightmapResolution / 2;
            
            for (int x = centerX - pathWidthInHeightmap/2; x < centerX + pathWidthInHeightmap/2; x++)
            {
                if (x >= 0 && x < heightmapResolution && z >= 0 && z < heightmapResolution)
                {
                    // Apply elevation curve for interesting path
                    float pathProgress = (float)z / pathLengthInHeightmap;
                    float elevation = pathElevation.Evaluate(pathProgress) * 0.1f;
                    heights[z, x] = elevation;
                }
            }
        }
        
        terrainData.SetHeights(0, 0, heights);
    }
    
    void CreatePathMarkers()
    {
        // Create lane markers and path indicators
        for (float z = 0; z < pathLength; z += 50f)
        {
            // Lane divider markers could be placed here
            Vector3 leftMarker = new Vector3(-pathWidth/3, 0, z);
            Vector3 rightMarker = new Vector3(pathWidth/3, 0, z);
            
            // Store positions for later use
            spawnedPositions.Add(leftMarker);
            spawnedPositions.Add(rightMarker);
        }
    }
    
    void PlaceVegetation()
    {
        PlaceTrees();
        PlaceBushes();
        PlaceRocks();
    }
    
    void PlaceTrees()
    {
        for (int i = 0; i < maxTrees; i++)
        {
            Vector3 position = GetRandomPositionAwayFromPath();
            
            if (IsValidSpawnPosition(position))
            {
                GameObject treePrefab = GetRandomAustralianTree();
                if (treePrefab)
                {
                    GameObject tree = Instantiate(treePrefab, position, GetRandomRotation());
                    OptimizeGameObject(tree);
                    spawnedPositions.Add(position);
                }
            }
        }
    }
    
    void PlaceBushes()
    {
        for (int i = 0; i < maxBushes; i++)
        {
            Vector3 position = GetRandomPositionAwayFromPath();
            
            if (IsValidSpawnPosition(position))
            {
                GameObject bushPrefab = GetRandomBush();
                if (bushPrefab)
                {
                    GameObject bush = Instantiate(bushPrefab, position, GetRandomRotation());
                    OptimizeGameObject(bush);
                    spawnedPositions.Add(position);
                }
            }
        }
    }
    
    void PlaceRocks()
    {
        for (int i = 0; i < maxRocks; i++)
        {
            Vector3 position = GetRandomPositionAwayFromPath();
            
            if (IsValidSpawnPosition(position))
            {
                GameObject rockPrefab = GetRandomRock();
                if (rockPrefab)
                {
                    GameObject rock = Instantiate(rockPrefab, position, GetRandomRotation());
                    OptimizeGameObject(rock);
                    spawnedPositions.Add(position);
                }
            }
        }
    }
    
    void PlaceLandmarks()
    {
        // Place Sydney Opera House at 25% mark
        if (operaHousePrefab && landmarkPositions.Length > 0)
        {
            Vector3 operaPos = new Vector3(50, 0, pathLength * 0.25f);
            GameObject opera = Instantiate(operaHousePrefab, operaPos, Quaternion.identity);
            OptimizeGameObject(opera);
        }
        
        // Place Uluru at 75% mark
        if (uluruPrefab && landmarkPositions.Length > 1)
        {
            Vector3 uluruPos = new Vector3(-50, 0, pathLength * 0.75f);
            GameObject uluru = Instantiate(uluruPrefab, uluruPos, Quaternion.identity);
            OptimizeGameObject(uluru);
        }
    }
    
    void SpawnWildlife()
    {
        for (int i = 0; i < maxWildlife; i++)
        {
            Vector3 position = GetRandomPositionAwayFromPath();
            
            if (IsValidSpawnPosition(position))
            {
                GameObject animalPrefab = GetRandomWildlife();
                if (animalPrefab)
                {
                    GameObject animal = Instantiate(animalPrefab, position, GetRandomRotation());
                    OptimizeGameObject(animal);
                    
                    // Add simple AI behavior
                    animal.AddComponent<SimpleAnimalAI>();
                    spawnedPositions.Add(position);
                }
            }
        }
    }
    
    void PlaceCollectibles()
    {
        int totalCoins = Mathf.RoundToInt((pathLength / 1000f) * coinsPerKm);
        
        for (int i = 0; i < totalCoins; i++)
        {
            Vector3 position = GetRandomPositionOnPath();
            
            GameObject collectible = GetRandomCollectible();
            if (collectible)
            {
                GameObject coin = Instantiate(collectible, position, Quaternion.identity);
                coin.tag = "Collectible";
                OptimizeGameObject(coin);
            }
        }
    }
    
    void SetupEducationalCheckpoints()
    {
        for (float z = checkpointInterval; z < pathLength; z += checkpointInterval)
        {
            Vector3 checkpointPos = new Vector3(0, 2, z);
            
            if (checkpointPrefab)
            {
                GameObject checkpoint = Instantiate(checkpointPrefab, checkpointPos, Quaternion.identity);
                EducationalCheckpoint eduComponent = checkpoint.GetComponent<EducationalCheckpoint>();
                
                if (eduComponent)
                {
                    eduComponent.SetupCheckpoint(GetEducationalContent(z));
                }
            }
        }
    }
    
    string GetEducationalContent(float distance)
    {
        if (distance < 500) return "Welcome to the Australian Outback! This vast landscape covers 70% of Australia.";
        if (distance < 1000) return "Kangaroos can hop at speeds up to 60 km/h and are excellent swimmers!";
        if (distance < 1500) return "Aboriginal Australians have lived here for over 65,000 years with deep cultural connections to the land.";
        if (distance < 2000) return "The outback is home to unique wildlife adapted to harsh conditions. Conservation is vital!";
        return "Congratulations! You've learned about Australia's amazing outback ecosystem!";
    }
    
    Vector3 GetRandomPositionAwayFromPath()
    {
        float x = Random.Range(-terrainWidth/2 + pathWidth, terrainWidth/2 - pathWidth);
        float z = Random.Range(0, pathLength);
        float y = terrain.SampleHeight(new Vector3(x, 0, z));
        
        return new Vector3(x, y, z);
    }
    
    Vector3 GetRandomPositionOnPath()
    {
        float x = Random.Range(-pathWidth/2, pathWidth/2);
        float z = Random.Range(0, pathLength);
        float y = terrain.SampleHeight(new Vector3(x, 0, z)) + 1f;
        
        return new Vector3(x, y, z);
    }
    
    bool IsValidSpawnPosition(Vector3 position)
    {
        foreach (Vector3 existing in spawnedPositions)
        {
            if (Vector3.Distance(position, existing) < 10f)
                return false;
        }
        return true;
    }
    
    GameObject GetRandomAustralianTree()
    {
        if (eucalyptusTrees.Length > 0)
            return eucalyptusTrees[Random.Range(0, eucalyptusTrees.Length)];
        if (treePrefabs.Length > 0)
            return treePrefabs[Random.Range(0, treePrefabs.Length)];
        return null;
    }
    
    GameObject GetRandomBush()
    {
        if (bushPrefabs.Length > 0)
            return bushPrefabs[Random.Range(0, bushPrefabs.Length)];
        return null;
    }
    
    GameObject GetRandomRock()
    {
        if (outbackRocks.Length > 0)
            return outbackRocks[Random.Range(0, outbackRocks.Length)];
        if (rockPrefabs.Length > 0)
            return rockPrefabs[Random.Range(0, rockPrefabs.Length)];
        return null;
    }
    
    GameObject GetRandomWildlife()
    {
        List<GameObject> allAnimals = new List<GameObject>();
        allAnimals.AddRange(kangarooPrefabs);
        allAnimals.AddRange(koalaPrefabs);
        allAnimals.AddRange(wombatPrefabs);
        
        if (allAnimals.Count > 0)
            return allAnimals[Random.Range(0, allAnimals.Count)];
        return null;
    }
    
    GameObject GetRandomCollectible()
    {
        int rand = Random.Range(0, 10);
        if (rand < 6 && coinPrefab) return coinPrefab;
        if (rand < 8 && fruitPrefab) return fruitPrefab;
        if (culturalArtifactPrefab) return culturalArtifactPrefab;
        return coinPrefab;
    }
    
    Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }
    
    void OptimizeGameObject(GameObject obj)
    {
        // Add LOD Group for mobile optimization
        LODGroup lodGroup = obj.GetComponent<LODGroup>();
        if (!lodGroup)
        {
            lodGroup = obj.AddComponent<LODGroup>();
            
            // Set up basic LOD levels
            LOD[] lods = new LOD[3];
            lods[0] = new LOD(0.6f, obj.GetComponentsInChildren<Renderer>());
            lods[1] = new LOD(0.3f, obj.GetComponentsInChildren<Renderer>());
            lods[2] = new LOD(0.1f, new Renderer[0]); // Cull at distance
            
            lodGroup.SetLODs(lods);
        }
    }
    
    void OptimizeForMobile()
    {
        // Set mobile-specific quality settings
        QualitySettings.shadowDistance = 75f;
        QualitySettings.shadowCascades = 2;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        
        // Enable occlusion culling
        Camera.main.useOcclusionCulling = true;
        
        // Optimize terrain settings
        if (terrain)
        {
            terrain.detailObjectDistance = 50f;
            terrain.treeDistance = 100f;
            terrain.treeBillboardDistance = 75f;
            terrain.treeCrossFadeLength = 5f;
        }
    }
}

// Simple AI for wildlife
public class SimpleAnimalAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float wanderRadius = 20f;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float changeDirectionTime = 3f;
    private float timer;
    
    void Start()
    {
        startPosition = transform.position;
        SetNewTarget();
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= changeDirectionTime)
        {
            SetNewTarget();
            timer = 0f;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            SetNewTarget();
        }
    }
    
    void SetNewTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        targetPosition = startPosition + new Vector3(randomDirection.x, 0, randomDirection.y);
    }
}
