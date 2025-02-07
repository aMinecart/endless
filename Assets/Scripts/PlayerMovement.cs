using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    private int timer = 0;
    private int dashCooldown = 60;

    [SerializeField] private Image dashCooldownImage;
    [SerializeField] private Button dashImage;

    private Rigidbody2D rb;

    private Vector2 moveDir;

    // private float minSpeed = 1.0f;
    private float maxSpeed = 10.0f;

    private float acceleration = 5.0f;
    private float friction = 4.0f;

    private float dashStrength = 1.5f;

    private Vector2 accelerate(Vector2 currVel, Vector2 accelDir, float accelRate, float maxVel)
    {
        if (accelDir == Vector2.zero)
        {
            return currVel;
        }

        // multiplying the acceleration by the max velocity makes it so that the player accelerates up to max velocity in the same amount of time, not matter how big it is
        float accelX = accelDir.x * accelRate * maxVel * Time.fixedDeltaTime;

        if (Mathf.Abs(currVel.x) > maxVel / 2 && Mathf.Sign(currVel.x) == Mathf.Sign(accelDir.x))
        {
            print("slowing (x)");
            accelX *= Mathf.Clamp01((maxVel - Mathf.Abs(currVel.x) + 0.5f)) / maxVel; // lower acceleration as current x speed gets closer to max speed
        }
        // print(accelX);

        float accelY = accelDir.y * accelRate * maxVel * Time.fixedDeltaTime;

        if (Mathf.Abs(currVel.y) > maxVel / 2 && Mathf.Sign(currVel.y) == Mathf.Sign(accelDir.y))
        {
            print("slowing (y)");
            accelY *= Mathf.Clamp01((maxVel - Mathf.Abs(currVel.y) + 0.5f)) / maxVel; // lower acceleration as current x speed gets closer to max speed
        }
        // print(accelY);

        return currVel + new Vector2(accelX, accelY);
    }

    private Vector2 applyFriction(Vector2 currVel, float friction)
    {
        // test and improve this function

        if (currVel.magnitude == 0)
        {
            return currVel;
        }

        float speed = currVel.magnitude;
        // float reduction = speed < stopSpeed ? stopSpeed * friction * Time.fixedDeltaTime : speed * friction * Time.fixedDeltaTime;
        float reduction = friction * Time.fixedDeltaTime;

        // multiply current velocity by new speed and divide by current speed to set magnitude equal to the new value (cannot subtract from the vector's magnitude) 
        currVel *= Mathf.Max(speed - reduction, 0) / speed;  

        return currVel;
    }

    private Vector2 dash(Vector2 currVel, Vector2 accelDir, float dashStrength)
    {
        return accelDir != Vector2.zero ? accelDir * currVel.magnitude * dashStrength : currVel * dashStrength / 6.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        dashCooldownImage.fillAmount = 0.0f;
    }

    private void OnMove(InputValue moveValue)
    {
        moveDir = moveValue.Get<Vector2>();
        // print(moveDir);
    }

    private void OnDash(InputValue iV)
    {
        if (timer != 0)
        {
            return;
        }
        
        rb.velocity = dash(rb.velocity, moveDir, dashStrength);
        timer = dashCooldown;
        dashCooldownImage.fillAmount = 1.0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timer > 0)
        {
            timer--;
            dashCooldownImage.fillAmount = timer / dashCooldown;
        }
        
        print(dashCooldownImage.fillAmount);

        rb.velocity = applyFriction(rb.velocity, friction);
        rb.velocity = accelerate(rb.velocity, moveDir, acceleration, maxSpeed);

        // print(rb.velocity.magnitude);
    }
}