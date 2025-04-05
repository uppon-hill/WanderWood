using System;
using System.Collections;
using System.Collections.Generic;
using Retro;
using UnityEngine;

public class Character : StateMachine {
    public new Rigidbody2D body;
    public new RetroAnimator animator;
    public BoxCollider2D bodyCollider;

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
    void Awake() {

        StateMachine[] allMachines = GetComponentsInChildren<StateMachine>();
        foreach (StateMachine s in allMachines) {
            s.SetCore(this);
        }
    }

    // Start is called before the first frame update
    void Start() {
        GameManager.instance.lights.Add(transform);
        Set(groundControl);
    }

    // Update is called once per frame
    void Update() {
        if (state.complete) {
            if (IsGrounded()) {
                Set(groundControl);
            } else {
                Set(airControl);
            }
        }
        DoBranch();
    }

    void FixedUpdate() {
        if (!state.complete) {
            DoFixedBranch();
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
}


