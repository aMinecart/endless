using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector2 moveDir;

    private Vector2 speed;
    private float speedGoal = 1;
    private float rampup;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue moveValue)
    {
        moveDir = moveValue.Get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Sign(rb.velocity.x) != Mathf.Sign(moveDir.x))
        {
            rampup = Mathf.MoveTowards(rampup, 1.0f, Time.fixedDeltaTime);
        }
        else if (moveDir.x == 0)
        {
            rampup = Mathf.MoveTowards(rampup, 1.0f, Time.fixedDeltaTime);
        }
        else
        {
            rampup = Mathf.MoveTowards(rampup, 30.0f, Time.fixedDeltaTime);
        }

        rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, speedGoal, rampup * Time.fixedDeltaTime), rb.velocity.y);
    }
}