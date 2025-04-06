using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Shaker : MonoBehaviour {

    public float shakeAmount = 0.03f;
    public SimpleAnimation anim;
    // Start is called before the first frame update
    void Start() {
        anim = new SimpleAnimation(0, 1, 0.1f, SimpleAnimation.Curve.Shake, false, true);
    }

    // Update is called once per frame
    void Update() {
        if (anim.animating) {
            anim.Update();
            transform.localPosition = Helpers.RotateVector(Vector2.up * anim.value, Random.value * 360);
        } else {
            transform.localPosition = Vector3.zero;
        }
    }

    public void Shake(float customAmount = 0) {
        anim.Play(0, customAmount == 0 ? shakeAmount : customAmount, true);
    }
}
