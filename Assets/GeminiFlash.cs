using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GeminiImageSimilarity : MonoBehaviour
{
    [Header("API Settings")]
    public string apiKey = "AIzaSyDcu7mh40TwuLHIzI7AprUhkDPRBk40"; // Paste your Gemini API key

    [Header("RawImages")]
    public RawImage userMap; // User-drawn map
    public RawImage solutionMap; // Solution maze

    [Header("UI (Optional)")]
    public UnityEngine.UI.Text scoreText; // Display score here

    public float similarityScore { get; private set; } // Score for the run (0-1)

    // Call this to compute similarity (e.g., on button press)
    public void CalculateSimilarity()
    {
        StartCoroutine(SendToGemini());
    }

    private IEnumerator SendToGemini()
    {
        // Validate
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API Key is missing!");
            yield break;
        }

        Texture2D userTex = userMap.texture as Texture2D;
        Texture2D solutionTex = solutionMap.texture as Texture2D;
        if (userTex == null || solutionTex == null)
        {
            Debug.LogError("One or both textures are missing!");
            yield break;
        }

        // Encode textures to base64 JPEG (API format)
        byte[] userBytes = userTex.EncodeToJPG(90); // 90% quality
        byte[] solutionBytes = solutionTex.EncodeToJPG(90);
        string userBase64 = Convert.ToBase64String(userBytes);
        string solutionBase64 = Convert.ToBase64String(solutionBytes);

        // Prompt for similarity (customize as needed)
        string prompt = "Compare these two maze images. The first is a user's drawing, the second is the correct solution. " +
                       "Return ONLY a similarity score from 0 (completely different) to 1 (identical), as a number like '0.85'. " +
                       "Consider overall structure, paths, and walls, ignoring minor drawing imperfections.";

        // JSON payload for Gemini API
        string jsonPayload = $@"{{
            ""contents"": [{{
                ""parts"": [
                    {{""text"": ""{prompt}""}},
                    {{""inline_data"": {{""mime_type"": ""image/jpeg"", ""data"": ""data:image/jpeg;base64,{userBase64}""}}}},
                    {{""inline_data"": {{""mime_type"": ""image/jpeg"", ""data"": ""data:image/jpeg;base64,{solutionBase64}""}}}}
                ]
            }}]
        }}";

        // API request
        using (UnityWebRequest request = new UnityWebRequest("https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key=" + apiKey, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API Error: {request.error} - {request.downloadHandler.text}");
                yield break;
            }

            // Parse response (simple regex for score; customize if needed)
            string response = request.downloadHandler.text;
            Debug.Log($"Gemini Response: {response}"); // Full response for debugging

            // Extract score (assumes response contains "0.XX" format; adjust prompt if needed)
            System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(response, @"(\d+\.?\d*)");
            if (match.Success)
            {
                similarityScore = float.Parse(match.Groups[1].Value);
                similarityScore = Mathf.Clamp01(similarityScore); // Ensure 0-1
                Debug.Log($"Similarity Score: {similarityScore:F3}");

                // Optional: Update UI
                if (scoreText != null)
                {
                    scoreText.text = $"Score: {(similarityScore * 100f):F1}%";
                }
            }
            else
            {
                Debug.LogError("Could not parse score from response. Check prompt/response format.");
                similarityScore = 0f;
            }
        }
    }

    // Example: Trigger on Enter
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            CalculateSimilarity();
        }
    }
}