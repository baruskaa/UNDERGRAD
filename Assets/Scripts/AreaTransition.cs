using UnityEngine;
using System.Threading.Tasks;

public class AreaTransition : MonoBehaviour
{
    [Header("NEW CAMERA BOUNDS")]
    public Vector2 newMinBounds;
    public Vector2 newMaxBounds;

    [Header("MOVE TO NEW POS")]
    public bool movePlayer = false;
    public Transform transitionPoint;

    [Header("FADE IN/OUT")]
    public bool useFade = false;

    [Header("REQUIRE KEY")]
    public bool requireKeyPress = false;
    public KeyCode activationKey = KeyCode.Q;

    private CameraFollow cam;
    private bool isTransitioning = false;
    private bool playerInZone = false;
    private GameObject playerRef;

    void Start()
    {
        cam = FindAnyObjectByType<CameraFollow>();
    }

    void Update()
    {
        if (requireKeyPress && playerInZone && !isTransitioning && Input.GetKeyDown(activationKey))
        {
            _ = DoTransition(playerRef);
        }
    }

    private async void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInZone = true;
        playerRef = other.gameObject;

        if (!requireKeyPress && !isTransitioning)
        {
            await DoTransition(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInZone = false;
        playerRef = null;
    }

    private async Task DoTransition(GameObject player)
    {
        isTransitioning = true;

        if (useFade)
        {
            await ScreenFader.Instance.FadeOut();
        }

        cam.SetBounds(newMinBounds, newMaxBounds);

        if (movePlayer && transitionPoint != null)
        {
            player.transform.position = transitionPoint.position;
        }

        if (useFade)
        {
            await ScreenFader.Instance.FadeIn();
        }

        isTransitioning = false;
    }
}