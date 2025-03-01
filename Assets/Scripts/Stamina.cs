using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    private PlayerMovement player;
    public Image CrouchImage;
    public float CurrentCrouch, MaxCrouch;
    public float CrouchCost;
    public float CrouchRecharge;
    private Coroutine recharge;
    private Coroutine drain;
    private bool crouching;
    public bool isEmpty = false;

    void Start()
    {
        if (MaxCrouch <= 0) MaxCrouch = 100f;
        player = GameObject.Find("Player").GetComponent<PlayerMovement>(); // Ensure the player is correctly assigned
        CurrentCrouch = MaxCrouch;
    }

    void Update()
    {
        crouching = player.isCrouching;

        if (crouching)
        {
            if (recharge != null)
            {
                StopCoroutine(recharge);
                recharge = null;
            }
            CurrentCrouch -= CrouchCost * Time.deltaTime;
            CurrentCrouch = Mathf.Clamp(CurrentCrouch, 0, MaxCrouch);
            CrouchImage.fillAmount = CurrentCrouch / MaxCrouch;
        }
        else if (recharge == null && CurrentCrouch < MaxCrouch)
        {
            recharge = StartCoroutine(Recharging());
        }

        isEmpty = (CurrentCrouch <= 0);
    }



    private IEnumerator Recharging()
    {

        yield return new WaitForSeconds(3f);
        while (CurrentCrouch < MaxCrouch)
        {
            CurrentCrouch += CrouchRecharge * Time.deltaTime;
            CurrentCrouch = Mathf.Clamp(CurrentCrouch, 0, MaxCrouch);
            CrouchImage.fillAmount = CurrentCrouch / MaxCrouch;
            yield return null;
        }

        recharge = null;
    }
}
