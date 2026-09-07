using System.Collections.Generic;
using UnityEngine;

public class AIChase : MonoBehaviour
{
    public GameObject player;
    public float speed;
    public float distanceBetween;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayer;          // Set this to your Wall / Obstacle layer
    public float obstacleCheckDistance = 1.2f; // How far ahead to scan for walls

    [Header("Reach Trigger")]
    public float reachDistance = 0.5f;
    public List<GameObject> objectsToActivate;

    private float distance;
    private bool hasReachedPlayer = false;

    void Update()
    {
        if (player == null) return;

        distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance < distanceBetween)
        {
            Vector2 targetDirection = (player.transform.position - transform.position).normalized;

            // Adjust direction if an obstacle is ahead
            Vector2 moveDirection = GetAvoidanceDirection(targetDirection);

            // Rotate toward movement direction
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(Vector3.forward * angle);

            // Move AI
            transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
        }

        // Trigger action when reaching the player
        if (distance <= reachDistance && !hasReachedPlayer)
        {
            hasReachedPlayer = true;
            ActivateObjects();
        }
    }

    private Vector2 GetAvoidanceDirection(Vector2 targetDirection)
    {
        // Cast ray forward toward destination
        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetDirection, obstacleCheckDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // Cast diagonal rays left and right to find an open path
            Vector2 leftDir = Quaternion.Euler(0, 0, 45) * targetDirection;
            Vector2 rightDir = Quaternion.Euler(0, 0, -45) * targetDirection;

            bool leftBlocked = Physics2D.Raycast(transform.position, leftDir, obstacleCheckDistance, obstacleLayer);
            bool rightBlocked = Physics2D.Raycast(transform.position, rightDir, obstacleCheckDistance, obstacleLayer);

            if (!leftBlocked) return leftDir.normalized;
            if (!rightBlocked) return rightDir.normalized;
        }

        return targetDirection;
    }

    private void ActivateObjects()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null) obj.SetActive(true);
        }
    }
}