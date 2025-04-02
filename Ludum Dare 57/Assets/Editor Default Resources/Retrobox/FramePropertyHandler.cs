using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FramePropertyHandler {

    public static void ApplyCurveProperties(Retro.RetroAnimator animator, bool fixedUdpate) {
        Dictionary<Retro.Layer, Retro.CurveInstance> curvesByLayer = animator.boxManager.curveInstancesByLayer;
        foreach (Retro.Layer l in curvesByLayer.Keys) {
            switch (l.myBoxType) {

                case "VelocityOffset":
                    HandleVelocityOffset(curvesByLayer[l], l, fixedUdpate);
                    break;

                case "Effect Trail":
                    if (!fixedUdpate) {
                        HandleEffectTrail(curvesByLayer[l]);
                    }
                    break;
            }
        }
    }


    static void HandleEffectTrail(Retro.CurveInstance curve) {
        //the job of this is to spawn particles along the curve.
        //every frame, we determine a position along the curve to move the particle system
        //we don't have to handle emission, as it's already emitting over distance.

        bool success = false;

        Vector2 p1 = GetEffectPosition(curve, curve.t, out success);
        if (success) {
            curve.animator.boxManager.effectCurveEvent.Invoke(curve, p1);
        }
    }

    static float GetEffectTimeFromAnimationTime(Retro.CurveInstance curve, float animatorTime) {
        Retro.RetroAnimator animator = curve.animator;

        int index = Mathf.Clamp((animator.frame + 1), curve.startIndex, curve.endIndex);

        float normalTime = animatorTime / animator.duration;
        float normalStart = (float)curve.startIndex / animator.mySheet.count;
        float normalEnd = (float)curve.endIndex / animator.mySheet.count;

        float t = Helpers.Map(normalTime, normalStart, normalEnd, 0, 1, true);
        return t;
    }

    static Vector2 GetEffectPosition(Retro.CurveInstance curve, float t, out bool success) {
        Retro.RetroAnimator animator = curve.animator;

        Retro.Curve sCurve = curve.layer.CurveFromKeyFrames(curve.startIndex); //spatial curve
        if (sCurve != null) {
            Retro.Curve tCurve = curve.startFrame.props["velocityCurve"].curveVal; //temporal curve
            Vector2 tVal = tCurve.EvaluateForX(t);
            Vector2 startPoint = sCurve.Evaluate(0);
            Vector2 samplePoint = sCurve.Evaluate(tVal.y);
            float height = animator.spriteRenderer.sprite.rect.height;

            Vector2 pivot = animator.spriteRenderer.sprite.pivot;
            pivot.y = height / 2f - pivot.y;
            Vector2 offset = samplePoint - pivot;
            offset.y = height / 2f - offset.y;

            offset /= animator.spriteRenderer.sprite.pixelsPerUnit;
            Vector2 final = (Vector2)animator.transform.position + (offset * animator.transform.lossyScale);
            success = true;
            return final;
        } else {
            success = false;
            return default;
        }
    }



    static void HandleVelocityOffset(Retro.CurveInstance curve, Retro.Layer l, bool fixedUpdate) {
        bool foundTracker = false;
        Retro.RetroAnimator animator = curve.animator;

        bool ignoreX = false;
        bool ignoreY = false;
        bool isVelocityCurve = false;
        bool isTrackPivot = false;
        foreach (string s in curve.properties.Keys) {//currently active curves on each layer

            if (s.Equals("velocityCurve")) {
                isVelocityCurve = true;
            }

            if (s.Equals("trackPivot")) {
                isTrackPivot = true;
            }
            if (s.Equals("ignoreX") && curve.startFrame.props["ignoreX"].boolVal) {
                ignoreX = true;
            }
            if (s.Equals("ignoreY") && curve.startFrame.props["ignoreY"].boolVal) {
                ignoreY = true;
            }
        }


        if (isVelocityCurve) {//This hasn't really been properly implemented yet...

            Vector2 vel = animator.rigidBody.velocity;

            if (!ignoreX) {
                vel.x = curve.properties["velocityCurve"].x;
            }
            if (!ignoreY) {
                vel.y = curve.properties["velocityCurve"].y;
            }
            animator.rigidBody.velocity = vel;

        }

        if (isTrackPivot && !fixedUpdate) {
            bool propertyTrue = curve.startFrame.props["trackPivot"].boolVal;

            if (propertyTrue && isVelocityCurve) {

                foundTracker = true;
                //get the curve instances
                Retro.Curve sCurve = l.CurveFromKeyFrames(curve.startIndex); //spatial curve
                Retro.Curve tCurve = curve.startFrame.props["velocityCurve"].curveVal; //temporal curve

                float t = curve.GetNormalTime(true);
                Vector2 tVal = tCurve.EvaluateForX(t);

                //calculate an offset
                Vector2 startPoint = sCurve.Evaluate(0);
                Vector2 samplePoint = sCurve.Evaluate(tVal.y);
                Vector2 o = samplePoint - startPoint;

                //map to world space
                Vector2 expectedDistance = o / animator.spriteRenderer.sprite.pixelsPerUnit;
                expectedDistance.x *= -1;

                animator.localOffsetHash.AddEffector(curve.GetID(), expectedDistance);
            }
        }
    }


    public static void ApplyEventProperties(Retro.RetroAnimator animator) {
        Retro.Properties props = animator.mySheet.propertiesList[animator.frame];
        //AnimationEffectPool.current.ApplyEffects(anim);
        foreach (Retro.BoxProperty b in props.frameProperties) {
            switch (b.name) {
                case "Event":
                    if (animator.frameTagsEvent.ContainsEvent(b.stringVal)) {
                        animator.frameTagsEvent.Invoke(b.stringVal);
                    }
                    break;
            }

            if (b.dataType == Retro.PDataType.FMODEvent) {
                //   GlobalAudio.PlayClip(b.fmodEventVal, animator.transform);
            }
        }
    }

    public static void ApplyFrameProperties(MonoBehaviour behaviour, Retro.RetroAnimator animator, Rigidbody2D body, bool facingRight) {

        Retro.Properties props = animator.mySheet.propertiesList[animator.GetCurrentFrame()];
        //AnimationEffectPool.current.ApplyEffects(anim);

        bool x = false;
        bool y = false;
        Vector2 force = new Vector2();

        foreach (Retro.BoxProperty b in props.frameProperties) {

            switch (b.name) {
                case "Velocity":
                    force = new Vector3(b.vectorVal.x, b.vectorVal.y);
                    break;
                case "VelX":
                    x = true;
                    break;
                case "VelY":
                    y = true;
                    break;
                    /*
                    case "CanMove":
                        if (behaviour is Character c) {
                            c.input.canMove = b.boolVal;
                        }
                        break;
                    case "CanAttack":
                        if (behaviour is Character d) {
                            d.input.canAttack = b.boolVal;
                        }
                        break;
                    case "CanWall":
                        if (behaviour is Character e) {
                            e.input.canWall = b.boolVal;
                        }
                        break;
                        */
            }
        }

        if ((x || y) /*&& force != Vector2.zero*/) {
            if (!facingRight) force.x = -force.x;

            if (!x) force.x = body.velocity.x;
            if (!y) force.y = body.velocity.y;
            body.velocity = force;

        }
    }

    public static void ApplyFrameProperties(MonoBehaviour behaviour, Retro.RetroAnimator anim) { //no physics stuff

        Retro.Properties props = anim.mySheet.propertiesList[anim.GetCurrentFrame()];
        // AnimationEffectPool.current.ApplyEffects(anim);

        foreach (Retro.BoxProperty b in props.frameProperties) {
            switch (b.name) {
                case "Event":
                    behaviour.SendMessage(b.stringVal, SendMessageOptions.RequireReceiver);
                    //behaviour.GetType().InvokeMember(b.stringVal, System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public, null, behaviour, null);
                    break;
            }
        }
    }
}


