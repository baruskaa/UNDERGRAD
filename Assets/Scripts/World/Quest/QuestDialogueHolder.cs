/*using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuestDialogueHolder : MonoBehaviour
{
    public string dialogue;
    private QuestBoxManager dMan;

    public List<string> dialogueLines = new List<string>();

    void Start()
    {
        dMan = FindObjectOfType<QuestBoxManager>();
        dMan.HideBox();
    }

    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (!dMan.dialogueActive)
                {
                    dMan.dialogueLines.Clear();
                    dMan.dialogueLines.AddRange(dialogueLines);
                    dMan.currentLine = 0;
                    dMan.ShowDialogue();
                }
            }
        }
    }
}*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestDialogueHolder : MonoBehaviour
{

    public string dialogue;
    // public GameObject questMark;
    private QuestBoxManager dMan;

    public string[] dialogueLines;


    // Start is called before the first frame update
    void Start()
    {
        dMan = FindObjectOfType<QuestBoxManager>();
        dMan.HideBox();
        //questMark.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            //questMark.SetActive(true);

            if (Input.GetKeyUp(KeyCode.Space))
            {
                // dMan.ShowBox(dialogue);
                if (!dMan.dialogueActive)
                {
                    dMan.dialogueLines = dialogueLines;
                    dMan.currentLine = 0;
                    dMan.ShowDialogue();
                }

            }
        }
    }
}
