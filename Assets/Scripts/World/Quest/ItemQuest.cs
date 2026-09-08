using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemQuest : MonoBehaviour
{

    public int questNumber;
    private QuestManager theQM;
    public string itemName;


    // Start is called before the first frame update
    void Start()
    {
        theQM = FindAnyObjectByType<QuestManager>();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!theQM.questCompleted[questNumber] && theQM.quests[questNumber].gameObject.activeSelf) 
            {
                theQM.itemCollected = itemName;
                gameObject.SetActive(false); 
            }
        }
    }
}
