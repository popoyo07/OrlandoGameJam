using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public LayerMask groundLayer;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        if (movement == null) return;
    }

    void Update()
    {
        if (movement == null || audioSource == null) return;

        bool walking = movement.isWalking;
        bool crouching = movement.isCrouching;

        // Do not play sounds if crouching
        if (crouching) return;

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
