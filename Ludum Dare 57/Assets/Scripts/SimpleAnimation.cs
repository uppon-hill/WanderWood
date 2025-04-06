using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class SimpleAnimation {

    public enum Curve { Linear, Accelerate, Decelerate, EaseInOut, EaseOutIn, Shake, Spring, SoftEaseInOut, CustomCurve }

    Curve defaultFunction;
    Curve curveType;

    public AnimationCurve curve;
    public float startTime;

    public GameTime.kind time;
    public bool unscaledTime;
    public bool fixedTime;
    public float startValue;
    public float value;

    public float elapsedTime { get { return GetTime() - startTime; } }
    public float targetValue;
    public int step;
    public float duration;
    public bool animating;
    public bool justFinished;
    public bool interrupted;

    public float t { get { return elapsedTime / duration; } }

    public class AnimationEvent : UnityEvent<SimpleAnimation> { }

    public AnimationEvent started;
    public AnimationEvent finished;

    public SimpleAnimation(float startValue_, float target_, float duration_, Curve k_ = Curve.Linear, bool fixedTime_ = false, bool unscaledTime_ = true, int step_ = 0) {

        startValue = startValue_;
        value = startValue_;
        targetValue = target_;
        duration = duration_;

        curveType = k_;
        defaultFunction = k_;

        fixedTime = fixedTime_;
        unscaledTime = unscaledTime_;

        step = step_;

        FinishSettingParams();

    }

    public SimpleAnimation(float startValue_, float target_, float duration_, AnimationCurve curve_, bool fixedTime_ = true, bool unscaledTime_ = true, int step_ = 0) {

        startValue = startValue_;
        value = startValue_;
        targetValue = target_;
        duration = duration_;

        curveType = Curve.CustomCurve;
        defaultFunction = Curve.CustomCurve;
        curve = curve_;

        fixedTime = fixedTime_;
        unscaledTime = unscaledTime_;

        step = step_;

        FinishSettingParams();

    }


    public SimpleAnimation(float startValue_, float target_, float duration_, GameTime.kind _timeKind = GameTime.kind.Time, Curve k_ = Curve.Linear) {

        startValue = startValue_;
        value = startValue_;
        targetValue = target_;
        duration = duration_;

        curveType = k_;
        defaultFunction = k_;

        SetTimeScale(_timeKind);

        step = 0;

        FinishSettingParams();

    }

    public void SetParams(float startValue_, float target_, float duration_ = 0) {
        duration_ = duration_ == 0 ? duration : duration_;
        SetParams(startValue_, target_, duration_, curveType, fixedTime, unscaledTime);
    }

    public void SetParams(float startValue_, float target_, float duration_, Curve k_, bool fixedTime_ = false, bool unscaledTime_ = true, int step_ = 0) {
        startValue = startValue_;
        value = startValue_;
        targetValue = target_;
        duration = duration_ == 0 ? duration : duration_;

        curveType = k_;
        defaultFunction = k_;

        fixedTime = fixedTime_;
        unscaledTime = unscaledTime_;

        step = step_;

        FinishSettingParams();

    }

    public void SetParams(float startValue_, float target_, float duration_, AnimationCurve curve_, bool fixedTime_ = true, bool unscaledTime_ = true, int step_ = 0) {

        startValue = startValue_;
        value = startValue_;
        targetValue = target_;
        duration = duration_ == 0 ? duration : duration_;

        curveType = Curve.CustomCurve;
        defaultFunction = Curve.CustomCurve;
        curve = curve_;

        fixedTime = fixedTime_;
        unscaledTime = unscaledTime_;

        step = step_;

        FinishSettingParams();
    }

    public void FinishSettingParams() {
        SetTimeScale(fixedTime, unscaledTime);
        startTime = GetTime();
        started = new AnimationEvent();
        finished = new AnimationEvent();
    }

    public void Play() {
        Play(false);
    }

    public void Play(bool overRide) {
        Play(startValue, targetValue, overRide);
    }

    public void Play(float startValue_, float targetValue_, bool overRide) {
        Play(startValue_, targetValue_, duration, overRide);
    }

    public void Play(float startValue_, float targetValue_, float duration_, bool overRide) {
        Play(startValue_, targetValue_, duration_, overRide, defaultFunction);
    }

    public void Play(float startValue_, float targetValue_, float duration_, bool overRide, Curve newFunction) {
        if (overRide || ((!animating) && (targetValue_ != value))) {
            value = startValue_;
            startValue = startValue_;
            targetValue = targetValue_;
            curveType = newFunction;
            duration = duration_;
            startTime = GetTime();
            animating = true;
            justFinished = false;
            started.Invoke(this);
            interrupted = false;
        }
    }

    public void Play(float startValue_, float targetValue_, float duration_, bool overRide, AnimationCurve curve_) {
        if (overRide || ((!animating) && (targetValue_ != value))) {
            value = startValue_;
            startValue = startValue_;
            targetValue = targetValue_;
            curveType = Curve.CustomCurve;
            curve = curve_;
            duration = duration_;
            startTime = GetTime();
            animating = true;
            justFinished = false;
            interrupted = false;
            started.Invoke(this);
        }
    }

    public void Update() {
        if (animating) {
            Update(elapsedTime / duration);
        }
    }

    public void UpdateIfAnimating() {
        if (animating) {
            Update(elapsedTime / duration);
        }
    }

    private void LateUpdate() {
        if (justFinished) {
            justFinished = false;
        }
    }

    public void Update(float t) {    //elapsedTime = normalised timescale 
        value = Evaluate(t);
        if (t >= 1) {
            if (animating) {

                if (curveType == Curve.Shake) {
                    value = startValue;
                } else if (curveType == Curve.CustomCurve) {
                    value = targetValue;
                } else {
                    value = targetValue;
                }

                Finish();
            }
        }
    }
    void Finish() {
        animating = false; //finish animating
        justFinished = true;
        finished.Invoke(this);
    }

    public float Evaluate(float elapsedTime_) {

        elapsedTime_ = Mathf.Clamp(elapsedTime_, 0, 1);
        justFinished = false;
        float t = elapsedTime_; //our animation time position, between 0 and 1
        float ti = elapsedTime_ * duration; //absolute time;
        float result = 0;

        switch (curveType) {
            case Curve.Linear: {
                    result = startValue + ((targetValue - startValue) * t); //linear
                    break;
                }

            case Curve.Accelerate: { //exponential increase
                    result = (targetValue - startValue) * Mathf.Pow(2, 10 * (ti / duration - 1)) + startValue;
                    break;
                }

            case Curve.Decelerate: {//exponential decrease
                    result = (targetValue - startValue) * (-Mathf.Pow(2, -10 * ti / duration) + 1) + startValue;
                    break;
                }

            case Curve.EaseInOut: {//exponential parabola
                    ti /= duration / 2;
                    if (ti < 1) {
                        result = (targetValue - startValue) / 2 * Mathf.Pow(2, 10 * (ti - 1)) + startValue;
                    } else {
                        ti--;
                        result = (targetValue - startValue) / 2 * (-Mathf.Pow(2, -10 * ti) + 2) + startValue;
                    }

                    break;
                }

            case Curve.SoftEaseInOut: {//exponential parabola
                    result = -(targetValue - startValue) / 2 * (Mathf.Cos(Mathf.PI * ti / duration) - 1) + startValue;

                    break;
                }

            case Curve.EaseOutIn: {//exponential parabola
                    float ts = (ti /= duration) * ti;
                    float tc = ts * ti;

                    result = startValue + (targetValue - startValue) * (5.6f * tc * ts + -12.95f * ts * ts + 13.5f * tc + -8.2f * ts + 3.05f * t);

                    break;
                }

            case Curve.Shake: {//Shake
                    result = startValue + (Random.Range(value * 0.5f, targetValue) * (Mathf.Sign(value) * -1f)); //linear
                    break;
                }

            case Curve.Spring: {// spring bounce
                    float a = 0.2f; //amplitude
                    float w = 20; //wavelength
                    float st = t; //spring time
                    result = targetValue - ((startValue - targetValue) * (-Mathf.Exp(-st / a) * Mathf.Cos(st * w)));
                    break;
                }

            case Curve.CustomCurve: {
                    float curveTime = Helpers.Map(t, 0, 1, curve.keys[0].time, curve.keys[curve.keys.Length - 1].time);
                    result = Helpers.Map(curve.Evaluate(curveTime), 0, 1, startValue, targetValue);
                    break;
                }
        }


        if (step != 0) {
            result = Step(result);
        }
        return result;

    }

    public float Step(float val) {
        float increment = (targetValue - startValue) / step;
        val /= increment;
        val = (int)val;
        val *= increment;
        return val;
    }

    public float ResolveVelocity(float normalTime) {
        float delta = GetDelta();


        if ((normalTime * duration) + GetDelta() > duration) {
            delta = duration - (normalTime * duration);
        }

        float current = Evaluate(normalTime);
        float next = Evaluate(normalTime + delta);
        return ((next - current) / delta) / duration;//slope of curve
    }

    public List<float> CalculateSteps(float range, int step) {
        List<float> steps = new List<float>();

        for (int i = 0; i < step; i++) steps.Add(startValue + (i * (range / step)));

        return steps;
    }

    public void SetFunction(AnimationCurve f) {
        curve = f;

    }

    public void SetFunction(SimpleAnimation.Curve f) {
        defaultFunction = f;
        curveType = f;
        curve = null;
    }

    public void SetTimeScale(GameTime.kind type) {
        time = type;
    }

    public void SetTimeScale(bool fixedT, bool unscaledT) {
        if (fixedTime) {
            if (unscaledTime) {
                time = GameTime.kind.FixedUnscaled;
            } else {
                time = GameTime.kind.Fixed;
            }
        } else {
            if (unscaledTime) {
                time = GameTime.kind.Unscaled;
            } else {
                time = GameTime.kind.Game;
            }
        }
    }

    public float GetTime() {
        switch (time) {
            case GameTime.kind.Fixed:
                return Time.fixedTime;

            case GameTime.kind.Unscaled:
                return Time.unscaledTime;

            case GameTime.kind.FixedUnscaled:
                return Time.fixedUnscaledTime;

            case GameTime.kind.Game:
                return GameTime.time;

            case GameTime.kind.Time:
                return Time.time;

        }
        return 0;

    }

    public void SeekTo(float t) {
        startTime = GetTime() - (t * duration);
    }

    public void ForceFinish() {
        startTime = startTime - duration;
        interrupted = true;
        Update();
    }

    public float GetDelta() {
        switch (time) {
            case GameTime.kind.Fixed:
                return Time.fixedDeltaTime;

            case GameTime.kind.Unscaled:
                return Time.unscaledDeltaTime;

            case GameTime.kind.FixedUnscaled:
                return Time.fixedUnscaledDeltaTime;

            case GameTime.kind.Game:
                return GameTime.gameDeltaTime;

        }
        return 0;

    }
}

