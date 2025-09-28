using UnityEngine;
using System.Collections;

public class MapController : MonoBehaviour
{
    public GameObject mapUI;           // Assign your map RawImage or Canvas panel
    public float fadeDuration = 0.5f;  // Fade speed

    private bool mapOpen = false;
    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    void Start()
    {
        if (mapUI != null)
        {
            // Make sure the GameObject has a CanvasGroup for fading
            canvasGroup = mapUI.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = mapUI.AddComponent<CanvasGroup>();

            // Start hidden
            mapUI.SetActive(false);
            canvasGroup.alpha = 0f;
        }

        // Lock cursor initially
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }
    }

  public void ToggleMap()
{
    mapOpen = !mapOpen;

    // Lock/unlock cursor immediately (no delay, prevents snapping)
    Cursor.lockState = mapOpen ? CursorLockMode.None : CursorLockMode.Locked;
    Cursor.visible = mapOpen;

    if (fadeRoutine != null)
        StopCoroutine(fadeRoutine);

    fadeRoutine = StartCoroutine(FadeMap(mapOpen));
}

private IEnumerator FadeMap(bool opening)
{
    if (opening)
        mapUI.SetActive(true);

    float startAlpha = canvasGroup.alpha;
    float endAlpha = opening ? 1f : 0f;
    float elapsed = 0f;

    while (elapsed < fadeDuration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / fadeDuration;
        canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
        yield return null;
    }

    canvasGroup.alpha = endAlpha;

    if (!opening)
        mapUI.SetActive(false);
}


    public bool IsMapOpen()
    {
        return mapOpen;
    }
}
