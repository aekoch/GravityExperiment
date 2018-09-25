using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GravityObjectManager : MonoBehaviour {
    public static GravityObjectManager instance = null;

    public GameObject focalPoint;
    public GravityGPU gpu;

    public Color negativeColor = Color.red;
    public Color neutralColor = Color.gray;
    public Color positiveColor = Color.blue;
    public List<GravityObject> managedObjects = new List<GravityObject>();

    public UnityEvent datasetChanged;

    private bool dirty = false;

    private void Awake() {
        if(instance != null) {
            Destroy(this);
        } else {
            instance = this;
        }
    }

    public void RegisterGravityObject(GravityObject obj) {
        managedObjects.Add(obj);
        obj.id = managedObjects.Count - 1;
        dirty = true;
    }

    public void UnregisterGravityObject(GravityObject obj) {
        if (managedObjects.Contains(obj)) {
            managedObjects.Remove(obj);
            dirty = true;
        }
    }
    
    public Color GetColor(GravityObject.Charge charge) {
        switch (charge) {
            case GravityObject.Charge.Negative:
                return negativeColor;
            case GravityObject.Charge.Neutral:
                return neutralColor;
            case GravityObject.Charge.Positive:
                return positiveColor;
            default:
                return Color.black;
        }
    }

    private void FixedUpdate() {
        if (dirty) {
            dirty = false;
            datasetChanged.Invoke();
            print("Dataset changed");
        }
        Vector3 avgPosition = Vector3.zero;
        foreach (GravityObject gravObj in managedObjects) {
            avgPosition += gravObj.transform.position;
            if (gpu.forces.Length > gravObj.id) {
                gravObj.ApplyForce(gpu.forces[gravObj.id]);
            }
        }
        avgPosition /= managedObjects.Count;
        focalPoint.transform.position = avgPosition;
    }
}
