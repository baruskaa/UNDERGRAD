using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public QuestObject[] quests;
    public bool[] questCompleted;
    public QuestBoxManager theDM;

    public string itemCollected;

    private void Awake()
    {
        if (quests != null && (questCompleted == null || questCompleted.Length != quests.Length))
        {
            questCompleted = new bool[quests.Length];
        }

        // Fallback reference check if not assigned in the Inspector
        if (theDM == null)
        {
            theDM = FindFirstObjectByType<QuestBoxManager>();
        }
    }

    public void ApplyLoadedQuests(bool[] loadedCompleted, string loadedItemCollected)
    {
        itemCollected = loadedItemCollected;

        if (quests == null) return;

        // Ensure array matches current scene's quest count
        questCompleted = new bool[quests.Length];
        if (loadedCompleted != null)
        {
            int count = Mathf.Min(quests.Length, loadedCompleted.Length);
            for (int i = 0; i < count; i++)
            {
                questCompleted[i] = loadedCompleted[i];
            }
        }

        // Sync each QuestObject's active state and re-show active quest objective text
        for (int i = 0; i < quests.Length; i++)
        {
            if (quests[i] == null) continue;

            if (questCompleted[i])
            {
                quests[i].gameObject.SetActive(false);
            }
            else if (quests[i].gameObject.activeSelf)
            {
                ShowQuestText(quests[i].startText);
            }
        }
    }

    public void ShowQuestText(string questText)
    {
        if (theDM != null)
        {
            theDM.ShowQuest(questText);
        }
        else
        {
            Debug.LogWarning("[QuestManager] QuestBoxManager (theDM) reference is missing!");
        }
    }
}