using UnityEngine;

public class MonsterEchoJumpWithHit : MonoBehaviour
{
    [Header("Jump Settings")]
    public Transform player;
    public Animator animator;
    public string prowlClipName = "prowl";
    public float jumpHeight = 5f;
    public float jumpDuration = 0.6f;
    public float overshootDistance = 2f;

    [Header("Echo Settings")]
    public int baseEchoThreshold = 3;
    public int randomOffsetMax = 1;
    private int currentEchoCount = 0;
    private int nextJumpThreshold;

    [Header("Hit Effect")]
    public Light hitFlashLight;      // assign a red spotlight in inspector
    public float flashDuration = 0.3f;
    public float hitRadius = 1.5f;

    private bool isJumping = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timer = 0f;
    private bool hasHitPlayer = false;

    void Start()
    {
        ResetNextThreshold();

        if (hitFlashLight != null)
            hitFlashLight.enabled = false; // start off
    }

    void Update()
    {
        if (isJumping)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / jumpDuration);
            float easeT = t * (2 - t);

            Vector3 horizontal = Vector3.Lerp(startPos, targetPos, easeT);
            float yOffset = jumpHeight * 4 * t * (1 - t);
            transform.position = new Vector3(horizontal.x, horizontal.y + yOffset, horizontal.z);

            // face player
            if (player != null)
            {
                Vector3 lookDir = (player.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(lookDir);
            }

            // mid-jump hit check
            if (!hasHitPlayer && player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < hitRadius)
                {
                    hasHitPlayer = true;
                    Debug.Log("[Monster] Player hit mid-jump!");
                    if (hitFlashLight != null)
                        StartCoroutine(RedLightFlash());
                }
            }

            if (t >= 1f)
            {
                isJumping = false;
                hasHitPlayer = false;
                Debug.Log("[Monster] Finished jump.");
            }
        }
    }

    public void RegisterEcho()
    {
        currentEchoCount++;
        Debug.Log($"[Monster] Echo registered. Count={currentEchoCount}/{nextJumpThreshold}");

        if (currentEchoCount >= nextJumpThreshold && !isJumping)
        {
            StartJump();
            currentEchoCount = 0;
            ResetNextThreshold();
        }
    }

    private void ResetNextThreshold()
    {
        nextJumpThreshold = baseEchoThreshold + Random.Range(0, randomOffsetMax + 1);
    }

    private void StartJump()
    {
        if (player == null)
        {
            return;
        }

        startPos = transform.position;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        targetPos = player.position + dirToPlayer * overshootDistance;

        if (animator != null)
        {
            animator.Play(prowlClipName, 0, 0f);
        }

        timer = 0f;
        isJumping = true;
        hasHitPlayer = false;
        Debug.Log($"[Monster] Jump started toward {targetPos} (overshoot {overshootDistance} units).");
    }

    private System.Collections.IEnumerator RedLightFlash()
    {
        hitFlashLight.enabled = true;
        float startIntensity = hitFlashLight.intensity;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            hitFlashLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }

        hitFlashLight.enabled = false;
        hitFlashLight.intensity = startIntensity;
    }
}
