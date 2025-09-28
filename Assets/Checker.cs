using UnityEngine;
using UnityEngine.UI;

public class ImageSimilarityWithScoreSheet : MonoBehaviour
{
    [Header("Map Images")]
    public RawImage userMap;
    public RawImage solutionMap;

    [Header("Score UI")]
    public GameObject scoreSheetPanel; // Panel to show score
    public Text scoreText; // Single Text for all stats

    [Header("Other Scripts")]
    public MonsterEchoJumpWithHit monsterScript; // for hits
    public MouseEchoSpawner echoSpawner;         // for echoCount

    public MapController mapController; // to check if map is open
    [Header("Similarity Settings")]
    public int maxOffset = 15;
    public float similarityScore { get; private set; }

    public float elapsedTime = 0f;

    void Update()
    {
        // Track elapsed time
        elapsedTime += Time.deltaTime;
        if (mapController.IsMapOpen()) {
            CalculateSimilarity();
        }
    

        UpdateScoreText();

    }

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

                        if (userPixels[srcIndex].a < 128 && solutionPixels[dstIndex].a < 128)
                            continue;

                        total++;
                        if ((userPixels[srcIndex].a >= 128) == (solutionPixels[dstIndex].a >= 128))
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
    }

    void UpdateScoreText()
    {
        if (scoreSheetPanel == null || scoreText == null) return;

        scoreSheetPanel.SetActive(true);

        // Determine rank


        // Read public variables
        int hits = monsterScript != null ? monsterScript.hits : 0;
        int echoes = echoSpawner != null ? echoSpawner.echoCount : 0;

        string rank;
        if (similarityScore >= 0.7f) rank = "S";
        else if (similarityScore >= 0.55f) rank = "A";
        else if (similarityScore >= 0.3f) rank = "B";
        else rank = "C";
        // Combine into single text
        scoreText.text = $"Similarity: {(similarityScore * 100f):F1}%\n" +
                         $"Rank: {rank}\n" +
                         $"Echoes Used: {echoes}\n" +
                         $"Injuries: {hits}\n" +
                         $"Time: {elapsedTime:F1}s";
    }
}
