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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isItemQuest)
        {
            if(theQM.itemCollected == targetItem)
            {
                theQM.itemCollected = null;
                EndQuest();
            }
        }
    }

    public void StartQuest()
    {
        theQM.ShowQuestText(startText);
    }
    public void EndQuest()
    {
        theQM.ShowQuestText(endText);

        theQM.questCompleted[questNumber] = true;
        theQMBox.CloseAfterDelay(3f);
        // Disable quest object and its trigger so it doesn't repeat
        gameObject.SetActive(false);
    }

}
