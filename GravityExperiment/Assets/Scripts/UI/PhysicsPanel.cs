using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhysicsPanel : MonoBehaviour {

    public Toggle collisionsToggle;
    public Slider dragSlider;
    public Slider bouncinesSlider;

    public PhysicMaterial physicMaterial;

	// Use this for initialization
	void Start () {
        collisionsToggle.onValueChanged.AddListener(ListenCollisionToggle);
        dragSlider.onValueChanged.AddListener(ListenDragSlider);
        bouncinesSlider.onValueChanged.AddListener(ListenBouncinessSlider);
	}

    private void ListenCollisionToggle(bool tf) {
        foreach(GravityObject obj in GravityObjectManager.instance.managedObjects) {
            obj.GetComponent<Collider>().enabled = tf;
        }
    }

    private void ListenDragSlider(float value) {
        foreach(GravityObject obj in GravityObjectManager.instance.managedObjects) {
            obj.GetComponent<Rigidbody>().drag = 0.1f * value * obj.mass;
        }
    }

    private void ListenBouncinessSlider(float value) {
        physicMaterial.bounciness = value;
    }
}
