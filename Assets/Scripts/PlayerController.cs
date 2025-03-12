using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb; // rigidbody attached to the player
    private CapsuleCollider2D capCollider; // hitbox attached to the player
    private CapsuleCollider2D capTrigger; // trigger attached to the player

    private Vector2 moveDir; // the direction the player wants to move in (length of 1)

    // private float minSpeed = 1.0f;
    private float maxSpeed = 10.0f; // maximum speed the player can move the x or y direction

    private float acceleration = 5.0f;

    private float friction = 4.0f; // amount the player slows down by each second (doubled when the player's desired move direction is equal to Vector2.zero) 

    private bool dashing = false; // true if the player is currently dashing (friction and player movement inputs disabled)
    private float dashStrength = 1.5f; // amount velocity is multiplied by when performing a dash
    private int dashLenience = 5; // how many frames before the dash are checked when calculating the dash strength

    private float dashCooldown = 60.0f; // time after dash is used before dash can be used again
    private float dashLength = 20.0f; // how long friction and player movement input is disabled for during a dash
    private float dashTimer = 0.0f; // time remaining until the player can dash again

    [HideInInspector] public float dashUIFill = 0.0f;

    private bool phasing = false;
    private bool phasingInDash = false;

    private float phaseCooldown = 300.0f; // time after phase is used before phase can be used again
    private float phaseLength = 30.0f; // how long the player hitbox is disabled for after phasing
    private float phaseTimer = 0.0f;

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
        if (Mathf.Abs(currVel.x) > maxVel && Mathf.Sign(currVel.x) == Mathf.Sign(accelDir.x))
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

        // original calculation
        /*
        if (Mathf.Abs(currVel.x) > maxVel / 2 && Mathf.Sign(currVel.x) == Mathf.Sign(accelDir.x))
        {
            // print("slowing (x)");
            accelX *= Mathf.Clamp01((maxVel - Mathf.Abs(currVel.x) + 0.5f) / maxVel); // lower acceleration as current x speed gets closer to max speed
        }
         */

        // print(accelX);

        float accelY = accelDir.y * accelRate * maxVel * Time.fixedDeltaTime;

        if (Mathf.Sign(currVel.y) == Mathf.Sign(accelDir.y))
        {
            // print("slowing (y)");
            accelY *= calcRampup(Mathf.Clamp01((maxVel - Mathf.Abs(currVel.y)) / maxVel)); // lower acceleration as current y speed gets closer to max speed
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
            reduction *= 4; // change to be stronger at high speeds?
        }

        // multiply current velocity by new speed and divide by current speed to set magnitude equal to the new value (cannot subtract from the vector's magnitude) 
        currVel *= Mathf.Max(speed - reduction, 0) / speed;

        return currVel;
    }

    private Vector2 dash(Vector2 currVel, Vector2 accelDir, float strength, float avgSpeed)
    {
        float dashSpeed = Mathf.Max(currVel.magnitude, avgSpeed);
        
        // print(dashSpeed);

        return accelDir != Vector2.zero ? accelDir * dashSpeed * strength : currVel.normalized * dashSpeed * strength;
    }

    private void phase(Collider2D col2D)
    {
        col2D.enabled = false;
        GetComponent<SpriteRenderer>().color += new Color(0.3f, 0.3f, 0.42f, 0);
    }

    private void unphase(Collider2D col2D, Collider2D trigger2D)
    {
        if (trigger2D.IsTouchingLayers(3))
        {
            gameObject.SetActive(false);
        }
        else if (trigger2D.IsTouchingLayers(6))
        {
        }

        col2D.enabled = true;
        GetComponent<SpriteRenderer>().color -= new Color(0.3f, 0.3f, 0.42f, 0);
    }


    private void handleTimers()
    {
        if (dashTimer > 0)
        {
            dashTimer--;
            dashUIFill = dashTimer / dashCooldown;
        }
        
        if (dashing && dashTimer < dashCooldown - dashLength)
        {
            dashing = false;

            if (phasingInDash)
            {
                phasingInDash = false;
                unphase(capCollider, capTrigger);
            }
        }


        if (phaseTimer > 0)
        {
            phaseTimer--;
            // phaseUIFill = phaseTimer / phaseCooldown;
        }

        if (phasing && phaseTimer < phaseCooldown - phaseLength)
        {
            phasing = false;
            unphase(capCollider, capTrigger);
        }
    }

    private void storeCurrSpeed()
    {
        speeds.Add(rb.velocity.magnitude);

        if (speeds.Count > dashLenience)
        {
            speeds.RemoveAt(0);
        }
    }

    private float findAvg(List<float> nums)
    {
        float avg = 0.0f;

        foreach (float num in nums)
        {
            avg += num;
        }

        avg /= nums.Count;
        return avg;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capCollider = GetComponents<CapsuleCollider2D>()[0];
        capTrigger = GetComponents<CapsuleCollider2D>()[1];
    }

    private void OnMove(InputValue moveValue)
    {
        moveDir = moveValue.Get<Vector2>();
        // print(moveDir);
    }

    private void OnPhase()
    {
        if (phasingInDash || phaseTimer > 0.0f)
        {
            return;
        }

        phasing = true;
        
        phaseTimer = phaseCooldown;
        phase(capCollider);
    }

    private void OnDash()
    {
        if (dashTimer > 0.0f)
        {
            return;
        }

        dashing = true;
        dashTimer = dashCooldown;
        
        rb.velocity = dash(rb.velocity, moveDir, dashStrength, avgSpeed);

        bool doDashWithPhase = true;
        if (doDashWithPhase)
        {
            phasingInDash = true;

            if (phasing)
            {
                phasing = false;
            }
            else
            {
                phase(capCollider);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        handleTimers();
        storeCurrSpeed();
        avgSpeed = findAvg(speeds);

        // print(dashUIFill);

        if (!dashing)
        {
            rb.velocity = applyFriction(rb.velocity, moveDir, friction);
            rb.velocity = accelerate(rb.velocity, moveDir, acceleration, maxSpeed, friction);
        }

        print(phaseTimer);
        // print(rb.velocity.magnitude);

        // change player rigidbody to kniematic using a cast script to do collisons, change player phase so that player disappers if phase ends inside of an object
    }
}