/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public QuestObject[] quests;
    public bool[] questCompleted;
    public QuestBoxManager theDM;

    void Start()
    {
        questCompleted = new bool[quests.Length];
    }

    void Update()
    {

    }

    public void ShowQuestText(string questText)
    {
        theDM.dialogueLines.Clear();
        theDM.dialogueLines.Add(questText);
        theDM.currentLine = 0;
        theDM.ShowDialogue();
    }
}*/
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


    // Start is called before the first frame update
    void Start()
    {
        questCompleted = new bool[quests.Length];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ShowQuestText(string questText)
    {
        theDM.dialogueLines = new string[1];
        theDM.dialogueLines[0] = questText;

        theDM.currentLine = 0;
        theDM.ShowDialogue();

        
    }
}
