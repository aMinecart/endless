using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    private float dashTimer = 0.0f;
    [SerializeField] private Image dashCooldownImage;
    [SerializeField] private Button dashImage;
    private PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        
        dashCooldownImage.fillAmount = 0.0f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        dashCooldownImage.fillAmount = playerMovement.dashUIFill;
    }
}
