using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderTrigger : MonoBehaviour
{
    [Header("Pangalan ng Boss Fight Scene")]
    public string bossFightSceneName = "BossFight";


    public void LoadBossFightScene()
    {
        SceneManager.LoadScene(bossFightSceneName);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SceneManager.LoadScene(bossFightSceneName);
        }
    }
}