using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class SpotlightFlicker : MonoBehaviour
{
    [Header("Initial Settings")]
    public float startIntensity = 1f;
    public float startRange = 7f;

    [Header("Final Settings")]
    public float finalIntensity = 0.7f;
    public float finalRange = 3f;

    [Header("Flicker Settings")]
    public float flickerDuration = 1f; // total flicker time
    public int flickerCount = 10;
    public float minIntensity = 0.1f;  // min during flicker

    [Header("Audio")]
    public AudioClip echoSound;          // One-shot echo
    public float echoVolume = 1f;
    public AudioClip buzzSound;          // Looping buzz
    public float buzzVolume = 1f;

    private Light spotLight;
    private AudioSource buzzSource;

    void Awake()
    {
        spotLight = GetComponent<Light>();
        spotLight.intensity = startIntensity;
        spotLight.range = startRange;

        if (buzzSound != null)
        {
            buzzSource = gameObject.AddComponent<AudioSource>();
            buzzSource.clip = buzzSound;
            buzzSource.loop = true;
            buzzSource.playOnAwake = false;
            buzzSource.spatialBlend = 1f; // 3D sound
            buzzSource.volume = buzzVolume;
        }
    }

    public void StartFlickerAfterDelay(float delay)
    {
        StartCoroutine(FlickerRoutine(delay));
    }

    IEnumerator FlickerRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Play one-shot echo
        if (echoSound != null)
            AudioSource.PlayClipAtPoint(echoSound, transform.position, echoVolume);

        // Start buzz
        if (buzzSource != null)
            buzzSource.Play();

        float interval = flickerDuration / flickerCount;

        // Flicker loop
        for (int i = 0; i < flickerCount; i++)
        {
            spotLight.intensity = Random.Range(minIntensity, startIntensity);
            yield return new WaitForSeconds(interval);
        }

        // Smooth transition to final intensity/range
        float elapsed = 0f;
        float transitionDuration = 0.5f;
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

        // Stop buzz after flicker completes
        if (buzzSource != null)
            buzzSource.Stop();
    }
}
