using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelZoomer : MonoBehaviour {

    public List<GameObject> levels;
    public int current;
    public SimpleAnimation anim;
    // Start is called before the first frame update
    void Start() {
        anim = new SimpleAnimation(0, 1, 0.6f, SimpleAnimation.Curve.EaseInOut, false, false);
        SetLevel(0);

    }

    // Update is called once per frame
    void Update() {
        HandleInput();
        HandleAnimation();
    }

    public void HandleInput() {
        if (Mathf.Abs(anim.value - anim.targetValue) < 0.1f) {
            if (Input.GetKeyDown(KeyCode.UpArrow)) {
                SetLevel(current + 1);
            } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                SetLevel(current - 1);
            }
        }

    }

    void HandleAnimation() {
        if (anim.animating) {
            anim.Update();
            foreach (GameObject l in levels) {
                SetLevelDepth(l);
            }
        }
    }

    public void SetLevel(int nextLevel) {
        anim.Play(anim.value, nextLevel, true);
        current = nextLevel;
    }

    void SetLevelDepth(GameObject level) {
        int index = levels.IndexOf(level);

        float levelDelta = Mathf.Max(0, (anim.value + 1) - index);
        float inverseDelta = index - anim.value;
        if (index >= current) {

            float shrink = (1 - (inverseDelta * 0.33f));
            level.transform.localScale = Vector3.one * (Mathf.Max(0, shrink));
        } else {
            level.transform.localScale = Vector3.one * levelDelta;
        }

        float visibility = Helpers.Map(levelDelta, 1, 2, 1, 0, true);

        foreach (SpriteRenderer s in level.GetComponentsInChildren<SpriteRenderer>()) {
            s.color = new Color(s.color.r, s.color.g, s.color.b, visibility);
        }

        level.GetComponentInChildren<Collider2D>().enabled = index == current;

    }
}
