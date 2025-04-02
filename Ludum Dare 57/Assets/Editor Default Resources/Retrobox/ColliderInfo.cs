using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Retro {
    public class ColliderInfo : MonoBehaviour {

        private RetroAnimator animator;

        private Sheet sheet;//the sheet this collider comes from
        private int physicsLayer;
        public int layer;//the layer in which it's found
        public int frame; //the animation frame on which it occurs
                          //private float value; //the amount of damage (or other value) this collider inflicts
        public BoxProps props; //the list of properties owned by this hitbox
        public Collider2D col; //the collider itself
        public void Setup(Sheet s, int layer_, BoxProps props_, Collider2D collider_, RetroAnimator animator_) { //"Constructor"
            sheet = s;
            layer = layer_;
            physicsLayer = gameObject.layer;
            frame = 0;
            //value = v;
            props = props_;
            col = collider_;
            animator = animator_;
        }

        public void Setup(Sheet s, int layer_) {
            sheet = s;
            layer = layer_;
        }

        public void SetSheet(Sheet s) {
            sheet = s;
        }

        public void SetFrame(int f) { //"Update"
            frame = f;
        }


        public void SetFrame(int f, BoxProps props_) { //"Update"
            SetFrame(f);
            props = props_;
        }

        public BoxProps GetProperties() {
            return props;
        }

        public string GetBoxType() {
            if (sheet != null) {
                return sheet.layers[layer].myBoxType;
            } else {
                return "";
            }
        }

        public Retro.Sheet GetSheet() {
            return sheet;
        }

        public Retro.Layer GetRetroLayer() {
            if (sheet != null && sheet.layers != null && sheet.layers.Count > layer) {
                return sheet.layers[layer];
            }
            return null;
        }

        public int GetPhysicsLayer() {
            return physicsLayer;
        }

        public int GetFrame() {
            return frame;
        }


        bool IsColliderOurs(Collider2D collider) {
            return collider == col;
        }


        public void OnTriggerEnter2D(Collider2D _col) {

            if (IsHurtbox()) {

                ColliderInfo otherCol = _col.GetComponent<ColliderInfo>();
                Collision c = new Collision(this, otherCol);
                //make sure it's actually our box that got hit, not a neighbouring box on the same gameobject...
                if (otherCol /*&& CompatibleCollision(c)*/) {

                    if (animator != null) {
                        //Debug.Log(System.Array.IndexOf(transform.GetComponents<MonoBehaviour>(), this) + " | " + animator.name + " | " + animator.GetSheet().name + " | " + LayerMask.LayerToName(gameObject.layer) + " | " + LayerMask.LayerToName(_col.gameObject.layer));
                        animator.boxManager.collisionEvent.Invoke(c);

                        /*
                        if (otherCol.animator != null) {
                            otherCol.animator.boxManager.collisionEvent.Invoke(c);
                        }
                        */
                        animator.SendMessageUpwards("OnBoxCollision", c, SendMessageOptions.DontRequireReceiver);
                    } else {
                        SendMessageUpwards("OnBoxCollision", c, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        bool IsHurtbox() {
            bool hurtBox = false;
            if (sheet == null || sheet.layers == null) {
                hurtBox = true;
            } else {
                string boxTypeName = sheet.layers[layer].myBoxType;
                hurtBox = !GameSystem.system.boxPrefs.boxDictionary[boxTypeName].isAggressor;
            }
            return hurtBox;
        }

        public void Ban(RetroAnimator otherAnimator) {
            animator.boxManager.BanAnimator(col, otherAnimator);
        }

        public bool IsBanned(RetroAnimator otherAnimator) {
            return animator.boxManager.IsBanned(col, otherAnimator);
        }

        public Retro.RetroAnimator GetAnimator() {
            return animator;
        }
        /*
        public static bool CompatibleCollision(Retro.Collision collision) {
            int a = collision.collidee.GetPhysicsLayer();
            int b = collision.collider.GetPhysicsLayer();
            return (a == hurt && b == enemyHit) ||
                    (a == enemyHurt && b == hit) ||
                    ((a == default || a == item) && b == hit);
        }


        public static bool CompatibleCollision(Collider2D collidee, Collider2D collider) {
            return (collidee.gameObject.layer == hurt && collider.gameObject.layer == enemyHit) ||
                   (collidee.gameObject.layer == enemyHurt && collider.gameObject.layer == hit);
        }
        */

        public void OnDrawGizmosSelected() {
            if (col && sheet) {
                // Gizmos.color = GameSystem.system.boxPrefs.boxDictionary[sheet.layers[layer].myBoxType].colour;
                Helpers.DrawBounds(col.bounds);

            }
        }
    }

}