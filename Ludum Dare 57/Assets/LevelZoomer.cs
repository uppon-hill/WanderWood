
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class LevelZoomer : MonoBehaviour {
    public AudioSource audioSource;
    public AudioClip shiftUp;
    public AudioClip shiftDown;
    public AudioClip bounce;

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
            int next = current;
            bool navigable = false;
            bool keyPressed = false;
            bool hasMoreLayers = false;
            bool clear = false;
            if (Input.GetKeyDown(KeyCode.UpArrow)) {

                hasMoreLayers = current < levels.Count - 1;
                navigable = hasMoreLayers && levels[current + 1].Navigable();
                next = current + 1;
                keyPressed = true;
                clear = levels[current].ClearOfFog();

            } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
                hasMoreLayers = current > 0;
                navigable = hasMoreLayers && levels[current - 1].Navigable();
                next = current - 1;
                keyPressed = true;
                clear = true;//levels[current - 1].ClearOfFog();

            }


            if (keyPressed && hasMoreLayers) {
                if (clear) {
                    anim.duration = 0.6f;

                    //pitch should go up one whole musical note tone for each level
                    float hz = 440f * Mathf.Pow(2, (next) / 12f);
                    audioSource.pitch = hz / 440f;
                    if (next > current) {
                        audioSource.PlayOneShot(shiftUp);
                    } else {
                        audioSource.PlayOneShot(shiftDown);
                    }

                    SetLevel(next);
                } else {
                    prev = current;
                    anim.duration = 0.3f;
                    SetPartialLevel(current + (next - current) * 0.1f);
                }

                if (!navigable || !clear) {
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

    public void SetPartialLevel(float nextLevel, SimpleAnimation.Curve curve = SimpleAnimation.Curve.EaseInOut) {
        anim.SetFunction(curve);
        anim.Play(anim.value, nextLevel, false);
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

        float indexDelta = Mathf.Clamp01(index - anim.value);
        //Debug.Log("setting shadowAlpha to ")
        level.SetFogShadowAlpha(1 - indexDelta);
        level.fogAlpha = (1 - indexDelta * 0.5f);

    }

    void FinishAnimation(SimpleAnimation a) {

        if (!levels[current].ClearOfFog() && prev > current) {
            GameManager.i.shouldBounce = true;
        }

        if (GameManager.i.shouldBounce) {
            audioSource.pitch = 1 + Random.Range(-0.1f, 0.1f);
            audioSource.PlayOneShot(bounce);
            GameManager.i.cameraShaker.Shake(0.05f);
            GameManager.i.zoomer.SetLevel(prev, SimpleAnimation.Curve.Decelerate);
            GameManager.i.shouldBounce = false;
            GameManager.i.character.Hurt();

        }
    }
}
