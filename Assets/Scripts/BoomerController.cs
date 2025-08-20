using UnityEngine;
using UnityEngine.InputSystem;

public class BoomerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float hopForce = 10f;
    public float lateralSpeed = 8f;
    public float maxHopHeight = 3f;
    
    [Header("Lane System")]
    public int currentLane = 1; // 0=left, 1=center, 2=right
    public float laneWidth = 4f;
    public float laneChangeSpeed = 5f;
    
    private Rigidbody rb;
    private bool isGrounded = true;
    private bool canHop = true;
    private Vector3 targetPosition;
    
    [Header("Animation")]
    public Animator animator;
    
    [Header("Audio")]
    public AudioSource hopSound;
    public AudioSource landSound;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetPosition = transform.position;
        
        // Mobile optimization
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    
    void Update()
    {
        HandleInput();
        HandleLaneMovement();
        CheckGrounded();
    }
    
    void HandleInput()
    {
        // Touch input for mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Vector2 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                
                if (touchPos.x < Screen.width * 0.33f && currentLane > 0)
                {
                    ChangeLane(-1); // Move left
                }
                else if (touchPos.x > Screen.width * 0.66f && currentLane < 2)
                {
                    ChangeLane(1); // Move right
                }
                else if (isGrounded && canHop)
                {
                    Hop(); // Center tap = hop
                }
            }
        }
        
        // Keyboard input for testing
        if (Input.GetKeyDown(KeyCode.A) && currentLane > 0)
            ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.D) && currentLane < 2)
            ChangeLane(1);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && canHop)
            Hop();
    }
    
    void ChangeLane(int direction)
    {
        currentLane += direction;
        currentLane = Mathf.Clamp(currentLane, 0, 2);
        
        float targetX = (currentLane - 1) * laneWidth;
        targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);
    }
    
    void HandleLaneMovement()
    {
        Vector3 currentPos = transform.position;
        currentPos.x = Mathf.Lerp(currentPos.x, targetPosition.x, laneChangeSpeed * Time.deltaTime);
        transform.position = currentPos;
    }
    
    void Hop()
    {
        if (!isGrounded || !canHop) return;
        
        rb.AddForce(Vector3.up * hopForce, ForceMode.Impulse);
        isGrounded = false;
        canHop = false;
        
        if (hopSound) hopSound.Play();
        if (animator) animator.SetTrigger("Hop");
        
        Invoke(nameof(ResetHop), 0.5f);
    }
    
    void ResetHop()
    {
        canHop = true;
    }
    
    void CheckGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.1f))
        {
            if (!isGrounded && hit.collider.CompareTag("Ground"))
            {
                isGrounded = true;
                if (landSound) landSound.Play();
                if (animator) animator.SetTrigger("Land");
            }
        }
        else
        {
            isGrounded = false;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            // Handle collectible pickup
            GameManager.Instance.AddScore(10);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            // Handle obstacle collision
            GameManager.Instance.GameOver();
        }
        else if (other.CompareTag("Educational"))
        {
            // Trigger educational content
            other.GetComponent<EducationalCheckpoint>()?.TriggerCheckpoint();
        }
    }
}
