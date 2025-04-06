using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Retro;
public class HurtState : State {

    public Sheet hurt;
    public override void Enter() {
        animator.Play(hurt);
    }

    public override void Do() {
        if (core.IsGrounded()) {
            Complete();
        }
    }

    public override void FixedDo() {

    }

    public override void Exit() {

    }

}
