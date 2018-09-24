using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Octree : MonoBehaviour {
    private class RenderCube {
        private static Vector3 NEU_base = new Vector3(1, 1, 1);// * Mathf.Sqrt(3f);
        private static Vector3 SEU_base = new Vector3(1, 1, -1);// * Mathf.Sqrt(3f);
        private static Vector3 SWU_base = new Vector3(-1, 1, -1);// * Mathf.Sqrt(3f);
        private static Vector3 NWU_base = new Vector3(-1, 1, 1);// * Mathf.Sqrt(3f);
        private static Vector3 NED_base = new Vector3(1, -1, 1);// * Mathf.Sqrt(3f);
        private static Vector3 SED_base = new Vector3(1, -1, -1);// * Mathf.Sqrt(3f);
        private static Vector3 SWD_base = new Vector3(-1, -1, -1);// * Mathf.Sqrt(3f);
        private static Vector3 NWD_base = new Vector3(-1, -1, 1);// * Mathf.Sqrt(3f);

        public Vector3 center {
            get; private set;
        }
        public float radius {
            get; private set;
        }
        public Color color;

        private Vector3 NEU, NWU, SEU, SWU, NED, NWD, SED, SWD;

        public Vector3[] lineVertices = new Vector3[24];

        public RenderCube(Vector3 center, float radius) {
            this.center = center;
            this.radius = radius;
            this.color = Color.white;
            MakeVertices();
            MakeLines();
        }

        public RenderCube(Vector3 center, float radius, Color color) {
            this.center = center;
            this.radius = radius;
            this.color = color;
            MakeVertices();
            MakeLines();
        }

        private void MakeVertices() {
            NEU = center + NEU_base * radius;
            NWU = center + NWU_base * radius;
            SEU = center + SEU_base * radius;
            SWU = center + SWU_base * radius;
            NED = center + NED_base * radius;
            NWD = center + NWD_base * radius;
            SED = center + SED_base * radius;
            SWD = center + SWD_base * radius;
        }

        private void MakeLine(Vector3 a, Vector3 b, int index) {
            lineVertices[2 * index] = a;
            lineVertices[2 * index + 1] = b;
        }

        private void MakeLines() {
            MakeLine(NEU, SEU, 0);
            MakeLine(SEU, SWU, 1);
            MakeLine(SWU, NWU, 2);
            MakeLine(NWU, NEU, 3);
            MakeLine(NED, SED, 4);
            MakeLine(SED, SWD, 5);
            MakeLine(SWD, NWD, 6);
            MakeLine(NWD, NED, 7);
            MakeLine(NEU, NED, 8);
            MakeLine(SEU, SED, 9);
            MakeLine(SWU, SWD, 10);
            MakeLine(NWU, NWD, 11);
        }

        public void Update(Vector3 center, float radius) {
            this.center = center;
            this.radius = radius;
            MakeVertices();
            MakeLines();
        }
    }
    public class Cell {
        public enum Cardinal { N, E, S, W, U, D }
        public enum Edge { NU, EU, SU, WU, ND, ED, SD, WD, NE, NW, SE, SW }
        public enum Vertex { NEU, NWU, SEU, SWU, NED, NWD, SED, SWD }
        private static readonly Dictionary<Cardinal, Cardinal> oppositeCardinalDirection = new Dictionary<Cardinal, Cardinal>() {
            { Cardinal.N, Cardinal.S },
            { Cardinal.S, Cardinal.N },
            { Cardinal.E, Cardinal.W },
            { Cardinal.W, Cardinal.E },
            { Cardinal.U, Cardinal.D },
            { Cardinal.D, Cardinal.U },
        };
        private static readonly Dictionary<Vertex, Vector3> directionVectors_Vertex = new Dictionary<Vertex, Vector3> {
            { Vertex.NEU, new Vector3(1, 1, 1) },
            { Vertex.NWU, new Vector3(-1, 1, 1) },
            { Vertex.SEU, new Vector3(1, 1, -1) },
            { Vertex.SEU, new Vector3(-1, 1, -1) },
            { Vertex.NED, new Vector3(1, -1, 1) },
            { Vertex.NWD, new Vector3(-1, -1, 1) },
            { Vertex.SED, new Vector3(1, -1, -1) },
            { Vertex.SED, new Vector3(-1, -1, -1) },
        };
        private static readonly Dictionary<Cardinal, Vector3> directionVectors_Cardinal = new Dictionary<Cardinal, Vector3>() {
            {Cardinal.N, Vector3.forward },
            {Cardinal.S, Vector3.back },
            {Cardinal.E, Vector3.right },
            {Cardinal.W, Vector3.left },
            {Cardinal.U, Vector3.up },
            {Cardinal.D, Vector3.down },
        };
        private static readonly Dictionary<Edge, Vector3> directionVectors_Edge = new Dictionary<Edge, Vector3>() {
            {Edge.NU, new Vector3(0, 1, 1) },
            {Edge.EU, new Vector3(1, 0, 1) },
            {Edge.SU, new Vector3(0, -1, 1) },
            {Edge.WU, new Vector3(-1, 0, 1) },
            {Edge.ND, new Vector3(0, 1, -1) },
            {Edge.ED, new Vector3(1, 0, -1) },
            {Edge.SD, new Vector3(0, -1, -1) },
            {Edge.WD, new Vector3(-1, 0, -1) },
            {Edge.NE, new Vector3(1, 1, 0) },
            {Edge.NW, new Vector3(-1, 1, 0) },
            {Edge.SE, new Vector3(1, -1, 0) },
            {Edge.SW, new Vector3(-1, -1, 0) }
        };

        public Octree octree;
        public Vector3 center;
        public float radius;
        public int depth;
        public Cell parent;
        public Cell[] children;
        public List<Transform> transforms;
        private bool[] xyzPosNeg = new bool[3];
        private bool xPositive;
        private bool yPositive;
        private bool zPositive;
        public List<Cell> neighbors;
        private Cell[] cardinalNeighbors;
        private Cell[] edgeNeighbors;
        private Cell[] vertexNeighbors;

        public Cell() {
            OctreeStaticDebug.cellCount += 1;
            transforms = new List<Transform>();
            neighbors = new List<Cell>();
        }

        private static readonly Vector3[] vertex_base = new Vector3[] {
            new Vector3(1,1,1), new Vector3(1,1,-1), new Vector3(-1,1,-1), new Vector3(-1,1,1),
            new Vector3(1,-1,1), new Vector3(1,-1,-1), new Vector3(-1,-1,-1), new Vector3(-1,-1,1)
        };

        private Cell GetChildDirectional(bool xPos, bool yPos, bool zPos) {
            if(children == null) {
                Debug.LogWarning("Attempting to access the child of a leaf node");
                return null;
            } else {
                for(int i = 0; i < 8; i++) {
                    Vector3 v = vertex_base[i];
                    if (xPos == v.x > 0 && yPos == v.y > 0 && zPos == v.z > 0) {
                        return children[i];
                    }
                }
                Debug.LogError("Directional child error, octree not set up correctly");
                return null;//Unreachable
            }
        }

        public void GetUpNeighbor() {
            Stack<bool[]> nonFocalStack = new Stack<bool[]>();
            Cell cell = this;
            while(cell != null) {
                if (!cell.yPositive) {
                    nonFocalStack.Push(new bool[] { cell.xPositive, true, cell.zPositive });
                    cell = cell.parent;
                    break;
                } else {
                    nonFocalStack.Push(new bool[] { cell.xPositive, false, cell.zPositive });
                    cell = cell.parent;
                }
                //neighbors.Add(cell);
            }
            while (cell != null && cell.children != null && cell.depth < this.depth) {
                bool[] nonFocal = nonFocalStack.Pop();
                cell = cell.GetChildDirectional(nonFocal[0], nonFocal[1], nonFocal[2]);
                //neighbors.Add(cell);
            }
            neighbors.Add(cell);
        }

        public void GetNeighbors() {
            GetCardinalNeighbors();
            GetEdgeNeighbors();
            GetVertexNeighbors();
        }

        private void GetCardinalNeighbors() {
            if(cardinalNeighbors == null) { cardinalNeighbors = new Cell[6]; }
            cardinalNeighbors[0] = GetCardinalNeighbor(0, true);
            cardinalNeighbors[1] = GetCardinalNeighbor(0, false);
            cardinalNeighbors[2] = GetCardinalNeighbor(1, true);
            cardinalNeighbors[3] = GetCardinalNeighbor(1, false);
            cardinalNeighbors[4] = GetCardinalNeighbor(2, true);
            cardinalNeighbors[5] = GetCardinalNeighbor(2, false);
        }

        private Cell GetCardinalNeighbor(int index, bool posNeg) {
            Stack<bool[]> directionMemoryStack = new Stack<bool[]>();
            Cell cell = this;
            while(cell != null) { //roll up the tree until root or containing cell is not the direction 
                if(cell.xyzPosNeg[index] != posNeg) { //direction we're seeking
                    bool[] directionMemory = new bool[3];
                    directionMemory[0] = (index == 0) ? posNeg : cell.xyzPosNeg[0];
                    directionMemory[1] = (index == 1) ? posNeg : cell.xyzPosNeg[1];
                    directionMemory[2] = (index == 2) ? posNeg : cell.xyzPosNeg[2];
                    directionMemoryStack.Push(directionMemory);
                    cell = cell.parent;
                    break;
                } else {
                    bool[] directionMemory = new bool[3];
                    directionMemory[0] = (index == 0) ? !posNeg : cell.xyzPosNeg[0];
                    directionMemory[1] = (index == 1) ? !posNeg : cell.xyzPosNeg[1];
                    directionMemory[2] = (index == 2) ? !posNeg : cell.xyzPosNeg[2];
                    directionMemoryStack.Push(directionMemory);
                    cell = cell.parent;
                }
            }
            while (cell != null && cell.children != null && cell.depth < this.depth) {
                bool[] directionMemory = directionMemoryStack.Pop();
                cell = cell.GetChildDirectional(directionMemory[0], directionMemory[1], directionMemory[2]);
            }
            neighbors.Add(cell);
            return cell;
            
        }

        private void GetEdgeNeighbors() {

        }

        private void GetEdgeNeighbor(int index1, int index2) {

        }

        private void GetVertexNeighbors() {

        }

        private void GetVertexNeighbor(int index1, int index2, int index3) {

        }

        private bool InBounds(Vector3 vector) {
            return vector.x >= center.x - radius && vector.x <= center.x + radius
                && vector.y >= center.y - radius && vector.y <= center.y + radius
                && vector.z >= center.z - radius && vector.z <= center.z + radius;
        }

        private bool ShouldSubdivide() {
            //return depth < octree.maxDepth;
            return transforms.Count > octree.maxParticlesPerCell;
        }

        private void Subdivide() {
            if (depth < octree.maxDepth) {
                children = new Cell[8];
                for (int i = 0; i < 8; i++) {
                    Cell child = new Cell();
                    child.center = center + vertex_base[i] * 0.5f * radius;
                    child.radius = 0.5f * radius;
                    child.octree = octree;
                    child.parent = this;
                    child.depth = depth + 1;
                    foreach (Transform t in transforms) {
                        if (child.InBounds(t.position)) {
                            child.AddData(t);
                            octree.cellByTransform[t] = child;
                            continue;
                        }
                    }
                    child.xyzPosNeg[0] = vertex_base[i].x > 0;
                    child.xyzPosNeg[1] = vertex_base[i].y > 0;
                    child.xyzPosNeg[2] = vertex_base[i].z > 0;
                    child.xPositive = vertex_base[i].x > 0;
                    child.yPositive = vertex_base[i].y > 0;
                    child.zPositive = vertex_base[i].z > 0;
                    children[i] = child;
                }
                //print("Successfully subdivided: " + children.Length + " children");
                transforms.Clear();
                foreach (Cell child in children) {
                    if (child.ShouldSubdivide()) {
                        child.Subdivide();
                    }
                }
            }
        }

        private bool ShouldMergeChildren() {
            //return depth >= octree.maxDepth;
            if(children == null) {
                return false;
            }
            int childSum = 0;
            foreach (Cell child in children) {
                childSum += child.transforms.Count;
                if (childSum > octree.maxParticlesPerCell || child.children != null) {
                    return false;
                }
            }
            //print("Should merge - only " + childSum + " in children");
            return true;
        }

        private void MergeChildren() {
            foreach (Cell child in children) {
                foreach(Transform t in child.transforms) {
                    transforms.Add(t);
                    continue;
                }
                child.Deallocate();
            }
            //print("Successfully merged with " + transforms.Count);
            children = null;
            if (parent != null && parent.ShouldMergeChildren()) {
                parent.MergeChildren();
            }
        }

        private void DistributeRecursive(Transform t) {
            foreach (Cell child in children) {
                if (child.InBounds(t.position)) {
                    if (child.children != null) {
                        child.DistributeRecursive(t);
                    } else {
                        child.transforms.Add(t);
                        octree.cellByTransform[t] = child;
                    }
                }
            }
        }

        private void AddData(Transform t) {
            if (transforms == null) {
                transforms = new List<Transform>();
            }
            transforms.Add(t);
        }

        private void RemoveData(Transform t) {
            if (transforms != null && transforms.Contains(t)) {
                if (transforms.Contains(t)) {
                    transforms.Remove(t);
                }
            }
        }

        public void Deallocate() {
            OctreeStaticDebug.cellCount -= 1;
            if (children != null) {
                foreach (Cell child in children) {
                    child.Deallocate();
                }
            }
            transforms.Clear();
        }

        public void UpdateBoundsRecursive(Vector3 center, float radius) {
            this.center = center;
            this.radius = radius;
            if (children != null) {
                for (int i = 0; i < 8; i++) {
                    children[i].UpdateBoundsRecursive(center + 0.5f * radius * vertex_base[i], 0.5f * radius);
                }
            }
        }

        public void RedistributeRecursive() {
            if (children != null) {
                if(transforms.Count > 0) {
                    foreach(Transform t in transforms) {
                        DistributeRecursive(t);
                    }
                    transforms.Clear();
                }
                foreach (Cell child in children) {
                    child.RedistributeRecursive();
                }
            } else {
                for(int i = transforms.Count - 1; i >= 0; i--) {
                    Transform t = transforms[i];
                    if (!InBounds(t.position)) {
                        octree.root.DistributeRecursive(t);
                        transforms.RemoveAt(i);
                    }
                }
            }
        }

        public void ResizeRecursive() {
            if (children != null) {
                foreach (Cell child in children) {
                    child.ResizeRecursive();
                }
            } else {
                //print("Resizing");
                if (ShouldSubdivide()) {
                    Subdivide();
                } 
                else if (parent != null && parent.children != null && parent.children[0] == this && parent.ShouldMergeChildren()) {
                    //print("Checking merger");
                    parent.MergeChildren();
                }
            }
        }

        public IEnumerable<Cell> GetChildrenAndSelf() {
            if(children != null) {
                foreach(Cell child in children) {
                    foreach(Cell cell in child.GetChildrenAndSelf()) {
                        yield return cell;
                    }
                }
            }
            yield return this;
        }

        public void GatherDebugData() {
            octree.cellCount++;
            octree.cellCountAtDepth[depth]++;
            octree.dataCount += transforms.Count;
            octree.dataCountAtDepth[depth] += transforms.Count;
            if (children != null) {
                foreach (Cell cell in children) {
                    cell.GatherDebugData();
                }
            }
        }
    }

    #region Public Properties
    public List<Transform> data = new List<Transform>();
    public Transform target;
    public Cell root;

    public Vector3 center;
    public float boundingRadius;

    public Material material;
    public Color targetCellColor = Color.green;
    public Color targetCellNeighborColor = Color.red;
    public Color defaultColor = Color.gray;

    public int maxParticlesPerCell = 20;

    public int maxDepth = 10;
    public int threshold = 1;
    public bool showOctree = true;

    public Dictionary<Transform, Cell> cellByTransform = new Dictionary<Transform, Cell>();
    #endregion

    private int cellCount;
    private int[] cellCountAtDepth;
    private int dataCount;
    private int[] dataCountAtDepth;

    public void ResetDebug() {
        cellCount = 0;
        cellCountAtDepth = new int[maxDepth + 1];
        dataCount = 0;
        dataCountAtDepth = new int[maxDepth + 1];
    }

    // Use this for initialization
    void Start () {
        GravityObjectManager.instance.datasetChanged.AddListener(ListenDataset);
        OctreeRenderer.instance.RegisterOctree(this);
        root = new Cell();
        root.octree = this;
        root.transforms = data;
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        center = GetCenter();
        boundingRadius = GetBoundingRadius();
        root.UpdateBoundsRecursive(center, boundingRadius);
        root.RedistributeRecursive();
        root.ResizeRecursive();
        ResetDebug();
        root.GatherDebugData();
        if (cellByTransform.ContainsKey(target)) {
            print(cellByTransform[target].depth);
            //cellByTransform[target].GetUpNeighbor();
            cellByTransform[target].GetNeighbors();
        }
	}

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireCube(center, Vector3.one * boundingRadius * 2);
        Gizmos.DrawWireSphere(center, boundingRadius);
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

    private void ListenDataset() {
        data = GravityObjectManager.instance.managedObjects.ConvertAll(x => x.transform);
        target = (data != null && data.Count > 0) ? data[0] : null;
        root.Deallocate();
        root.transforms = new List<Transform>(data);
    }

    private Color GetColor(Cell cell) {
        if(target == null) {
            return defaultColor;
        } else {
            if (target && cellByTransform.ContainsKey(target)) {
                if (cell == cellByTransform[target]) {
                    return targetCellColor;
                }
                if (cellByTransform[target].neighbors.Contains(cell)) {
                    return targetCellNeighborColor;
                }
            }
        }
        return defaultColor;
        
    }

    private void OnRenderObject() {
        if (showOctree) {
            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            material.SetPass(0);
            GL.Begin(GL.LINES);

            List<Cell> renderLast = new List<Cell>();
            foreach (Cell cell in root.GetChildrenAndSelf()) {
                if (cellByTransform.ContainsKey(target) && (cell == cellByTransform[target] || cellByTransform[target].neighbors.Contains(cell))) {
                    renderLast.Add(cell);
                } else if (cell.transforms.Count >= threshold) {
                    GL.Color(GetColor(cell));
                    RenderCube cube = new RenderCube(cell.center, cell.radius);
                    foreach (Vector3 v in cube.lineVertices) {
                        GL.Vertex(v);
                    }
                }
            }

            foreach (Cell cell in renderLast) {
                GL.Color(GetColor(cell));
                RenderCube cube = new RenderCube(cell.center, cell.radius);
                foreach (Vector3 v in cube.lineVertices) {
                    GL.Vertex(v);
                }
            }
            if (target && cellByTransform.ContainsKey(target)) {
                GL.Color(targetCellColor);
                Cell c = cellByTransform[target];
                RenderCube cube = new RenderCube(c.center, c.radius);
                foreach (Vector3 v in cube.lineVertices) {
                    GL.Vertex(v);
                }
            }


            GL.End();
            GL.PopMatrix();
        }
        
    }
}
