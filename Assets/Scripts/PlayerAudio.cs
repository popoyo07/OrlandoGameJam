using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerAudio : MonoBehaviour
{
    public Transform player;
    public AudioSource audioSource;
    public AudioClip ground;

    private PlayerMovement movement;

    public float range = 1.0f;
    public float footstepVolume = 1.0f;
    public float stepInterval = 0.5f;
    private float nextStepTime = 0.0f;
    public bool haveSound = false;
    public bool walking;

    public LayerMask groundLayer;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        if (movement == null) return;
    }

    void Update()
    {
        if (movement == null || audioSource == null) return;

        walking = movement.isWalking;
        bool crouching = movement.isCrouching;

        if (crouching)
        {
            haveSound = false;
            audioSource.Stop(); // Stop playing audio when crouching
            return; // Exit early to prevent footstep sound
        }

        if (walking && Time.time >= nextStepTime)
        {
            Footstep();
            nextStepTime = Time.time + stepInterval;
        }
    }


    private void Footstep()
    {
        RaycastHit hit;
        if (Physics.Raycast(player.position, Vector3.down, out hit, range, groundLayer))
        {
            if (hit.collider.CompareTag("Wood"))
            {
                PlayFootstepSFX(ground, footstepVolume);
                haveSound = true;
            }
        }
    }

    private void PlayFootstepSFX(AudioClip audio, float volume)
    {
        if (audioSource == null || audio == null) return;

        audioSource.pitch = Random.Range(0.8f, 1.2f);
        audioSource.PlayOneShot(audio, volume);
    }
}
