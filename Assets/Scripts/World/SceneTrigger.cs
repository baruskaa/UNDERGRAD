using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private MainMenu levelLoader; // Reference to your MainMenu/LevelLoader script
    [SerializeField] private string playerTag = "Player"; // Tag assigned to your Player GameObject

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
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