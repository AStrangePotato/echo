using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(AudioSource))]
public class SpotlightFlicker : MonoBehaviour
{
    [Header("Initial Settings")]
    public float startIntensity = 1f;
    public float startRange = 7f;

    [Header("Final Settings")]
    public float finalIntensity = 0.7f;
    public float finalRange = 3f;

    [Header("Flicker Settings")]
    public float delay = 5f;           // Wait before flicker
    public float flickerDuration = 1f; // Total flicker time
    public int flickerCount = 10;      // Number of flickers
    public float minIntensity = 0.1f;  // Minimum during flicker

    [Header("Audio")]
    public AudioClip flickerEndSfx;    // Assign your sound effect here
    [Range(0f,1f)]
    public float sfxVolume = 1f;

    private Light spotLight;
    private AudioSource audioSource;

    void Start()
    {
        spotLight = GetComponent<Light>();
        audioSource = GetComponent<AudioSource>();

        if (spotLight.type != LightType.Spot)
            Debug.LogWarning("This script is designed for Spotlights");

        spotLight.intensity = startIntensity;
        spotLight.range = startRange;

        // Configure AudioSource
        audioSource.playOnAwake = false;
        audioSource.clip = flickerEndSfx;
        audioSource.spatialBlend = 0f; // 2D sound

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        // Wait before flicker
        yield return new WaitForSeconds(delay);

        float interval = flickerDuration / flickerCount;

        // Flicker loop
        for (int i = 0; i < flickerCount; i++)
        {
            spotLight.intensity = Random.Range(minIntensity, startIntensity);
            yield return new WaitForSeconds(interval);
        }

        // Smoothly transition to final intensity and range
        float elapsed = 0f;
        float transitionDuration = 0.5f; // fade duration
        float initialIntensity = spotLight.intensity;
        float initialRange = spotLight.range;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            spotLight.intensity = Mathf.Lerp(initialIntensity, finalIntensity, t);
            spotLight.range = Mathf.Lerp(initialRange, finalRange, t);
            yield return null;
        }

        spotLight.intensity = finalIntensity;
        spotLight.range = finalRange;

        // Play SFX after flicker completes
        if (flickerEndSfx != null)
            audioSource.PlayOneShot(flickerEndSfx, sfxVolume);
    }
}
