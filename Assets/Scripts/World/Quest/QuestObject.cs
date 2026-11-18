using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    public int questNumber;
    public QuestManager theQM;
    public QuestBoxManager theQMBox;

    public string startText;
    public string endText;

    public bool isItemQuest;
    public string targetItem;

    private SaveableWorldObject saveable;

    private void Awake()
    {
        saveable = GetComponent<SaveableWorldObject>();
    }

    void Update()
    {
        if (isItemQuest && theQM != null)
        {
            if (theQM.itemCollected == targetItem)
            {
                theQM.itemCollected = null;
                EndQuest();
            }
        }
    }

    public void StartQuest()
    {
        if (theQM != null)
        {
            theQM.ShowQuestText(startText);
        }

        if (saveable != null)
        {
            saveable.SetStateChanged(true);
        }
    }

    public void EndQuest()
    {
        if (theQM != null)
        {
            theQM.ShowQuestText(endText);
            if (questNumber >= 0 && questNumber < theQM.questCompleted.Length)
            {
                theQM.questCompleted[questNumber] = true;
            }
        }

        if (theQMBox != null)
        {
            theQMBox.CloseAfterDelay(3f);
        }

        if (saveable != null)
        {
            saveable.SetStateChanged(true);
        }

        // Disable quest object and its trigger so it doesn't repeat
        gameObject.SetActive(false);
    }
}