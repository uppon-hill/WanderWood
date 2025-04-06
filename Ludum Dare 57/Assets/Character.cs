using System;
using System.Collections;
using System.Collections.Generic;
using Retro;
using UnityEngine;
using UnityEngine.Animations;

public class Character : StateMachine {
    public new Rigidbody2D body;
    public new RetroAnimator animator;
    public BoxCollider2D bodyCollider;
    public Transform orbSlot;
    public AudioSource audioSource;
    public float velX {
        get { return body.velocity.x; }
        set { body.velocity = new(value, body.velocity.y); }
    }
    public float velY {
        get { return body.velocity.y; }
        set { body.velocity = new(body.velocity.x, value); }
    }


    public GroundControl groundControl;
    public AirControl airControl;
    public HurtState hurt;
    void Awake() {

        StateMachine[] allMachines = GetComponentsInChildren<StateMachine>();
        foreach (StateMachine s in allMachines) {
            s.SetCore(this);
        }
    }

    // Start is called before the first frame update
    void Start() {
        //  GameManager.i.lights.Add(transform);
        Set(groundControl);
        GameManager.i.character = this;
    }

    // Update is called once per frame
    void Update() {
        if (state.complete) {
            if ((state == airControl || state == hurt) && IsGrounded()) {
                groundControl.Step();
            }


            if (IsGrounded()) {
                Set(groundControl);
            } else {
                Set(airControl);
            }
        }
        DoBranch();
        CheckNavigateLayerInput();
        SetSortingOrder();
    }


    void FixedUpdate() {
        if (!state.complete) {
            DoFixedBranch();
        }
    }

    void CheckNavigateLayerInput() {
        if (!GameManager.i.zoomer.anim.animating) {

            if (Input.GetKeyDown(KeyCode.Z)) {
                if (GameManager.i.selectedOrb == null) {
                    GameManager.i.Select();
                } else {
                    GameManager.i.Deselect();
                }
            }
        }
    }

    void SetSortingOrder() {
        if (GameManager.i.zoomer.anim.t > 0.5f && !GameManager.i.shouldBounce) {
            int sortingOrder = GameManager.i.zoomer.currentLevel.sortingOrder - 1;
            animator.spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    public void FaceDirection(Vector2 direction) {
        transform.localScale = new Vector3(Mathf.Sign(direction.x), 1);
    }



    public bool IsGrounded() {
        //use physics2d overlap all to check if the character is touching the ground
        //use a box cast to check if the character is touching the ground
        Vector2 size = new Vector2(bodyCollider.size.x - 0.04f, 0.08f);
        Collider2D[] cols = Physics2D.OverlapBoxAll(transform.position, size, 0, LayerMask.GetMask("Ground"));

        return cols.Length > 0;
    }

    public void Hurt() {
        Set(hurt, true);
    }


}


