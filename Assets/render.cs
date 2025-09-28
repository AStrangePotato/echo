using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraToRenderTexture : MonoBehaviour
{
    public RenderTexture playerViewRT; // assign a RenderTexture asset in Inspector

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Copy the camera output into our RT
        if (playerViewRT != null)
        {
            Graphics.Blit(src, playerViewRT);
        }

        // Pass camera output along to screen
        Graphics.Blit(src, dest);
    }

        void Update()
    {
        Camera cam = GetComponent<Camera>();
        cam.aspect = (float)Screen.width / Screen.height;
    }
}
