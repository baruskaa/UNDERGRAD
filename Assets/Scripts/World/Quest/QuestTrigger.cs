using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    private QuestManager QM;
    private SaveableWorldObject saveable;

    public int questNumber;

    public bool startQuest;
    public bool endQuest;

    private void Awake()
    {
        QM = FindAnyObjectByType<QuestManager>();
        saveable = GetComponent<SaveableWorldObject>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Player" || other.CompareTag("Player"))
        {
            TriggerQuest();
        }
    }

    public void TriggerQuest()
    {
        if (QM == null) QM = FindAnyObjectByType<QuestManager>();
        if (QM == null || questNumber < 0 || questNumber >= QM.questCompleted.Length) return;

        if (!QM.questCompleted[questNumber])
        {
            if (startQuest && !QM.quests[questNumber].gameObject.activeSelf)
            {
                QM.quests[questNumber].gameObject.SetActive(true);
                QM.quests[questNumber].StartQuest();

                if (saveable != null) saveable.SetStateChanged(true);
                gameObject.SetActive(false);
            }

            if (endQuest && QM.quests[questNumber].gameObject.activeSelf)
            {
                QM.quests[questNumber].EndQuest();

                // Prevent future triggers
                if (saveable != null) saveable.SetStateChanged(true);
                gameObject.SetActive(false);
            }
        }
        else
        {
            // Disable trigger if quest is already completed
            if (saveable != null) saveable.SetStateChanged(true);
            gameObject.SetActive(false);
        }
    }
}