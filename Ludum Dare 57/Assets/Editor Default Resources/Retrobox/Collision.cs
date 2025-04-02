using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Retro {
    public class Collision {

        public ColliderInfo collidee;
        public ColliderInfo collider;

        public Collision(ColliderInfo _collidee, ColliderInfo _collider) {
            collidee = _collidee;
            collider = _collider;
        }
    }
}
