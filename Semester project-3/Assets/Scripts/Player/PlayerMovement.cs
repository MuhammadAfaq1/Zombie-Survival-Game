
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Player Health and Damage")]
    private int maxHealth = 100;
    public int currentHealth;
    public Slider healthSlider;

    public DeathScreen deathScreen;

    [Header("Player Movement and Gravity")]
    public float speed = 2f;
    public float jumpForce = 1.5f;
    public float gravity = -9.81f;

    private CharacterController controller;

    // Kept for compatibility with your existing Inspector setup.
    // Ground detection now uses CharacterController.isGrounded.
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    private bool isGrounded;
    private bool canJump = true;
    private Vector3 velocity;

    [Header("Footstep Audio")]
    public AudioSource leftFootstepAudioSource;
    public AudioSource rightFootstepAudioSource;
    public AudioClip[] footstepSounds;
    public float footstepInterval = 0.5f;

    private bool isLeftFootstep = true;
    private float nextFootstepTime;

    private bool isDead;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("CharacterController is missing from Player!");
            enabled = false;
            return;
        }

        // Initialize player health.
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("Health Slider is not assigned in PlayerMovement!");
        }
    }

    void Update()
    {
        if (isDead || controller == null || !controller.enabled)
            return;

        // Check actual CharacterController ground contact.
        isGrounded = controller.isGrounded;

        // Allow jumping again only after landing.
        if (isGrounded && velocity.y <= 0f)
        {
            velocity.y = -2f;
            canJump = true;
        }

        // Jump only once until the player lands.
        if (Input.GetButtonDown("Jump") && isGrounded && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

            // Prevent another jump while airborne.
            canJump = false;
        }

        // Apply gravity.
        velocity.y += gravity * Time.deltaTime;

        // Get horizontal movement.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement =
            transform.right * horizontalInput +
            transform.forward * verticalInput;

        movement.y = 0f;

        // Prevent faster diagonal movement.
        movement = Vector3.ClampMagnitude(movement, 1f);

        // Combine horizontal and vertical movement.
        Vector3 finalMovement =
            movement * speed + Vector3.up * velocity.y;

        // Move CharacterController only once per frame.
        controller.Move(finalMovement * Time.deltaTime);

        // Play footsteps while moving on the ground.
        if (isGrounded && movement.magnitude > 0.1f)
        {
            if (Time.time >= nextFootstepTime)
            {
                PlayerFootstepSound();
                nextFootstepTime = Time.time + footstepInterval;
            }
        }
    }

    void PlayerFootstepSound()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
            return;

        if (leftFootstepAudioSource == null ||
            rightFootstepAudioSource == null)
            return;

        AudioClip footstepClip =
            footstepSounds[Random.Range(0, footstepSounds.Length)];

        if (isLeftFootstep)
            leftFootstepAudioSource.PlayOneShot(footstepClip);
        else
            rightFootstepAudioSource.PlayOneShot(footstepClip);

        isLeftFootstep = !isLeftFootstep;
    }

    // Called by zombie scripts when they attack the player.
    public void TakeDamage(int damageAmount)
    {
        if (isDead)
            return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Restore player health.
    public void Heal(int healAmount)
    {
        if (isDead)
            return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log("Player healed. Health: " + currentHealth);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Player has died.");

        velocity = Vector3.zero;

        if (controller != null)
            controller.enabled = false;

        if (deathScreen != null)
        {
            deathScreen.ShowDeathScreen();
        }
        else
        {
            Debug.LogError("DeathScreen reference is missing!");
        }
    }
}
