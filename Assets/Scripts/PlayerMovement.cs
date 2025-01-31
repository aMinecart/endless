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
    private float acceleration = 5f;

    private Vector2 accelerate(Vector2 currVel, Vector2 accelDir, float accelRate, float maxVel)
    {
        if (accelDir == Vector2.zero)
        {
            return currVel;
        }

        // multiplying the acceleration by the max velocity makes it so that the player accelerates up to max velocity in the same amount of time, not matter how big it is
        float accelX = accelDir.x * accelRate * maxVel * Time.fixedDeltaTime;

        if (Mathf.Sign(currVel.x) == Mathf.Sign(accelDir.x))
        {
            print("slowing");
            //print(maxVel - Mathf.Abs(currVel.x) / maxVel);
            accelX *= maxVel - Mathf.Abs(currVel.x) / maxVel; // lower acceleration as current x speed gets closer to max speed
        }

        // print(accelX);
        float accelY = accelDir.y * accelRate * maxVel * Time.fixedDeltaTime;

        if (Mathf.Sign(currVel.y) == Mathf.Sign(accelDir.y))
        {
            accelX *= (Mathf.Sqrt(maxVel) - currVel.y) / Mathf.Sqrt(maxVel); // lower acceleration as current y speed gets closer to max speed
        }
        // print(accelY);
        return currVel + new Vector2(accelX, accelY);
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