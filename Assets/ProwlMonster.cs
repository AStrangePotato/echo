using UnityEngine;

public class MonsterEchoJumpWithHit : MonoBehaviour
{
    public int hits;

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

    [Header("Hide Settings")]
    public float roofHeight = 10f;        // height to hide at
    public float roofDuration = 0.5f;     // duration to reach roof

        [Header("Audio")]
    public AudioClip echoSound;          // Sound to play on each echo
    public float echoVolume = 1f;

    private bool isJumping = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timer = 0f;
    private bool hasHitPlayer = false;

    private enum JumpPhase { None, ToPlayer, ToRoof }
    private JumpPhase jumpPhase = JumpPhase.None;

    private float postJumpPause = 0.5f; // wait time before jumping to roof
    private float pauseTimer = 0f;
    private bool waitingToHide = false;

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
            if (echoSound != null)
            AudioSource.PlayClipAtPoint(echoSound, transform.position, echoVolume);
    
            timer += Time.deltaTime;
            float duration = (jumpPhase == JumpPhase.ToPlayer) ? jumpDuration : roofDuration;
            float t = Mathf.Clamp01(timer / duration);
            float easeT = t * (2 - t);

            // Horizontal movement
            Vector3 horizontal = Vector3.Lerp(startPos, targetPos, easeT);
            float yOffset;

            if (jumpPhase == JumpPhase.ToPlayer)
                yOffset = jumpHeight * 4 * t * (1 - t); // parabolic jump
            else
                yOffset = Mathf.Lerp(startPos.y, targetPos.y, easeT) - startPos.y; // linear up

            transform.position = new Vector3(horizontal.x, horizontal.y + yOffset, horizontal.z);

            // Face player during first jump
            if (jumpPhase == JumpPhase.ToPlayer && player != null)
            {
                Vector3 lookDir = (player.position - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(lookDir);
            }

            // Mid-jump hit check only during first jump
            if (!hasHitPlayer && jumpPhase == JumpPhase.ToPlayer && player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < hitRadius)
                {
                    hasHitPlayer = true;
                    hits++;
                    Debug.Log("[Monster] Player hit mid-jump!");
                    if (hitFlashLight != null)
                        StartCoroutine(RedLightFlash());
                }
            }

            if (t >= 1f)
            {
                if (jumpPhase == JumpPhase.ToPlayer)
                {
                    // Start waiting instead of immediately jumping to roof
                    waitingToHide = true;
                    pauseTimer = 0f;
                    isJumping = false; // Temporarily stop jump updates
                }
                else if (jumpPhase == JumpPhase.ToRoof)
                {
                    isJumping = false;
                    jumpPhase = JumpPhase.None;
                    Debug.Log("[Monster] Hidden in roof.");
                }
            }
        }

        // Face player while on ground during pause
        if (waitingToHide && player != null)
        {
            Vector3 lookDir = (player.position - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(lookDir);

            pauseTimer += Time.deltaTime;
            if (pauseTimer >= postJumpPause)
            {
                waitingToHide = false;
                StartRoofHide();
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
        if (player == null) return;

        startPos = transform.position;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        targetPos = player.position + dirToPlayer * overshootDistance;

        if (animator != null)
            animator.Play(prowlClipName, 0, 0f);

        timer = 0f;
        isJumping = true;
        jumpPhase = JumpPhase.ToPlayer;
        hasHitPlayer = false;

        Debug.Log($"[Monster] Jump started toward {targetPos} (overshoot {overshootDistance} units).");
    }

    private void StartRoofHide()
    {
        startPos = transform.position;
        targetPos = new Vector3(transform.position.x, roofHeight, transform.position.z);
        timer = 0f;
        jumpPhase = JumpPhase.ToRoof;
        isJumping = true;
        Debug.Log("[Monster] Jumping to roof to hide.");
    }

    private System.Collections.IEnumerator RedLightFlash()
    {
        hitFlashLight.enabled = true;
        float startIntensity = hitFlashLight.intensity;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            hitFlashLight.intensity = Mathf.Lerp(startIntensity, 0f, elapsed / flashDuration);
            yield return null;
        }

        hitFlashLight.enabled = false;
        hitFlashLight.intensity = startIntensity;
    }
}