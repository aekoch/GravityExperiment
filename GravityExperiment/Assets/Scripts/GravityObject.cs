using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityObject : MonoBehaviour {

    public enum Charge { Negative = -1, Neutral = 0, Positive = 1 }
    public Charge charge = Charge.Neutral;
    public float mass = 1f;
    public float density = 1f;

    public int id = -1;

    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        GravityObjectManager.instance.RegisterGravityObject(this);
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        transform.localScale = Vector3.one * GetRadiusFromMass();
        GetComponent<Renderer>().material.SetColor("_Color", GravityObjectManager.instance.GetColor(charge));
	}

    public void ApplyForce(Vector3 force) {
        rb.AddForce(force, ForceMode.Force);
    }

    private float GetRadiusFromMass() {
        return Mathf.Pow(mass, 1f / 3f) / density;
    }

    private bool ShouldAttract(GravityObject other) {
        switch (charge) {
            case Charge.Neutral:
                return true;
            case Charge.Negative:
                return !(other.charge == Charge.Negative);
            case Charge.Positive:
                return !(other.charge == Charge.Positive);
            default:
                return false;
        }
    }

    private bool ShouldRepel(GravityObject other) {
        switch (charge) {
            case Charge.Neutral:
                return false;
            case Charge.Negative:
                return (other.charge == Charge.Negative);
            case Charge.Positive:
                return (other.charge == Charge.Positive);
            default:
                return false;
        }
    }

    /*private void OnCollisionEnter(Collision collision) {
        //print(collision.relativeVelocity.magnitude);
    }

    private void OnCollisionStay(Collision collision) {

    }*/

    //private IEnumerator MergeAfterTimeElapsed(float t) {

    //}

    private void OnDestroy() {
        GravityObjectManager.instance.UnregisterGravityObject(this);
    }
}
