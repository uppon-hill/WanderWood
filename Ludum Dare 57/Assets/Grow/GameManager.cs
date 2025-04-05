using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public List<Transform> lights;
    public static GameManager instance;

    public void Awake() {

        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }
}