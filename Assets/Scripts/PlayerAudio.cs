using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAudio : MonoBehaviour
{
    // --- References ---
    public Transform player;
    public AudioSource audioSource;
    public AudioClip ground;

    private PlayerMovement movement;

    // --- Footstep Parameters ---
    public float range = 1.0f;
    public float footstepVolume = 1.0f;
    public float stepInterval = 0.5f;
    private float nextStepTime = 0.0f;

    // --- Sound State Flags ---
    public bool haveSound = false;
    public bool walking;

    // --- Layer Mask ---
    public LayerMask groundLayer;

    void Start()
    {
        // Cache the PlayerMovement component
        movement = GetComponent<PlayerMovement>();

        // Safety check — ensure player movement script exists before proceeding
        if (movement == null) return;
    }

    void Update()
    {
        // Exit early if references are missing to prevent runtime errors
        if (movement == null || audioSource == null) return;

        walking = movement.isWalking;
        bool crouching = movement.isCrouching;

        // If crouching, the player makes no sound (used for stealth gameplay)
        if (crouching)
        {
            haveSound = false;
            // Stop playing audio when crouching
            audioSource.Stop();
            return;
        }

        // If walking and it's time for the next step, play a footstep sound
        if (walking && Time.time >= nextStepTime)
        {
            Footstep();
            nextStepTime = Time.time + stepInterval;
        }
    }


    private void Footstep()
    {
        RaycastHit hit;

        // Cast a ray straight down to find what surface the player is standing on
        if (Physics.Raycast(player.position, Vector3.down, out hit, range, groundLayer))
        {
            // Check for a tagged collider (in this case, "Wood")
            if (hit.collider.CompareTag("Wood"))
            {
                // Play the matching sound and flag that the player is making noise
                PlayFootstepSFX(ground, footstepVolume);
                haveSound = true;
            }
        }
    }

    // Plays a single randomized footstep sound.
    private void PlayFootstepSFX(AudioClip audio, float volume)
    {
        // Ensure audio source and clip are assigned
        if (audioSource == null || audio == null) return;

        // Randomize pitch slightly to avoid repetitive sound patterns
        audioSource.pitch = Random.Range(0.8f, 1.2f);

        // Play the footstep sound once (does not interrupt looping sounds)
        audioSource.PlayOneShot(audio, volume);
    }
}
