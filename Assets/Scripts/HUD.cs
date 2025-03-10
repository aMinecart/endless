using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private Image dashCooldownImage;
    [SerializeField] private Button dashImage;
    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        
        dashCooldownImage.fillAmount = 0.0f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        dashCooldownImage.fillAmount = playerController.dashUIFill;
    }
}