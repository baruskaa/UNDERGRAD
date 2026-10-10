using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestDialogueHolder : MonoBehaviour
{
    public string dialogue;
    public string[] dialogueLines;

    private QuestBoxManager dMan;

    private void Start()
    {
        dMan = FindFirstObjectByType<QuestBoxManager>();

        // Null check prevents NullReferenceException on frame 1
        if (dMan != null)
        {
            dMan.HideBox();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (dMan != null && !dMan.isQuestActive)
                {
                    // Pass dialogue lines to QuestBoxManager's updated ShowQuest system
                    if (dialogueLines != null && dialogueLines.Length > 0)
                    {
                        dMan.ShowQuest(dialogueLines[0]);
                    }
                    else if (!string.IsNullOrEmpty(dialogue))
                    {
                        dMan.ShowQuest(dialogue);
                    }
                }
            }
        }
    }
}