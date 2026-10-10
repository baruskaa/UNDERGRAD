using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestBoxManager : MonoBehaviour
{
    public GameObject dBox;
    public TextMeshProUGUI dText;
    public Animator questAnimator;

    [HideInInspector]
    public bool isQuestActive = false;

    private void Start()
    {
        // Keep active so the Animator can play animations
        if (dBox != null) dBox.SetActive(true);
    }

    public void ShowQuest(string questText)
    {
        isQuestActive = true;
        if (dBox != null) dBox.SetActive(true);
        if (dText != null) dText.text = questText;

            questAnimator.Play("SlideIn");
    }

    public void HideBox()
    {
        isQuestActive = false;

            questAnimator.Play("SlideOut");
        
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