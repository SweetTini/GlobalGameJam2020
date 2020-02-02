﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float jumpSpeed = 35f;
    [SerializeField] private float runFasterFactor = 1.5f;
    [SerializeField] private float groundErrorThreshold = 1.5f;
    [SerializeField] private float wallErrorThreshold = 1.5f;
    [SerializeField] private float wallLineCaseDistance = 0.5f;
    [SerializeField] private float runErrorThreshold = 0.05f;


    private Rigidbody2D rigidBody;
    new private BoxCollider2D collider;
    private Animator animator;
    private bool isMovementEnabled = true;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        MovePlayer();

        //if (IsPlayerOnWall(true))
        //{
        //    if (collider.sharedMaterial == null || collider.sharedMaterial.friction != 0f)
        //    {
        //        collider.sharedMaterial = new PhysicsMaterial2D();
        //        collider.sharedMaterial.friction = 0f;
        //        collider.enabled = false;
        //        collider.enabled = true;
        //    }
        //}
        //else
        //{
        //    if (collider.sharedMaterial == null || collider.sharedMaterial.friction == 0f)
        //    {
        //        collider.sharedMaterial = new PhysicsMaterial2D();
        //        collider.sharedMaterial.friction = 0.5f;
        //        collider.enabled = false;
        //        collider.enabled = true;
        //    }
        //}

    }

    private void MovePlayer()
    {
        if (isMovementEnabled)
        {
            Jump();
            Run();
            FlipSprite();
        }

        HandleAnimations();
    }

    private void Jump()
    {
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && IsPlayerOnGround())
        {
            rigidBody.velocity += new Vector2(0, jumpSpeed);
            rigidBody.velocity = new Vector2(rigidBody.velocity.x,
                                             Mathf.Clamp(rigidBody.velocity.y, 0, jumpSpeed));
        }
    }

    private void Run()
    {
        var actualRunSpeed = runSpeed;

        // Speed Run Increase
        if (Input.GetKey(KeyCode.W))
        {
            actualRunSpeed *= runFasterFactor;
        }
        if (IsPlayerOnWall())
        {
            actualRunSpeed = 0;
        }

        // Run
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rigidBody.velocity = new Vector2(actualRunSpeed, rigidBody.velocity.y);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            rigidBody.velocity = new Vector2(actualRunSpeed * -1, rigidBody.velocity.y);
        }
    }

    private bool IsPlayerOnGround()
    {
        return IsPointOnGround(transform.position + new Vector3(collider.bounds.extents.x * 0.97f, 0, 0)) ||
               IsPointOnGround(transform.position) ||
               IsPointOnGround(transform.position - new Vector3(collider.bounds.extents.x, 0, 0));
    }

    private bool IsPointOnGround(Vector2 position)
    {
        Debug.DrawLine(position, position - new Vector2(0, collider.bounds.extents.y * groundErrorThreshold));

        RaycastHit2D hit = Physics2D.Raycast(position,
                                             -Vector2.up,
                                             collider.bounds.extents.y * groundErrorThreshold,
                                             LayerMask.GetMask(LayerNames.Ground));

        return hit.collider != null && hit.collider.IsTouching(collider);
    }

    private bool IsPlayerOnWall(bool ignoreGround = false)
    {
        return IsPointOnWall(transform.position + new Vector3(0, collider.bounds.extents.y * 0.6f, 0), ignoreGround) ||
               IsPointOnWall(transform.position - new Vector3(0, collider.bounds.extents.y * 0.25f, 0), ignoreGround) ||
               IsPointOnWall(transform.position - new Vector3(0, collider.bounds.extents.y * 0.8f, 0), ignoreGround) ||
               IsPointOnWall(transform.position - new Vector3(0, collider.bounds.extents.y * 1.5f, 0), ignoreGround);
    }

    private bool IsPointOnWall(Vector2 position, bool ignoreGround = false)
    {
        Debug.DrawLine(position, position + new Vector2(collider.bounds.extents.x * wallErrorThreshold, 0), Color.green);
        Debug.DrawLine(position, position - new Vector2(collider.bounds.extents.x * wallErrorThreshold, 0), Color.green);

        var rightHit = Physics2D.Linecast(position,
                                          position + new Vector2(collider.bounds.extents.x * wallErrorThreshold, 0),
                                          LayerMask.GetMask(LayerNames.Ground));
        var leftHit = Physics2D.Linecast(position,
                                         position - new Vector2(collider.bounds.extents.x * wallErrorThreshold, 0),
                                         LayerMask.GetMask(LayerNames.Ground));

        return ((rightHit.collider != null && rightHit.collider.IsTouching(collider)) ||
               (leftHit.collider != null && leftHit.collider.IsTouching(collider))) && (!IsPlayerOnGround() || ignoreGround);
    }

    private bool HasEncounteredEnemy()
    {
        return rigidBody.IsTouchingLayers(LayerMask.GetMask(LayerNames.Enemies));
    }

    private void FlipSprite()
    {
        if (Mathf.Abs(rigidBody.velocity.x) > runErrorThreshold && (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow)))
        {
            transform.localScale = new Vector2(Mathf.Sign(rigidBody.velocity.x), 1f);
        }
    }

    private void HandleAnimations()
    {
        animator.SetBool("IsRunning", Mathf.Abs(rigidBody.velocity.x) > runErrorThreshold);
        animator.SetBool("IsGround", IsPlayerOnGround());
        animator.SetFloat("YVelocity", rigidBody.velocity.y);
    }

    public void DisablePlayerMovement()
    {
        isMovementEnabled = false;
    }

    public void EnablePlayerMovement()
    {
        isMovementEnabled = true;
    }
}
