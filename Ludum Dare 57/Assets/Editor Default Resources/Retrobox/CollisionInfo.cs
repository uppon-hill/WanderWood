using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionInfo {

    public List<MonoBehaviour> handlers;
    public Retro.Collision collision;
    //Collidee 
    public bool ignoreHurt => collideeProps.ignoreHurt;
    //Collider
    public int damage => (int)(colliderProps.damage * collideeProps.vulnerability);
    public float knockback => colliderProps.knockback;
    public float hitstopValue => colliderProps.hitstopValue;
    public Vector2 damageVector => colliderProps.damageVector;

    public Vector2 scaledDamageVector => damageVector * knockback;

    public float poise; //starts at life.poise, subtracts the collision poise from that. if less than 75%, get stunned. if 0, get knocked down.
    public float normalisedPoise;

    public ProcessedColliderProperties collideeProps;
    public ProcessedColliderProperties colliderProps;
    // Life collideeLife;
    public Vector2 overlapPoint;

    public CollisionInfo(/*Life l,*/ Retro.Collision collision_) {
        //collideeLife = l;
        collision = collision_;
        collideeProps = new ProcessedColliderProperties(collision.collidee);
        colliderProps = new ProcessedColliderProperties(collision.collider);
        float collideePoise = /*collideeLife.poise + */ collideeProps.poise;
        float colliderPoise = colliderProps.damage + colliderProps.poise;
        poise = collideePoise - colliderPoise;
        if (collideePoise <= 0) {
            normalisedPoise = 0;
        } else {
            normalisedPoise = (collideePoise - colliderPoise) / collideePoise;
        }

        handlers = new List<MonoBehaviour>();
        overlapPoint = GetOverlap();

    }


    public Vector2 GetOverlap() {
        Collider2D ca = collision.collidee.col;
        Collider2D cb = collision.collider.col;

        // Assuming collider1 and collider2 are already initialized
        Vector2 min = Vector2.Max(new Vector2(ca.bounds.min.x, ca.bounds.min.y), new Vector2(cb.bounds.min.x, cb.bounds.min.y));
        Vector2 max = Vector2.Min(new Vector2(ca.bounds.max.x, ca.bounds.max.y), new Vector2(cb.bounds.max.x, cb.bounds.max.y));

        if (min.x < max.x && min.y < max.y) // Check if the rectangles actually overlap
        {
            return (min + max) / 2f;
        }

        //average the position between the collider centres
        return (ca.bounds.center + cb.bounds.center) / 2f;
    }
}

public struct ProcessedColliderProperties {

    //Collidee 
    public bool ignoreHurt;
    public float vulnerability;

    //Collider
    public int damage;
    public float knockback;
    public float hitstopValue;
    public Vector2 damageVector;
    public float poise;
    public Collider2D collider;
    public string colliderTag;
    public Retro.Sheet sheet;
    public int frame;
    public Retro.Layer layer;


    public ProcessedColliderProperties(Retro.ColliderInfo info) {
        this = new ProcessedColliderProperties();
        poise = 0;
        vulnerability = 1;
        hitstopValue = 1;
        knockback = 1;
        colliderTag = default;
        collider = info.col;
        sheet = info.GetSheet();
        frame = info.frame;
        layer = info.GetRetroLayer();

        foreach (KeyValuePair<string, Retro.BoxProperty> bp in info.props) { //process properties
            ProcessProperty(bp.Value);
        }
    }

    void ProcessProperty(Retro.BoxProperty p) {

        switch (p.name) {
            case "damage":
                damage = (int)p.floatVal;
                break;

            case "hitstop":
                hitstopValue = (p.floatVal == 0) ? 1 : p.floatVal;

                //if (Engine.IsArmin(collision.collidee.col)) {
                //    hitstopValue *= ArminBehaviour.hitStopMod;
                //}

                break;

            case "effectVector":
                damageVector = (p.vectorVal).normalized * collider.transform.lossyScale;
                break;

            case "tag":
                colliderTag = p.stringVal;
                //SendMessage(p.stringVal, col, SendMessageOptions.DontRequireReceiver); //call function
                break;

            case "knockback":
                knockback = p.floatVal;
                break;

            case "ignoreHurt":
                ignoreHurt = p.boolVal;
                break;

            case "vulnerability":
                vulnerability = p.floatVal;
                break;
            case "poise":
                poise = p.floatVal;
                break;
        }

    }



}