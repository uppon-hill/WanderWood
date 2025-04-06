using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelZoomer : MonoBehaviour {

    public List<LevelContainer> levels;
    public int current;
    public SimpleAnimation anim;
    // Start is called before the first frame update
    void Start() {
        anim = new SimpleAnimation(0, 1, 0.6f, SimpleAnimation.Curve.EaseInOut, false, true);
        SetLevel(0);
        GameManager.i.zoomer = this;
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
        HandleAnimation();
        Time.timeScale = anim.animating ? 0.1f : 1f;

    }

    public void HandleInput() {
        if (Mathf.Abs(anim.value - anim.targetValue) < 0.1f) {
            if (Input.GetKeyDown(KeyCode.UpArrow)) {

                bool hasMoreLayers = current < levels.Count - 1;
                bool nextLayerNavigable = hasMoreLayers && levels[current + 1].Navigable();

                if (nextLayerNavigable) {
                    SetLevel(current + 1);
                } else {
                    GameManager.i.cameraShaker.Shake();
                }

            } else if (Input.GetKeyDown(KeyCode.DownArrow) && current > 0) {
                SetLevel(current - 1);
            }
        }

    }

    void HandleAnimation() {
        if (anim.animating) {
            anim.Update();
            foreach (LevelContainer l in levels) {
                SetLevelDepth(l);
            }
        }
    }

    public void SetLevel(int nextLevel) {
        anim.Play(anim.value, nextLevel, true);
        current = nextLevel;
    }

    void SetLevelDepth(LevelContainer level) {
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

        level.SetAlpha(visibility);
        level.SetGeometry(index == current && !anim.animating);

    }
}
