using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private MainMenu levelLoader; // Reference to your MainMenu/LevelLoader script
    [SerializeField] private string playerTag = "Player"; // Tag assigned to your Player GameObject

    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        // For 2D games, change 'Collider other' to 'Collider2D other' and use OnTriggerEnter2D
        if (!isTransitioning && other.CompareTag(playerTag))
        {
            isTransitioning = true;

            if (levelLoader != null)
            {
                levelLoader.LoadNextLevel();
            }
            else
            {
                Debug.LogError("LevelLoader reference is missing on " + gameObject.name);
            }
        }
    }
}