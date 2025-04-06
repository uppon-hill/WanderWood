using UnityEngine;
public class GroundControl : State {
    public Retro.Sheet idle;
    public Retro.Sheet run;
    public Retro.Sheet skid;
    public float speed = 3.5f;
    public float decay = 0.9f;

    public AudioClip jumpSound;
    public AudioClip stepSound;
    int lastFrame = 0;
    public override void Enter() {
        animator.Play(idle);
    }
    public override void Do() {
        float x = Input.GetAxisRaw("Horizontal");
        if (x != 0) {
            core.velX = Mathf.Sign(x) * speed;
            core.FaceDirection(new Vector2(x, 1));
            animator.Play(run, 12, true, false);
        } else {
            animator.Play(idle);
        }

        if (Input.GetButtonDown("Jump")) {
            core.velY = 3;
            Complete();
            core.audioSource.pitch = Random.Range(0.8f, 1.2f);
            core.audioSource.PlayOneShot(jumpSound);
        }

        if (!core.IsGrounded()) {
            Complete();
        }

        CheckStep();
        lastFrame = animator.frame;
    }
    public override void FixedDo() {
        float x = Input.GetAxisRaw("Horizontal");
        if (x == 0) {
            core.velX *= decay;
        }

    }

    public override void Exit() {
    }

    void CheckStep() {
        if (animator.GetSheet() == run && animator.frame != lastFrame) {
            //2 and 6
            if (animator.frame == 2 || animator.frame == 6) {
                Step();
            }
        }
    }

    public void Step() {
        core.audioSource.pitch = Random.Range(0.8f, 1.2f);
        core.audioSource.PlayOneShot(stepSound);
    }


}