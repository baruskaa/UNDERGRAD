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

    private void Start()
    {
        if (quests != null)
        {
            questCompleted = new bool[quests.Length];
        }

        // Fallback reference check if not assigned in the Inspector
        if (theDM == null)
        {
            theDM = FindFirstObjectByType<QuestBoxManager>();
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