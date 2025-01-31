using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector2 moveDir;

    private float speed = 3f;
    private float acceleration = 1f;

    private Vector2 accelerate(Vector2 currVel, Vector2 accelDir, float accelRate, float maxVel)
    {
        if (accelDir == Vector2.zero)
        {
            return currVel;
        }
        
        float velRatio = (maxVel - currVel.magnitude) / maxVel;
        float angleBetween = Vector2.Angle(currVel, accelDir);

        //accelRate *= Mathf.Clamp01(velRatio + 0.5f) - angleBetween;

        accelRate *= velRatio;

        print(accelRate);

        float accelMag = accelRate * maxVel * Time.fixedDeltaTime; // multiplying the acceleration by the max velocity makes it so that the player accelerates up to max velocity in the same amount of time, not matter how big it is
        Vector2 newVel = currVel + accelMag * accelDir;

        return Vector2.ClampMagnitude(newVel, maxVel);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue moveValue)
    {
        moveDir = moveValue.Get<Vector2>();
        //print(moveDir);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = accelerate(rb.velocity, moveDir, acceleration, speed);

        //print(rb.velocity.magnitude);
    }
}