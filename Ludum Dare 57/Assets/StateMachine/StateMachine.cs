using UnityEngine;

public class StateMachine : MonoBehaviour {
    protected State state;
    protected Retro.RetroAnimator animator;
    protected Rigidbody2D body;
    protected Character core;
    public void SetCore(Character core_) {
        core = core_;
        animator = core.animator;
        body = core.body;

    }

    public void Set(State newState, bool overRide = false) {
        if (newState != state || overRide) {
            newState.startTime = Time.time;
            newState.complete = false;

            if (state != newState && state != null) {
                state.Exit();
            }
        }
        state = newState;
        state.Enter();
    }

    public void DoBranch() {
        if (state != null) {
            state.Do();
            state.DoBranch();
        }
    }

    public void DoFixedBranch() {
        if (state != null) {
            state.FixedDo();
            state.DoFixedBranch();
        }
    }
}