
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimingSkillCheck : MonoBehaviour
{
    public RectTransform barBackground;
    public RectTransform targetZone;
    public RectTransform movingIndicator;

    public TMP_Text resultText;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Boss Animation & Audio")]
    public Animator bossAnimator;
    public AudioSource audioSource;
    public AudioClip paloSound;
    public AudioClip spacePressSound;

    [Header("Background Music")]
    public AudioSource bgmAudioSource;
    public float bgmFadeDuration = 2f;

    [Header("Health Settings")]
    public GameObject[] hearts;
    private int currentHealth;

    [Header("Speed & Rounds")]
    public float moveSpeed = 300f;
    public float speedIncrease = 30f;
    public float maxTimePerRound = 5f;

    private float roundTimer;

    private int currentRound = 1;
    private int successfulRounds = 0;
    private int totalRounds = 10;

    private float indicatorPosition;
    private float targetPosition;

    private bool gameActive = true;
    private bool inputLocked = false;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        currentHealth = hearts.Length;

        StartRound();
    }

    void Update()
    {
        if (!gameActive || inputLocked)
            return;

        MoveIndicator();

        roundTimer += Time.deltaTime;

        if (roundTimer >= maxTimePerRound)
        {
            TimeOut();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (audioSource != null && spacePressSound != null)
                audioSource.PlayOneShot(spacePressSound);

            CheckTiming();
        }
    }

    void MoveIndicator()
    {
        float range = (barBackground.rect.width -
                       movingIndicator.rect.width) / 2f;

        indicatorPosition =
            Mathf.PingPong(Time.time * moveSpeed, range * 2f) - range;

        movingIndicator.anchoredPosition =
            new Vector2(indicatorPosition, 0f);
    }

    void StartRound()
    {
        roundTimer = 0f;

        float range = (barBackground.rect.width -
                       movingIndicator.rect.width) / 2f;

        float targetHalfWidth = targetZone.rect.width / 2f;

        float allowedRange = range - targetHalfWidth;

        targetPosition = Random.Range(-allowedRange, allowedRange);

        targetZone.anchoredPosition =
            new Vector2(targetPosition, 0f);

        resultText.text = "";
    }

    void TimeOut()
    {
        if (inputLocked)
            return;

        resultText.text = "TOO SLOW!";

        TakeDamage();
    }

    void CheckTiming()
    {
        float targetMin =
            targetPosition - targetZone.rect.width / 2f;

        float targetMax =
            targetPosition + targetZone.rect.width / 2f;

        if (indicatorPosition >= targetMin &&
            indicatorPosition <= targetMax)
        {
            successfulRounds++;

            resultText.text = "SUCCESS!";

            StartCoroutine(NextRound());
        }
        else
        {
            resultText.text = "FAILED!";

            TakeDamage();
        }
    }

    void TakeDamage()
    {
        if (inputLocked || !gameActive)
            return;

        inputLocked = true;

        if (bossAnimator != null)
            bossAnimator.SetTrigger("palo");

        if (audioSource != null && paloSound != null)
            audioSource.PlayOneShot(paloSound);

        currentHealth--;

        if (currentHealth >= 0 && currentHealth < hearts.Length)
            hearts[currentHealth].SetActive(false);

        if (currentHealth <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(NextRound());
        }
    }

    IEnumerator NextRound()
    {
        inputLocked = true;

        yield return new WaitForSeconds(0.8f);

        if (!gameActive)
            yield break;

        currentRound++;

        if (currentRound > totalRounds)
        {
            EndGame();
        }
        else
        {
            if ((currentRound - 1) % 2 == 0)
                moveSpeed += speedIncrease;

            StartRound();

            inputLocked = false;
        }
    }

    void GameOver()
    {
        gameActive = false;
        inputLocked = true;

        resultText.text = "GAME OVER!";

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            CanvasGroup canvasGroup =
                gameOverPanel.GetComponent<CanvasGroup>();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            gameOverPanel.transform.SetAsLastSibling();
        }

        StartCoroutine(FadeOutBGM());
    }

    void EndGame()
    {
        gameActive = false;
        inputLocked = true;

        resultText.text = "YOU SURVIVED!";

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            CanvasGroup canvasGroup =
                victoryPanel.GetComponent<CanvasGroup>();

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            victoryPanel.transform.SetAsLastSibling();
        }

        StartCoroutine(FadeOutBGM());
    }

    IEnumerator FadeOutBGM()
    {
        if (bgmAudioSource == null)
            yield break;

        float startingVolume = bgmAudioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < bgmFadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            bgmAudioSource.volume = Mathf.Lerp(
                startingVolume,
                0f,
                elapsedTime / bgmFadeDuration
            );

            yield return null;
        }

        bgmAudioSource.volume = 0f;
        bgmAudioSource.Stop();
    }
}
