using UnityEngine;

public class MapController : MonoBehaviour
{
    public GameObject mapUI;      // Assign your map RawImage or Canvas panel
    private bool mapOpen = false;

    void Start()
    {
        if (mapUI != null)
            mapUI.SetActive(false);

        // Lock cursor initially
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            mapOpen = !mapOpen;

            if (mapUI != null)
                mapUI.SetActive(mapOpen);

            // Toggle cursor
            Cursor.lockState = mapOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = mapOpen;
        }
    }

    // Optional helper to let other scripts know if they should ignore mouse input
    public bool IsMapOpen()
    {
        return mapOpen;
    }
}
