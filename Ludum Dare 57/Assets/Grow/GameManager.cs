using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour {

    public AudioSource audioSource;
    public AudioClip orbGet;
    public AudioClip orbDrop;
    public List<Transform> lights;
    public static GameManager i;
    public LevelZoomer zoomer;
    public Pointer pointer;
    public Orb selectedOrb;
    public Character character;
    public Shaker cameraShaker;
    public UnityEvent<Transform> onSelect;
    public SpriteRenderer shade;
    public List<GameObject> apples;
    public bool shouldBounce = false;
    public void Awake() {
        if (i == null) {
            i = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void Update() {
        /*
                if (Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
                    Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height) {
                    UnityEngine.Cursor.visible = false; // Hide the cursor
                } else {
                    UnityEngine.Cursor.visible = true; // Show the cursor
                }
        */
        if (selectedOrb != null) {
            selectedOrb.SetPosition(character.orbSlot.position);
        }
        shade.sortingOrder = zoomer.currentLevel.sortingOrder - 1;
    }

    public void DeselectOrb() {

        selectedOrb.Reparent(zoomer.currentLevel.transform);
        zoomer.currentLevel.AddRenderer(selectedOrb.sprite);
        selectedOrb = null;
        audioSource.PlayOneShot(orbDrop);
    }

    public void SelectOrb(Orb orb) {
        if (selectedOrb == null) {
            audioSource.PlayOneShot(orbGet);
            zoomer.currentLevel.RemoveRenderer(orb.sprite);
            selectedOrb = orb;
            selectedOrb.Reparent(character.orbSlot);
        }

    }

    public void Select() {
        onSelect.Invoke(character.orbSlot);
    }

    public void Deselect() {
        GameManager.i.DeselectOrb();
    }

    public int GetAppleScore() {
        int score = 0;
        foreach (GameObject apple in apples) {
            if (!apple.activeSelf) {
                score++;
            }
        }
        return score;
    }

}