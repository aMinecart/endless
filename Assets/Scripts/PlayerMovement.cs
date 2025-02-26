using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb; // rigidbody attached to the player

    private Vector2 moveDir; // the direction the player wants to move in (length of 1)

    // private float minSpeed = 1.0f;
    private float maxSpeed = 10.0f; // maximum speed the player can move the x or y direction

    private float acceleration = 5.0f;

    private float friction = 4.0f; // amount the player slows down by each second (doubled when the player's desired move direction is equal to Vector2.zero) 

    private bool dashing = false; // true if the player is currently dashing (friction and player movement inputs disabled)
    private float dashStrength = 1.5f; // amount velocity is multiplied by when performing a dash
    private float dashCooldown = 60.0f; // time after dash is used before dash can be used again
    private float dashLength = 30.0f; // how long friction and player movement input is disabled for during a dash

    private float dashTimer = 0.0f;
    [SerializeField] private Image dashCooldownImage;
    [SerializeField] private Button dashImage;

    private List<float> speeds = new();
    private float avgSpeed = 0;

    private float calcRampup(float f)
    {
        return 2 * f / (f * f + 1);
    }

    private Vector2 accelerate(Vector2 currVel, Vector2 accelDir, float accelRate, float maxVel, float friction)
    {
        if (accelDir == Vector2.zero)
        {
            return currVel;
        }

        // multiplying the acceleration by the max velocity makes it so that the player accelerates up to max velocity in the same amount of time, not matter how big it is
        float accelX = accelDir.x * accelRate * maxVel * Time.fixedDeltaTime;

        // alt calculation
        if (Mathf.Sign(currVel.x) == Mathf.Sign(accelDir.x))
        {
            // print("slowing (x)");
            
            // float speedToMaxSpeedRatio = Mathf.Clamp01((maxVel - Mathf.Abs(currVel.x)) / maxVel);
            accelX *= calcRampup(Mathf.Clamp01((maxVel - Mathf.Abs(currVel.x)) / maxVel)); // lower acceleration as current x speed gets closer to max speed
        }

        /* alt 2
        if (Mathf.Sign(currVel.x) == Mathf.Sign(accelDir.x))
        {
            // print("stopping accel (x)");
            accelX = accelDir.x * friction * Time.deltaTime;
        }
        else if (Mathf.Abs(currVel.x) > maxVel / 2 && Mathf.Sign(currVel.x) == Mathf.Sign(accelDir.x)) // lower acceleration as current x speed gets closer to max speed
        {
            // print("slowing accel (x)");
            accelX /= 2;
        }
         */

        /*
        if (Mathf.Abs(currVel.x) > maxVel / 2 && Mathf.Sign(currVel.x) == Mathf.Sign(accelDir.x))
        {
            // print("slowing (x)");
            accelX *= Mathf.Clamp01((maxVel - Mathf.Abs(currVel.x) + 0.5f) / maxVel); // lower acceleration as current x speed gets closer to max speed
        }
         */

        // print(accelX);

        float accelY = accelDir.y * accelRate * maxVel * Time.fixedDeltaTime;

        if (Mathf.Abs(currVel.y) > maxVel / 2 && Mathf.Sign(currVel.y) == Mathf.Sign(accelDir.y))
        {
            // print("slowing (y)");
            accelY *= Mathf.Clamp01((maxVel - Mathf.Abs(currVel.y) + 0.5f) / maxVel); // lower acceleration as current y speed gets closer to max speed
        }
        // print(accelY);

        return currVel + new Vector2(accelX, accelY);
    }

    private Vector2 applyFriction(Vector2 currVel, Vector2 accelDir, float friction)
    {
        // add scaling based on changes to maximum speed (maxSpeed is currently the max speed in the x or y direction, not both)

        if (currVel.magnitude == 0)
        {
            return currVel;
        }

        float speed = currVel.magnitude;
        // float reduction = speed < stopSpeed ? stopSpeed * friction * Time.fixedDeltaTime : speed * friction * Time.fixedDeltaTime;
        float reduction = friction * Time.fixedDeltaTime;

        if (accelDir == Vector2.zero)
        {
            reduction *= 2;
        }

        // multiply current velocity by new speed and divide by current speed to set magnitude equal to the new value (cannot subtract from the vector's magnitude) 
        currVel *= Mathf.Max(speed - reduction, 0) / speed;

        return currVel;
    }

    private Vector2 dash(Vector2 currVel, Vector2 accelDir, float dashStrength, float avgSpeed)
    {
        float dashSpeed = Mathf.Max(currVel.magnitude, avgSpeed);
        
        print(dashSpeed);

        return accelDir != Vector2.zero ? accelDir * dashSpeed * dashStrength : currVel.normalized * dashSpeed * dashStrength;
    }


    private void handleTimers()
    {
        speeds.Add(rb.velocity.magnitude);
        
        if (speeds.Count > 5)
            speeds.RemoveAt(0);

        if (dashTimer > 0)
        {
            dashTimer--;
            dashCooldownImage.fillAmount = dashTimer / dashCooldown;
        }
        
        if (dashTimer < dashCooldown - dashLength)
        {
            dashing = false;
        }
    }

    private void storeCurrSpeed()
    {
        speeds.Add(rb.velocity.magnitude);

        if (speeds.Count > 5)
        {
            speeds.RemoveAt(0);
        }
    }

    private void findAvgSpeed()
    {
        avgSpeed = 0.0f;

        foreach (float speed in speeds)
        {
            avgSpeed += speed;
        }

        avgSpeed /= speeds.Count;
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
        if (dashTimer > 0.0f)
        {
            return;
        }

        dashing = true;
        dashTimer = dashCooldown;
        dashCooldownImage.fillAmount = 1.0f;
        rb.velocity = dash(rb.velocity, moveDir, dashStrength, avgSpeed);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        handleTimers();
        storeCurrSpeed();
        findAvgSpeed();

        // print(dashCooldownImage.fillAmount);

        if (!dashing)
        {
            rb.velocity = applyFriction(rb.velocity, moveDir, friction);
            rb.velocity = accelerate(rb.velocity, moveDir, acceleration, maxSpeed, friction);
        }

        print(rb.velocity.magnitude);
    }
}