using UnityEngine;
using TMPro;

public class NoteInteract : MonoBehaviour
{
    [Header("UI Pop-up References")]
    [Tooltip("Ang Canvas UI Image overlay na nakagitna sa screen")]
    public GameObject noteImagePopup;

    [Tooltip("Optional: TMP_Text UI sa loob ng note popup para sa dynamic message text")]
    public TMP_Text noteTextUI;

    [Header("Note Content")]
    [TextArea(3, 8)]
    public string noteMessage = "Isulat dito ang mensahe ng papel...";

    [Header("Alert Icon Setup")]
    public bool isUsingAlert = true;
    [Tooltip("Ang child Sprite/Icon object sa itaas ng papel")]
    public GameObject alertObject;

    private bool isPlayerInZone = false;

    void Start()
    {
        // Siguraduhing nakatago ang Alert Icon sa simula ng game
        if (alertObject != null)
        {
            alertObject.SetActive(false);
        }

        // Siguraduhing nakatago ang Note UI Overlay sa simula
        if (noteImagePopup != null)
        {
            noteImagePopup.SetActive(false);
        }
    }

    void Update()
    {
        // Kapag nasa zone at pinindot ang 'Q'
        if (isPlayerInZone && Input.GetKeyDown(KeyCode.Q))
        {
            ToggleNote();
        }
    }

    public void ToggleNote()
    {
        if (noteImagePopup == null)
        {
            Debug.LogWarning("Walang nakakabit na Note Image Popup UI!", this);
            return;
        }

        bool isOpening = !noteImagePopup.activeSelf;

        if (isOpening)
        {
            OpenNote();
        }
        else
        {
            CloseNote();
        }
    }

    public void OpenNote()
    {
        if (noteImagePopup == null) return;

        // I-apply ang text kung may nakakabit na TMP_Text component
        if (noteTextUI != null)
        {
            noteTextUI.text = noteMessage;
        }

        noteImagePopup.SetActive(true);
        Time.timeScale = 0f; // I-pause ang oras habang binabasa

        // Itago ang alert icon habang nakabukas ang popup
        if (isUsingAlert && alertObject != null)
        {
            alertObject.SetActive(false);
        }
    }

    public void CloseNote()
    {
        if (noteImagePopup == null) return;

        noteImagePopup.SetActive(false);
        Time.timeScale = 1f; // Ituloy ang oras ng laro

        // Ipakita ulit ang alert icon kung nasa loob pa rin ng trigger zone ang player
        if (isUsingAlert && alertObject != null && isPlayerInZone)
        {
            alertObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;

            // Ipakita ang Alert Icon kapag pumasok sa zone (basta sarado ang note popup)
            if (isUsingAlert && alertObject != null && !noteImagePopup.activeSelf)
            {
                alertObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;

            // Kapag lumayo ang player, isara ang note at ituloy ang time scale
            if (noteImagePopup != null && noteImagePopup.activeSelf)
            {
                CloseNote();
            }

            // Itago ang Alert Icon kapag umalis sa zone
            if (isUsingAlert && alertObject != null)
            {
                alertObject.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        // Siguraduhing maibabalik ang Time.timeScale sa 1f sakaling ma-disable ang object
        Time.timeScale = 1f;
    }
}