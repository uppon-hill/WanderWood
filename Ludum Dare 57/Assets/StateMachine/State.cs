using UnityEngine;
public class State : StateMachine {
    [HideInInspector]
    public bool complete;
    public float t => Time.time - startTime;
    [HideInInspector]

    public float startTime;

    public virtual void Enter() {

    }

    public virtual void Do() {

    }

    public virtual void FixedDo() {

    }

    public virtual void Exit() {

    }

    public void Complete() {
        complete = true;
    }

}