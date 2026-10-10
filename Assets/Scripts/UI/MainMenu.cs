using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("TRANSITION SETTINGS")]
    public Animator transition;
    public float transitionTime = 1f;

    [Header("LOAD BUTTON SETUP (OPTIONAL)")]
    [Tooltip("Drag your Main Menu 'Load Game' Button here to automatically grey it out if no save file exists.")]
    public Button loadButton;

    private void Start()
    {
        // Greys out the Load button if no savegame.json file is found
        if (loadButton != null)
        {
            bool hasSave = SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile();
            loadButton.interactable = hasSave;
        }
    }

    public void Play()
    {
        LoadNextLevel();
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("app quit");
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void LoadLevelByIndex(int buildIndex)
    {
        StartCoroutine(LoadLevel(buildIndex));
    }

   
    public void LoadSavedGame()
    {
        if (SaveSystem.Instance != null && SaveSystem.Instance.HasSaveFile())
        {
            StartCoroutine(LoadSavedGameRoutine());
        }
        else
        {
            Debug.LogWarning("[MainMenu] Cannot load: No save file found!");
        }
    }

    private IEnumerator LoadSavedGameRoutine()
    {
        // 1. Play your fade out / transition animation
        if (transition != null)
        {
            transition.SetTrigger("Start");
        }

        // 2. Wait for transition duration
        yield return new WaitForSeconds(transitionTime);

        // 3. Load the saved scene and restore player/world state
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGameFromMainMenu();
        }
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        if (transition != null)
        {
            transition.SetTrigger("Start");
        }

        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
    }
}