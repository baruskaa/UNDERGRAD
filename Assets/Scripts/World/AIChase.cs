using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class AIChase : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject player;
    public float detectionDistance = 7f;
    public LayerMask obstacleLayer;

    [Header("Movement Speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;

    [Header("Hallway Roaming")]
    public BoxCollider2D hallwayArea;
    public float roamRadius = 5f;
    public float roamWaitTime = 3f;

    [Header("Reach Trigger")]
    public float reachDistance = 0.5f;
    public List<GameObject> objectsToActivate;

    [Header("Jumpscare & GameOver")]
    public GameObject jumpscareUI;             // Assign your Jumpscare Image/Canvas object
    public GameOverManager gameOverManager;   // Reference to your GameOverManager script
    public float delayBeforeGameOver = 2f;     // Delay time in seconds

    [Header("Audio Settings")]
    public AudioClip reachPlayerSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private float roamTimer;
    private bool hasReachedPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Hide jumpscare object on start
        if (jumpscareUI != null) jumpscareUI.SetActive(false);

        // Lock agent rotation to 2D XY plane
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        roamTimer = roamWaitTime;
    }

    void Update()
    {
        // Stop updating behavior once player is caught
        if (hasReachedPlayer) return;

        // Freeze AI movement during active dialogue
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            StopAgent();
            return;
        }

        if (player == null)
        {
            StopAgent();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        bool canSeePlayer = HasLineOfSight(distanceToPlayer);

        if (canSeePlayer)
        {
            // --- CHASE STATE ---
            agent.speed = runSpeed;
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);

            // Trigger sequence when reaching the player
            if (distanceToPlayer <= reachDistance && !hasReachedPlayer)
            {
                hasReachedPlayer = true;
                StartCoroutine(TriggerJumpscareSequence());
            }
        }
        else
        {
            // --- ROAM STATE ---
            agent.speed = walkSpeed;
            HandleRoaming();
        }

        // --- ANIMATION UPDATES ---
        UpdateAnimations();
    }

    private IEnumerator TriggerJumpscareSequence()
    {
        // 1. Freeze enemy movement and animation immediately
        StopAgent();

        // 2. Play the jumpscare audio clip
        PlayReachSound();

        // 3. Show jumpscare UI and activate any designated GameObjects
        if (jumpscareUI != null) jumpscareUI.SetActive(true);
        ActivateObjects();

        // 4. Wait for the specified delay (e.g., 2 seconds) with the jumpscare visible
        yield return new WaitForSeconds(delayBeforeGameOver);

        // 5. Fade in the Game Over UI OVER the jumpscare (jumpscare stays active underneath)
        if (gameOverManager != null)
        {
            gameOverManager.TriggerGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverManager reference missing on AIChase script.");
        }
    }

    private bool HasLineOfSight(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionDistance) return false;

        Vector2 direction = (player.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distanceToPlayer, obstacleLayer);

        return hit.collider == null;
    }

    private void HandleRoaming()
    {
        roamTimer += Time.deltaTime;

        if (roamTimer >= roamWaitTime || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            Vector2 randomPoint = GetRandomRoamPosition();
            agent.isStopped = false;
            agent.SetDestination(randomPoint);
            roamTimer = 0f;
        }
    }

    private Vector2 GetRandomRoamPosition()
    {
        if (hallwayArea != null)
        {
            Bounds bounds = hallwayArea.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomPoint = new Vector2(randomX, randomY);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        Vector2 circlePoint = (Vector2)transform.position + (Random.insideUnitCircle * roamRadius);
        if (NavMesh.SamplePosition(circlePoint, out NavMeshHit fallbackHit, roamRadius, NavMesh.AllAreas))
        {
            return fallbackHit.position;
        }

        return transform.position;
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        Vector2 velocity = agent.velocity;
        bool isWalking = velocity.sqrMagnitude > 0.01f;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            Vector2 moveDir = velocity.normalized;
            animator.SetFloat("InputX", moveDir.x);
            animator.SetFloat("InputY", moveDir.y);
            animator.SetFloat("LastInputX", moveDir.x);
            animator.SetFloat("LastInputY", moveDir.y);
        }
    }

    private void StopAgent()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }
    }

    private void ActivateObjects()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null) obj.SetActive(true);
        }
    }

    private void PlayReachSound()
    {
        if (reachPlayerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reachPlayerSound, soundVolume);
        }
    }
}