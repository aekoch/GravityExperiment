using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Octree : MonoBehaviour {

    public List<Transform> data;
    public Transform target;
    public Cell root;

    public Vector3 center;
    public float boundingRadius;

    public Material lineMaterial;
    public Color targetCellColor = Color.green;
    public Color targetCellNeighborColor = Color.red;
    public Color defaultColor = Color.gray;

    public int maxParticlesPerCell = 20;
    public int maxDepth = 10;
    public int threshold = 1;
    public bool showOctree = true;
    public bool forceShowAllCells = false;

    public Dictionary<Transform, Cell> cellByTransform = new Dictionary<Transform, Cell>();
    public Dictionary<Cell, List<Transform>> dataByLeafCell = new Dictionary<Cell, List<Transform>>();

    #region Debug
    public int cellCount;
    public List<int> cellCountAtDepth = new List<int>();
    public int dataCount;
    public List<int> dataCountAtDepth = new List<int>();

    public void ResetDebug() {
        cellCount = 0;
        while(cellCountAtDepth.Count != maxDepth + 1) {
            if(cellCountAtDepth.Count > maxDepth + 1) {
                cellCountAtDepth.RemoveAt(cellCountAtDepth.Count - 1);
            } else {
                cellCountAtDepth.Add(0);
            }
        } for(int i = 0; i < cellCountAtDepth.Count; i++) {
            cellCountAtDepth[i] = 0;
        }
        
        dataCount = 0;
        while (dataCountAtDepth.Count != maxDepth + 1) {
            if (dataCountAtDepth.Count > maxDepth + 1) {
                dataCountAtDepth.RemoveAt(dataCountAtDepth.Count - 1);
            } else {
                dataCountAtDepth.Add(0);
            }
        }
        for (int i = 0; i < dataCountAtDepth.Count; i++) {
            dataCountAtDepth[i] = 0;
        }
    }
    #endregion

    private void Start() {
        GravityObjectManager.instance.datasetChanged.AddListener(ListenDataset);
        ListenDataset();
    }

    private void ListenDataset() {
        print("Received data set changed");
        data = GravityObjectManager.instance.managedObjects.ConvertAll(x => x.transform);
        target = (data != null && data.Count > 0) ? data[0] : null;
        if (root != null) {
            root.Deallocate();
        } else {
            root = new Cell();
        }
        root.transforms = new List<Transform>(data);
        root.octree = this;
        root.MergeOrSubdivide();
    }

    private void FixedUpdate() {
        ResetDebug();
        center = GetCenter();
        boundingRadius = GetBoundingRadius();
        if(root != null) {
            root.RecalculateBounds(center, boundingRadius);
            root.RedistributeRecursive();
            root.MergeOrSubdivide();
            root.GatherDebugData();
        }
        
    }

    public float GetBoundingRadius() {
        Vector3 maxDelta = Vector3.zero;
        foreach (Transform t in data) {
            Vector3 delta = t.position - center;
            maxDelta = (delta.magnitude > maxDelta.magnitude) ? delta : maxDelta;
        }
        Debug.DrawLine(center, center + maxDelta, Color.green);
        return maxDelta.magnitude;
    }

    public Vector3 GetCenter() {
        Vector3 result = Vector3.zero;
        foreach (Transform t in data) {
            result += t.position;
        }
        return result / data.Count;
    }

    private void OnRenderObject() {
        if (showOctree && root != null) {
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);

            var enumerator = root.GetChildrenAndSelf().GetEnumerator();
            while (enumerator.MoveNext()) {
                Cell cell = enumerator.Current;
                if (cell.transforms.Count >= threshold  || forceShowAllCells) {
                    GL.Color(defaultColor);
                    Vector3[] cube = MakeCubeLines(cell.center, cell.radius);
                    for(int i = 0; i < Cell.N_EDGE * 2; i++) {
                        GL.Vertex(cube[i]);
                    }
                }
            }
            GL.End();
            GL.PopMatrix();
        }

    }
    private Vector3[] MakeCubeLines(Vector3 center, float radius) {
        Vector3[] result = new Vector3[Cell.N_EDGE * 2];
        for(int i = 0; i < Cell.N_EDGE; i++) {
            result[i * 2] = center + radius * Cell.vertexIntToDirectionVector[Cell.edgeIntToVertexComponentInts[i][0]];
            result[i * 2 + 1] = center + radius * Cell.vertexIntToDirectionVector[Cell.edgeIntToVertexComponentInts[i][1]];
        }
        return result;
    }
}
