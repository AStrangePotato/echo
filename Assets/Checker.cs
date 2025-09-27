using UnityEngine;
using UnityEngine.UI;

public class ImageSimilarityWithSlidingCheck : MonoBehaviour
{
    public RawImage userMap;
    public RawImage solutionMap;
    public float similarityScore { get; private set; }
    public int maxOffset = 15;

    public void CalculateSimilarity()
    {
        Texture2D userTexture = userMap.texture as Texture2D;
        Texture2D solutionTexture = solutionMap.texture as Texture2D;

        if (userTexture == null || solutionTexture == null)
        {
            Debug.LogError("One or both RawImage textures are missing or not Texture2D");
            similarityScore = 0f;
            return;
        }

        if (userTexture.width != solutionTexture.width || userTexture.height != solutionTexture.height)
        {
            Debug.LogError("Textures must be the same size");
            similarityScore = 0f;
            return;
        }

        Color32[] userPixels = userTexture.GetPixels32();
        Color32[] solutionPixels = solutionTexture.GetPixels32();
        int width = userTexture.width;
        int height = userTexture.height;

        float bestScore = 0f;

        // Slide offsets
        for (int offsetX = -maxOffset; offsetX <= maxOffset; offsetX++)
        {
            for (int offsetY = -maxOffset; offsetY <= maxOffset; offsetY++)
            {
                int matched = 0;
                int total = 0;

                for (int y = 0; y < height; y++)
                {
                    int dstY = y + offsetY;
                    if (dstY < 0 || dstY >= height) continue;

                    for (int x = 0; x < width; x++)
                    {
                        int dstX = x + offsetX;
                        if (dstX < 0 || dstX >= width) continue;

                        int srcIndex = y * width + x;
                        int dstIndex = dstY * width + dstX;

                        // Skip fully transparent pixels in both images
                        if (userPixels[srcIndex].a < 128 && solutionPixels[dstIndex].a < 128)
                            continue;

                        total++;

                        bool userFilled = userPixels[srcIndex].a >= 128;
                        bool solFilled = solutionPixels[dstIndex].a >= 128;

                        if (userFilled == solFilled)
                            matched++;
                    }
                }

                if (total > 0)
                {
                    float score = (float)matched / total;
                    bestScore = Mathf.Max(bestScore, score);
                }
            }
        }

        similarityScore = Mathf.Clamp01(bestScore) * 5;
        Debug.Log($"Similarity Score: {similarityScore:F3}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            CalculateSimilarity();
    }
}
