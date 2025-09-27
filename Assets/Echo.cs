using UnityEngine;
using System.Collections.Generic;

public class MouseEchoSpawner : MonoBehaviour
{   
    public int echoCount = 0;
    [Header("Echo Settings")]
    public GameObject echoLightPrefab;   // Prefab with Spot Light + Cookie
    public Camera playerCamera;
    public MapController mapController;  // Assign in inspector
    public float pulseDuration = 3f;
    public float travelDistance = 25f;
    public float maxIntensity = 8f;
    public float mouseCooldown = 0.5f;   // minimum interval between echoes

    private float lastEchoTime = -10f;
    private List<GameObject> activeEchoes = new List<GameObject>();

    public MonsterEchoJumpWithHit monster; // Assign in inspector

    void Update()
    {
        // Only trigger echo if map is not open
        if ((mapController == null || !mapController.IsMapOpen()) &&
            Input.GetMouseButtonDown(0) &&
            Time.time >= lastEchoTime + mouseCooldown)
        {
            lastEchoTime = Time.time;
            SpawnEcho();
        }

        // Update active echoes
        for (int i = activeEchoes.Count - 1; i >= 0; i--)
        {
            EchoInstance echo = activeEchoes[i].GetComponent<EchoInstance>();
            if (echo != null)
            {
                echo.UpdateEcho(Time.deltaTime);
                if (echo.IsFinished)
                {
                    Destroy(activeEchoes[i]);
                    activeEchoes.RemoveAt(i);
                }
            }
        }
    }

    void SpawnEcho()
    {
        if (echoLightPrefab == null || playerCamera == null)
            return;

        GameObject echoObj = Instantiate(echoLightPrefab, playerCamera.transform.position, Quaternion.identity);
        echoObj.transform.rotation = Quaternion.LookRotation(playerCamera.transform.forward);

        EchoInstance echo = echoObj.AddComponent<EchoInstance>();
        echo.Init(playerCamera.transform.forward, pulseDuration, travelDistance, maxIntensity);

        activeEchoes.Add(echoObj);
        if (monster != null)
        monster.RegisterEcho();

        echoCount++;
    }
}

public class EchoInstance : MonoBehaviour
{
    private Vector3 direction;
    private float duration;
    private float travelDistance;
    private float maxIntensity;
    private float timer = 0f;

    private Light echoLight;
    public bool IsFinished { get; private set; } = false;
    private Vector3 startPosition;

    public void Init(Vector3 dir, float pulseDuration, float distance, float intensity)
    {
        direction = dir.normalized;
        duration = pulseDuration;
        travelDistance = distance;
        maxIntensity = intensity;

        echoLight = GetComponentInChildren<Light>();
        if (echoLight != null)
        {
            echoLight.type = LightType.Spot;
            echoLight.intensity = maxIntensity;
            echoLight.range = travelDistance;
            echoLight.spotAngle = 60f;
            echoLight.shadows = LightShadows.None;
        }

        startPosition = transform.position;
    }

    public void UpdateEcho(float deltaTime)
    {
        if (IsFinished) return;

        timer += deltaTime;
        float t = timer / duration;

        // Move forward
        transform.position = startPosition + direction * (t * travelDistance);

        // Fade out intensity
        if (echoLight != null)
        {
            echoLight.intensity = Mathf.Lerp(maxIntensity, 0f, t);
        }

        if (timer >= duration)
        {
            IsFinished = true;
        }
    }
}
