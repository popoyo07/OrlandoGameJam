using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    // --- References ---
    private PlayerMovement player;
    public Image CrouchImage;

    // --- Stamina values ---
    public float CurrentCrouch, MaxCrouch;
    public float CrouchCost;
    public float CrouchRecharge;

    // --- Coroutine handles ---
    private Coroutine recharge;

    // --- State flags ---
    private bool crouching;
    public bool isEmpty = false;

    void Start()
    {
        // Set default max crouch if not assigned
        if (MaxCrouch <= 0) MaxCrouch = 100f;

        // Ensure the player is correctly assigned
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();

        // Initialize current stamina to maximum at start
        CurrentCrouch = MaxCrouch;
    }

    void Update()
    {
        // Get current crouching state from player
        crouching = player.isCrouching;

        if (crouching)
        {
            // If currently recharging, stop the recharge coroutine
            if (recharge != null)
            {
                StopCoroutine(recharge);
                recharge = null;
            }

            // Drain stamina over time while crouching
            CurrentCrouch -= CrouchCost * Time.deltaTime;
            CurrentCrouch = Mathf.Clamp(CurrentCrouch, 0, MaxCrouch);

            // Update UI to reflect current stamina
            CrouchImage.fillAmount = CurrentCrouch / MaxCrouch;
        }
        else if (recharge == null && CurrentCrouch < MaxCrouch)
        {
            // If not crouching and stamina is not full, start recharging
            recharge = StartCoroutine(Recharging());
        }

        // Set flag if stamina is empty
        isEmpty = (CurrentCrouch <= 0);
    }


    // Gradually restores crouch stamina over time after a delay.
    private IEnumerator Recharging()
    {
        // Wait a short delay before starting recharge
        yield return new WaitForSeconds(3f);

        // Recharge stamina gradually until full
        while (CurrentCrouch < MaxCrouch)
        {
            CurrentCrouch += CrouchRecharge * Time.deltaTime;
            CurrentCrouch = Mathf.Clamp(CurrentCrouch, 0, MaxCrouch);

            // Update UI image to match current stamina
            CrouchImage.fillAmount = CurrentCrouch / MaxCrouch;
            yield return null;
        }

        // Reset recharge coroutine reference when done
        recharge = null;
    }
}
