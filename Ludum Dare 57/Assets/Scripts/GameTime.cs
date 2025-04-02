using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections.Specialized;

public class GameTime : MonoBehaviour {

    public static GameTime gameTimeInstance;
    public enum kind { Time, Unscaled, Fixed, FixedUnscaled, Game };

    public static int fixedFrameCount;
    public static bool fixedFrameTicker;
    public static float time;
    public static float gameDeltaTime;
    static float oldGameTime;

    public static float globalHitStopTime;
    public static bool isInHitStop {
        get { return globalHitStopTime > 0; }
    }

    public static FloatControlHash<MonoBehaviour> timeControl;

    public static float initialFixedDelta;

    private void Awake() {
        initialFixedDelta = Time.fixedDeltaTime;
        time = 0;
        globalHitStopTime = 0;
        timeControl = new FloatControlHash<MonoBehaviour>(1f);
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Time.timeScale != 0) {
            time += Time.unscaledDeltaTime;
            gameDeltaTime = time - oldGameTime;
            oldGameTime = time;
        }

        if (globalHitStopTime > 0) {
            UpdateGlobalHitstop();
        }

        // Time.fixedDeltaTime = Time.timeScale * initialFixedDelta;
        Time.fixedDeltaTime = 1 / 40f;
    }

    private void FixedUpdate() {
        fixedFrameTicker = true;
        fixedFrameCount++;
    }

    private void LateUpdate() {
        fixedFrameTicker = false;
    }

    public static void AddEffector(MonoBehaviour m, float f) {

        timeControl.AddEffector(m, f);
        Time.timeScale = timeControl.GetValue();
    }

    public static void RemoveEffector(MonoBehaviour m) {

        timeControl.RemoveEffector(m);
        Time.timeScale = timeControl.GetValue();
    }

    public static void ClearTimeEffectors() {
        if (timeControl == null) {
            return;
        }
        timeControl.Clear();
        timeControl.GetValue();
    }

    public static void SetGlobalHitstop(float duration) {
        globalHitStopTime = duration;
        AddEffector(gameTimeInstance, 0f);
    }

    public void UpdateGlobalHitstop() {
        globalHitStopTime -= gameDeltaTime;
        if (globalHitStopTime <= 0) {
            globalHitStopTime = 0;
            RemoveEffector(this);
        }

    }
    public static float GetTimeFromKind(GameTime.kind kind) {
        switch (kind) {
            case GameTime.kind.Fixed:
                return Time.fixedTime;
            case GameTime.kind.FixedUnscaled:
                return Time.fixedUnscaledTime;
            case GameTime.kind.Game:
                return GameTime.time;
            case GameTime.kind.Time:
                return Time.time;
            case GameTime.kind.Unscaled:
                return Time.unscaledTime;
        }
        return GameTime.time;
    }


    //easing animation function
    public static float EaseDecel(float v, float tv, float func) {
        return EaseDecel(v, tv, func, 0.01f);
    }

    public static float EaseDecel(float v, float tv, float func, float tolerance) {
        if (GameTime.fixedFrameTicker) {
            if (Mathf.Abs(tv - v) > tolerance) {
                return v + ((tv - v) * func);
            } else {
                return tv;
            }
        } else {
            return v;
        }
    }

    /// <summary>
    /// Increment a float toward a target value, call over multiple frames.
    /// </summary>
    /// <param name="value">The current value to be incremented.</param>
    /// <param name="targetValue">The intended target value</param>
    /// <param name="step">The distance to cover per function call</param>    
    /// <param name="frequency">The frequency that the function should be called</param>

    public static float Step(float value, float targetValue, float step, int frequency, bool useFixed) { // value, target value, total steps 
        if ((GameTime.fixedFrameTicker && GameTime.fixedFrameCount % frequency == 0) || !useFixed) {
            float distance = (targetValue - value > 0) ? targetValue - value : -(targetValue - value); //distance from value to target value.

            if (distance > step) {
                //return v + ((tv - v) - ((tv - v) / Mathf.Abs(tv - v) * s)); //haha wow. divide a number by the absolute value of itself to "normalise" but perserve the sign.
                return value + (((targetValue - value) / distance) * step); //add the difference between current and target, times the step value.
            } else {
                return targetValue;
            }

        } else {
            return value;
        }
    }

    public static float Step(float value, float targetValue, float step, int frequency) { // value, target value, total steps 
        return Step(value, targetValue, step, frequency, true);

    }
}

class FloatControlHash : ControlHash<MonoBehaviour, float> {

}