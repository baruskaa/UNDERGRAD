using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    private QuestManager QM;

    public int questNumber;

    public bool startQuest;
    public bool endQuest;

    // Start is called before the first frame update
    void Start()
    {
        QM = FindAnyObjectByType<QuestManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            if (!QM.questCompleted[questNumber])
            {
                if (startQuest && !QM.quests[questNumber].gameObject.activeSelf)
                {
                    QM.quests[questNumber].gameObject.SetActive(true);
                    QM.quests[questNumber].StartQuest();
                }

                if (endQuest && QM.quests[questNumber].gameObject.activeSelf)
                {
                    QM.quests[questNumber].EndQuest();

                    //Prevent future triggers
                    gameObject.SetActive(false);
                }
            }
            else
            {
                // Optional: also disable trigger if quest is already completed
                gameObject.SetActive(false);
            }
        }

    }

    public void TriggerQuest()
    {
            if (!QM.questCompleted[questNumber])
            {
                if (startQuest && !QM.quests[questNumber].gameObject.activeSelf)
                {
                    QM.quests[questNumber].gameObject.SetActive(true);
                    QM.quests[questNumber].StartQuest();
                }

                if (endQuest && QM.quests[questNumber].gameObject.activeSelf)
                {
                    QM.quests[questNumber].EndQuest();

                    //Prevent future triggers
                    gameObject.SetActive(false);
                }
            }
            else
            {
                // Optional: also disable trigger if quest is already completed
                gameObject.SetActive(false);
            }
        
    }

}
