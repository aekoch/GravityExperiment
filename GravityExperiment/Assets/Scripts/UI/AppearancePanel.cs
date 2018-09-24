using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AppearancePanel : MonoBehaviour {

    public Octree octree;

    public Toggle trailsToggle;
    public Toggle meshToggle;
    public Toggle showOctreeToggle;
    public InputField thresholdInput;

	// Use this for initialization
	void Start () {
        trailsToggle.onValueChanged.AddListener(ListenTrailsToggle);
        meshToggle.onValueChanged.AddListener(ListenMeshToggle);
        showOctreeToggle.onValueChanged.AddListener(ListenShowOctreeToggle);
        thresholdInput.onValueChanged.AddListener(ListenThresholdInput);
	}
	
	// Update is called once per frame
	void Update () {
        showOctreeToggle.isOn = octree.showOctree;
	}

    private void ListenTrailsToggle(bool tf) {
        foreach (GravityObject obj in GravityObjectManager.instance.managedObjects) {
            obj.GetComponent<TrailRenderer>().enabled = tf;
        }
    }

    private void ListenMeshToggle(bool tf) {
        foreach(GravityObject obj in GravityObjectManager.instance.managedObjects) {
            obj.GetComponent<Renderer>().enabled = tf;
        }
    }

    private void ListenShowOctreeToggle(bool tf) {
        octree.showOctree = tf;
    }

    public void ListenThresholdInput(string strInput) {
        int value = int.Parse(strInput);
        value = Mathf.Clamp(value, 0, octree.maxParticlesPerCell);
        octree.threshold = value;
    }
}
