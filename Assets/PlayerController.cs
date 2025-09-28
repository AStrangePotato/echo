using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement & Look")]
    public float speed = 5f;
    public float gravity = -9.81f;
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float pitchLimit = 85f;

    [Header("References")]
    public MapController mapController;

    [Header("Footstep Audio")]
    public AudioClip footstepClip; // assign your long footstep loop
    private AudioSource footstepSource;

    private CharacterController controller;
    private float verticalVelocity = 0f;
    private float pitch = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (footstepClip != null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.clip = footstepClip;
            footstepSource.loop = true;
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 1f; // 3D sound
            footstepSource.volume = 0.5f;
        }
    }

    void Update()
    {
        if (mapController == null || !mapController.IsMapOpen())
        {
            Move();
            Look();
        }
    }

    void Look()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime * 100f;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        move = Vector3.ClampMagnitude(move, 1f) * speed;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        HandleFootsteps(move);
    }

    void HandleFootsteps(Vector3 move)
    {
        Vector3 horizontalMove = new Vector3(move.x, 0f, move.z);
        if (controller.isGrounded && horizontalMove.magnitude > 0.1f)
        {
            if (footstepSource != null && !footstepSource.isPlaying)
                footstepSource.Play();
        }
        else
        {
            if (footstepSource != null && footstepSource.isPlaying)
                footstepSource.Pause();
        }
    }
}
