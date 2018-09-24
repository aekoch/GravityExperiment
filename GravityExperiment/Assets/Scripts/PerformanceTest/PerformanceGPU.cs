using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceGPU : MonoBehaviour {
    const int WARP_SIZE = 256;
    const int VECTOR3 = 12;
    const int INT = 4;
    const int FLOAT = 4;

    public ComputeShader gpu;
    public int n = 10000;

    private ComputeBuffer sourceBuffer;
    private ComputeBuffer returnBuffer;

    public float[] sourceData;
    public float[] returnData;

    private int kernelId_CSMain;
    private int bufferSize;

	// Use this for initialization
	void Start () {
        bufferSize = n;
        kernelId_CSMain = gpu.FindKernel("CSMain");
        ResizeBuffers(n);
        UpdateBufferValues();
    }
	
	// Update is called once per frame
	void Update () {
		if(n != bufferSize) {
            ResizeBuffers(n);
        }
        UpdateBufferValues();
        gpu.Dispatch(kernelId_CSMain, threadGroups(bufferSize), 1, 1);
        returnBuffer.GetData(returnData);
    }

    private int threadGroups(int count) {
        return Mathf.Max(1, Mathf.CeilToInt((float)count / WARP_SIZE));
    }

    private void UpdateBufferValues() {
        //Currently unused
    }

    private void ResizeBuffers(int _size_) {
        print("Realocating buffers");
        int size = Mathf.Min(50000000, Mathf.Max(1, _size_));

        //Release buffers
        if (sourceBuffer != null) { sourceBuffer.Release(); }
        if (returnBuffer != null) { returnBuffer.Release(); }

        //Reinstantiate buffers
        sourceBuffer = new ComputeBuffer(size, FLOAT);
        returnBuffer = new ComputeBuffer(size, FLOAT);

        //Reset buffer data
        sourceData = new float[size];
        //for(int i = 0; i < size; i++) { sourceData[i] = Random.value; }
        returnData = new float[size];

        //Apply data to buffers
        //sourceBuffer.SetData(sourceData);
        //returnBuffer.SetData(returnData);

        //Send buffer to GPU
        gpu.SetBuffer(kernelId_CSMain, "sourceBuffer", sourceBuffer);
        gpu.SetBuffer(kernelId_CSMain, "returnBuffer", returnBuffer);

        //Ensure size variables consistent with actual size
        gpu.SetInt("size", size);
        bufferSize = size;
    }

    private void OnDestroy() {
        sourceBuffer.Release();
        returnBuffer.Release();
    }
}
