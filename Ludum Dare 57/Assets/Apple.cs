using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apple : MonoBehaviour, IContainable {
    public AudioClip appleGet;
    public LevelContainer container { get; set; }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        //if current container is ours and the player is near us 
        if (container.IsCurrentIndex() && IsNearCharacter()) {
            GameManager.i.audioSource.PlayOneShot(appleGet);
            gameObject.SetActive(false);
        }
    }

    public bool IsNearCharacter() {
        return Vector2.Distance(GameManager.i.character.transform.position + Vector3.up * 0.2f, transform.position) < 0.16f;
    }
}
