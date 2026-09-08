using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIChase : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject player;
    public float detectionDistance = 7f;
    public LayerMask obstacleLayer; // Set this to your Wall / Obstacle layer

    [Header("Movement Speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;

    [Header("Hallway Roaming")]
    public BoxCollider2D hallwayArea; // Assign a BoxCollider2D covering the rectangular hallway
    public float roamRadius = 5f;      // Fallback if no hallwayArea is assigned
    public float roamWaitTime = 3f;

    [Header("Reach Trigger")]
    public float reachDistance = 0.5f;
    public List<GameObject> objectsToActivate;

    private NavMeshAgent agent;
    private Animator animator;
    private float roamTimer;
    private bool hasReachedPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Lock agent rotation to 2D XY plane
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        roamTimer = roamWaitTime; // Force immediate initial location pick
    }

    void Update()
    {
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

            // Trigger action when reaching the player
            if (distanceToPlayer <= reachDistance && !hasReachedPlayer)
            {
                hasReachedPlayer = true;
                ActivateObjects();
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

    private bool HasLineOfSight(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionDistance) return false;

        // Cast ray toward player to check if walls block vision
        Vector2 direction = (player.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distanceToPlayer, obstacleLayer);

        // Line of sight is clear if the ray hits nothing on the obstacle layer
        return hit.collider == null;
    }

    private void HandleRoaming()
    {
        roamTimer += Time.deltaTime;

        // Pick a new location when wait timer expires or destination is reached
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
        // Option 1: Sample point inside rectangular BoxCollider2D bounds
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

        // Option 2: Circular fallback around current location
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
}
