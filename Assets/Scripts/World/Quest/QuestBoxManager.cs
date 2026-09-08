/*using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestBoxManager : MonoBehaviour
{
    public GameObject dBox;
    public TextMeshProUGUI dText;
    public bool dialogueActive;

    public List<string> dialogueLines = new List<string>();
    public int currentLine;

    void Start()
    {
        dBox.SetActive(false);
        dialogueActive = false;
    }

    void Update()
    {
        if (dialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            currentLine++;
        }

        if (currentLine >= dialogueLines.Count)
        {
            HideBox();
            currentLine = 0;
            return;
        }

        if (dialogueActive && dialogueLines.Count > 0)
        {
            dText.text = dialogueLines[currentLine];
        }
    }

    public void ShowBox(string dialogue)
    {
        dialogueActive = true;
        dBox.SetActive(true);
        dText.text = dialogue;
    }

    public void HideBox()
    {
        dialogueActive = false;
        dBox.SetActive(false);
    }

    public void ShowDialogue()
    {
        dialogueActive = true;
        dBox.SetActive(true);
    }
}*/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestBoxManager : MonoBehaviour
{

    public GameObject dBox;
    public TextMeshProUGUI dText;
    public bool dialogueActive;

    public string[] dialogueLines;
    public int currentLine;
    // public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        dBox.SetActive(false);
        dialogueActive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueActive && Input.GetKeyDown(KeyCode.Space))
        {

            currentLine++;
        }

        if (currentLine >= dialogueLines.Length)
        {
            HideBox();
            currentLine = 0;
        }

        dText.text = dialogueLines[currentLine];
    }

    public void ShowBox(string dialogue)
    {
        dialogueActive = true;
        dBox.SetActive(true);
        dText.text = dialogue;
    }

    public void HideBox()
    {
        dBox.SetActive(false);
        dialogueActive = false;
    }

    public void ShowDialogue()
    {
        dialogueActive = true;
        dBox.SetActive(true);
    }

    public void CloseAfterDelay(float delay)
    {
        StartCoroutine(CloseQuestBoxAfterDelay(delay));
    }

    private IEnumerator CloseQuestBoxAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideBox();
    }
}
