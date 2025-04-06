using System.Collections;
using System.Collections.Generic;
using Retro;
using UnityEngine;

public class AirControl : State {
    public Sheet fall;
    public float terminal = 2;
    public float decay = 0.96f;
    public float accel = 0.1f;
    public override void Enter() {
        animator.Play(fall);
        animator.Stop();
    }

    public override void Do() {
        SetFrame();

        if (core.IsGrounded()) {
            Complete();
        }
    }

    public override void FixedDo() {
        float x = Input.GetAxisRaw("Horizontal");
        if (x != 0) {
            core.velX += Mathf.Sign(x) * accel;

            core.FaceDirection(new Vector2(x, 1));

        } else {
            core.velX *= decay;
        }

        core.velX = Mathf.Clamp(core.velX, -1.6f, 1.6f);
    }

    public override void Exit() {

    }


    void SetFrame() {
        int frame = (int)Helpers.Map(core.velY, terminal * 0.7f, -terminal * 0.7f, 0, fall.count - 1);

        animator.frame = frame;
    }
}
