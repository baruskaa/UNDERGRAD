using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class LibrarianAI : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject player;
    public LayerMask obstacleLayer;

    [Header("Library Alert State")]
    [Tooltip("If true, librarian ignores stealth/line-of-sight and actively hunts the player.")]
    public bool isAlerted = false;

    [Header("Flashlight Detection Settings")]
    [Tooltip("The GameObject or Light component representing the player's flashlight.")]
    public GameObject playerFlashlight;
    public float flashlightOnDetectionDistance = 20f;
    public float flashlightOffDetectionDistance = 3f;

    [Header("Movement Speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 4.5f;

    [Header("Library Roaming")]
    public BoxCollider2D libraryArea;
    public float roamRadius = 5f;
    public float roamWaitTime = 3f;

    [Header("Reach Trigger")]
    public float reachDistance = 0.5f;
    public List<GameObject> objectsToActivate;

    [Header("Jumpscare Settings")]
    [Tooltip("The Jumpscare Canvas/Panel GameObject (must have JumpscareManager script attached).")]
    public GameObject jumpscareUI;

    [Header("Audio Clips")]
    public AudioClip reachPlayerSound;
    [Range(0f, 1f)] public float soundVolume = 1f;

    [Header("Proximity Audio Settings")]
    [Tooltip("Plays when player is within 25f (Far)")]
    public AudioClip audioFar;
    [Tooltip("Plays when player is within 10f (Medium)")]
    public AudioClip audioMedium;
    [Tooltip("Plays when player is within 5f (Close)")]
    public AudioClip audioClose;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private float roamTimer;
    private bool hasReachedPlayer = false;

    private enum AudioProximityState { None, Far, Medium, Close }
    private AudioProximityState currentAudioState = AudioProximityState.None;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        hasReachedPlayer = false;
        currentAudioState = AudioProximityState.None;
    }

    private void Start()
    {
        if (jumpscareUI != null) jumpscareUI.SetActive(false);

        // Lock agent rotation to 2D XY plane
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        roamTimer = roamWaitTime;
    }

    private void Update()
    {
        if (hasReachedPlayer) return;

        // Freeze AI movement during active dialogue
        if (DialogueManager.Instance != null && DialogueManager.Instance.isDialogueActive)
        {
            StopAgent();
            UpdateProximityAudio(float.MaxValue);
            return;
        }

        if (player == null)
        {
            StopAgent();
            UpdateProximityAudio(float.MaxValue);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        UpdateProximityAudio(distanceToPlayer);

        // Chase if triggered by the computer OR if player enters line of sight
        bool shouldChase = isAlerted || HasLineOfSight(distanceToPlayer);

        if (shouldChase)
        {
            // --- CHASE / ATTACK STATE ---
            agent.speed = runSpeed;
            agent.isStopped = false;
            agent.SetDestination(player.transform.position);

            if (distanceToPlayer <= reachDistance && !hasReachedPlayer)
            {
                TriggerPlayerCaught();
            }
        }
        else
        {
            // --- ROAM STATE ---
            agent.speed = walkSpeed;
            HandleRoaming();
        }

        UpdateAnimations();
    }

    /// <summary>
    /// Called by LibraryComputer after 3 failed Student ID attempts.
    /// </summary>
    public void TriggerSecurityAlertChase()
    {
        isAlerted = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Called by LibraryComputer when the correct Student ID is entered.
    /// </summary>
    public void DisableLibrarian()
    {
        isAlerted = false;
        StopAgent();
        if (audioSource != null) audioSource.Stop();
        gameObject.SetActive(false);
    }

    private void UpdateProximityAudio(float distanceToPlayer)
    {
        AudioProximityState newState = AudioProximityState.None;

        if (distanceToPlayer <= 5f)
            newState = AudioProximityState.Close;
        else if (distanceToPlayer <= 10f)
            newState = AudioProximityState.Medium;
        else if (distanceToPlayer <= 25f)
            newState = AudioProximityState.Far;

        if (newState != currentAudioState)
        {
            currentAudioState = newState;

            switch (currentAudioState)
            {
                case AudioProximityState.Close:
                    PlayProximityClip(audioClose);
                    break;
                case AudioProximityState.Medium:
                    PlayProximityClip(audioMedium);
                    break;
                case AudioProximityState.Far:
                    PlayProximityClip(audioFar);
                    break;
                case AudioProximityState.None:
                    if (audioSource != null) audioSource.Stop();
                    break;
            }
        }
    }

    private void PlayProximityClip(AudioClip clip)
    {
        if (audioSource == null) return;

        if (clip == null)
        {
            audioSource.Stop();
            return;
        }

        audioSource.clip = clip;
        audioSource.volume = soundVolume;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void TriggerPlayerCaught()
    {
        hasReachedPlayer = true;
        StopAgent();
        PlayReachSound();
        ActivateObjects();

        if (jumpscareUI != null)
        {
            jumpscareUI.SetActive(true);
        }
    }

    private bool HasLineOfSight(float distanceToPlayer)
    {
        bool isFlashlightOn = playerFlashlight != null && playerFlashlight.activeSelf;
        float currentDetectionDistance = isFlashlightOn ? flashlightOnDetectionDistance : flashlightOffDetectionDistance;

        if (distanceToPlayer > currentDetectionDistance) return false;

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
        if (libraryArea != null)
        {
            Bounds bounds = libraryArea.bounds;
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
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;

            if (reachPlayerSound != null)
            {
                audioSource.clip = reachPlayerSound;
                audioSource.volume = soundVolume;
                audioSource.Play();
            }
        }
    }
}