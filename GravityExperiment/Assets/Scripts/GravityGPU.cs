using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGPU : MonoBehaviour {
    const int WARP_SIZE = 256;
    const int VECTOR3 = 12;
    const int INT = 4;
    const int FLOAT = 4;
    const int PARTICLE = VECTOR3 + INT + FLOAT;

    private struct Particle{
        Vector3 position;
        float mass;
        int charge;

        public Particle(Vector3 position, float mass, int charge) {
            this.position = position;
            this.mass = mass;
            this.charge = charge;
        }
    }

    private ComputeBuffer forceBuffer;
    private ComputeBuffer particleBuffer;

    public Vector3[] forces;

    private int kernelID_CSMain;

    public ComputeShader gpu;

    private bool isSetup = false;

    private int bufferSize;

	void Setup () {
        bufferSize = particleCount();
        kernelID_CSMain = gpu.FindKernel("CSMain");
        ResizeBuffers(particleCount());
        UpdateBufferValues();
    }
	
	void FixedUpdate () {
        if (particleCount() > 0) {
            if (!isSetup) {
                Setup();
                this.isSetup = true;
            } else {
                if(particleCount() != bufferSize) {
                    ResizeBuffers(particleCount());
                }
                UpdateBufferValues();
                gpu.Dispatch(kernelID_CSMain, threadGroups(particleCount()), 1, 1);
                forceBuffer.GetData(forces);
            }
        }
	}

    private void OnDestroy() {
        particleBuffer.Release();
        forceBuffer.Release();
    }

    private int ChargeToInt(GravityObject.Charge charge) {
        switch (charge) {
            case GravityObject.Charge.Negative:
                return 0;
            case GravityObject.Charge.Neutral:
                return 1;
            case GravityObject.Charge.Positive:
                return 2;
            default:
                return 0;
        }
    }

    private int particleCount() {
        return GravityObjectManager.instance.managedObjects.Count;
    }
    
    private int threadGroups(int count) {
        return Mathf.Max(1, Mathf.CeilToInt((float)count / WARP_SIZE));
    }

    private void UpdateBufferValues() {
        Particle[] particleData = GravityObjectManager.instance.managedObjects.ConvertAll(x => ToParticle(x)).ToArray();
        particleBuffer.SetData(particleData);
    }

    private void ResizeBuffers(int _size_) {
        print("Realocating buffers");
        int size = Mathf.Max(1, _size_);
        if(particleBuffer != null) { particleBuffer.Release(); }
        if(forceBuffer != null) { forceBuffer.Release(); }

        particleBuffer = new ComputeBuffer(size, PARTICLE);
        forceBuffer = new ComputeBuffer(size, VECTOR3);
        forces = new Vector3[size];
        forceBuffer.SetData(forces);

        gpu.SetBuffer(kernelID_CSMain, "particles", particleBuffer);
        gpu.SetBuffer(kernelID_CSMain, "forces", forceBuffer);
        gpu.SetInt("numParticles", size);
        bufferSize = size;
    }

    private Particle ToParticle(GravityObject obj) {
        return new Particle(obj.transform.position, obj.mass, ChargeToInt(obj.charge));
    }
}