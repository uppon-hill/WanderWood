using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelZoomer : MonoBehaviour {

    public List<LevelContainer> levels;
    public int current;
    public int prev;
    public LevelContainer currentLevel => levels[current];

    public SimpleAnimation anim;
    // Start is called before the first frame update
    void Awake() {
        anim = new SimpleAnimation(0, 1, 0.6f, SimpleAnimation.Curve.EaseInOut, false, true);
        anim.finished.AddListener(FinishAnimation);
    }
    void Start() {
        SetLevel(0);
        foreach (LevelContainer l in levels) {
            l.SetSorting(-levels.IndexOf(l) * 10);
        }
        GameManager.i.zoomer = this;
    }

    // Update is called once per frame
    void Update() {
        HandleInput();
        HandleAnimation();
        Time.timeScale = anim.animating ? 0.05f : 1f;


    }

    public void HandleInput() {
        if (Mathf.Abs(anim.value - anim.targetValue) < 0.1f) {
            if (Input.GetKeyDown(KeyCode.UpArrow)) {

                bool hasMoreLayers = current < levels.Count - 1;
                bool nextLayerNavigable = hasMoreLayers && levels[current + 1].Navigable();

                SetLevel(current + 1);
                if (!nextLayerNavigable) {
                    GameManager.i.shouldBounce = true;
                }

            } else if (Input.GetKeyDown(KeyCode.DownArrow) && current > 0) {
                bool hasLowerLayers = current > 0;
                bool prevLayerNavigable = hasLowerLayers && levels[current - 1].Navigable();

                SetLevel(current - 1);
                if (!prevLayerNavigable) {
                    GameManager.i.shouldBounce = true;
                }

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

    public void SetLevel(int nextLevel, SimpleAnimation.Curve curve = SimpleAnimation.Curve.EaseInOut) {
        anim.SetFunction(curve);
        anim.Play(anim.value, nextLevel, true);
        prev = current;
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

        float shadowDelta = (index - 1) - anim.value;
        float shadowAlpha = Helpers.Map(shadowDelta, 1, 0, 0, 1);
        level.SetShadowAlpha(shadowAlpha);
    }

    void FinishAnimation(SimpleAnimation a) {
        if (GameManager.i.shouldBounce) {
            GameManager.i.cameraShaker.Shake(0.05f);
            GameManager.i.zoomer.SetLevel(prev, SimpleAnimation.Curve.Decelerate);
            GameManager.i.shouldBounce = false;
        }
    }
}
