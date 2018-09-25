using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Cell {
    private static Queue<Cell> pool = new Queue<Cell>();
    #region Directions
    public enum Axis { X, Y, Z }
    public enum Cardinal { N, E, S, W, U, D }
    public enum Edge { NU, EU, SU, WU, ND, ED, SD, WD, NE, NW, SE, SW }
    public enum Vertex { NEU, NWU, SEU, SWU, NED, NWD, SED, SWD }
    private static readonly Dictionary<Cardinal, Axis> cardinalAxis = new Dictionary<Cardinal, Axis>() {
        { Cardinal.N, Axis.Z },
        { Cardinal.S, Axis.Z },
        { Cardinal.E, Axis.X },
        { Cardinal.W, Axis.X },
        { Cardinal.U, Axis.Y },
        { Cardinal.D, Axis.Y }
    };
    private static readonly Dictionary<Axis, List<Cardinal>> axisCardinals = new Dictionary<Axis, List<Cardinal>>() {
        { Axis.X, new List<Cardinal>( new Cardinal[]{ Cardinal.E, Cardinal.W }) },
        { Axis.Y, new List<Cardinal>( new Cardinal[]{ Cardinal.U, Cardinal.D }) },
        { Axis.Z, new List<Cardinal>( new Cardinal[]{ Cardinal.N, Cardinal.S }) },
    };
    private static readonly Dictionary<Cardinal, Cardinal> oppositeCardinalDirection = new Dictionary<Cardinal, Cardinal>() {
            { Cardinal.N, Cardinal.S },
            { Cardinal.S, Cardinal.N },
            { Cardinal.E, Cardinal.W },
            { Cardinal.W, Cardinal.E },
            { Cardinal.U, Cardinal.D },
            { Cardinal.D, Cardinal.U },
        };
    private static readonly Dictionary<Vertex, List<Cardinal>> vertexComponents = new Dictionary<Vertex, List<Cardinal>>() {
        { Vertex.NEU, new List<Cardinal>(new Cardinal[] { Cardinal.N, Cardinal.E, Cardinal.U}) },
        { Vertex.NWU, new List<Cardinal>(new Cardinal[] { Cardinal.N, Cardinal.W, Cardinal.U}) },
        { Vertex.SEU, new List<Cardinal>(new Cardinal[] { Cardinal.S, Cardinal.E, Cardinal.U}) },
        { Vertex.SWU, new List<Cardinal>(new Cardinal[] { Cardinal.S, Cardinal.W, Cardinal.U}) },
        { Vertex.NED, new List<Cardinal>(new Cardinal[] { Cardinal.N, Cardinal.E, Cardinal.D}) },
        { Vertex.NWD, new List<Cardinal>(new Cardinal[] { Cardinal.N, Cardinal.W, Cardinal.D}) },
        { Vertex.SED, new List<Cardinal>(new Cardinal[] { Cardinal.S, Cardinal.E, Cardinal.D}) },
        { Vertex.SWD, new List<Cardinal>(new Cardinal[] { Cardinal.S, Cardinal.W, Cardinal.D}) },
    };
    private static readonly Dictionary<Vertex, Vector3> directionVectors_Vertex = new Dictionary<Vertex, Vector3> {
            { Vertex.NEU, new Vector3(1, 1, 1) },
            { Vertex.NWU, new Vector3(-1, 1, 1) },
            { Vertex.SEU, new Vector3(1, 1, -1) },
            { Vertex.SWU, new Vector3(-1, 1, -1) },
            { Vertex.NED, new Vector3(1, -1, 1) },
            { Vertex.NWD, new Vector3(-1, -1, 1) },
            { Vertex.SED, new Vector3(1, -1, -1) },
            { Vertex.SWD, new Vector3(-1, -1, -1) },
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
    private Vertex vertexFromCardinals(Cardinal cardinal1, Cardinal cardinal2, Cardinal cardinal3) {
        foreach (Vertex vertex in vertexComponents.Keys) {
            if(vertexComponents[vertex].Contains(cardinal1)
            && vertexComponents[vertex].Contains(cardinal2)
            && vertexComponents[vertex].Contains(cardinal3)) {
                return vertex;
            }
        }
        Debug.LogError("Invalid vertex: " + System.Enum.GetName(typeof(Cardinal), cardinal1) + System.Enum.GetName(typeof(Cardinal), cardinal2) + System.Enum.GetName(typeof(Cardinal), cardinal3));
        return Vertex.NEU;//Unreachable;
    }
    private Vertex vertexFromCardinals(List<Cardinal> cardinals) {
        return vertexFromCardinals(cardinals[0], cardinals[1], cardinals[2]);
    }
    #endregion

    public Octree octree;
    public Vector3 center;
    public float radius;
    public int depth;
    public Vertex vertexInParent;
    public Cell parent;
    public Cell[] children;
    public List<Transform> transforms;
    public List<Cell> neighbors;
    private Cell[] cardinalNeighbors;
    private Cell[] edgeNeighbors;
    private Cell[] vertexNeighbors;

    private UnityEvent onSubdivide = new UnityEvent();
    private UnityEvent onMerge = new UnityEvent();

    private static Cell MakeCell() {
        Debug.Log(pool.Count);
        if (pool.Count != 0) {
            Cell result = pool.Dequeue();
            result.children = null;
            result.cardinalNeighbors = new Cell[6];
            result.edgeNeighbors = new Cell[12];
            result.vertexNeighbors = new Cell[8];
            return result;
        } else {
            return new Cell();
        }
        return new Cell();
    }

    public void Deallocate() {
        OctreeStaticDebug.cellCount -= 1;
        transforms.Clear();
        neighbors.Clear();
        cardinalNeighbors = null;
        edgeNeighbors = null;
        vertexNeighbors = null;
        parent = null;
        onSubdivide.RemoveAllListeners();
        onMerge.RemoveAllListeners();
        pool.Enqueue(this);
        if (children != null) {
            for(int i = 0; i < 8; i++) {
                children[i].Deallocate();
            }
        }
        children = null;
    }

    public Cell() {
        OctreeStaticDebug.cellCount += 1;
        transforms = new List<Transform>();
        neighbors = new List<Cell>();
        cardinalNeighbors = new Cell[6];
        edgeNeighbors = new Cell[12];
        vertexNeighbors = new Cell[8];
    }

    #region Neighbors
    public void GetNeighbors() {
        /*GetCardinalNeighbors();
        GetEdgeNeighbors();
        GetVertexNeighbors();*/
    }

    private void GetCardinalNeighbors() {
        if (cardinalNeighbors == null) { cardinalNeighbors = new Cell[6]; }
        cardinalNeighbors[(int)Cardinal.N] = GetCardinalNeighbor(Cardinal.N);
        cardinalNeighbors[(int)Cardinal.S] = GetCardinalNeighbor(Cardinal.S);
        cardinalNeighbors[(int)Cardinal.E] = GetCardinalNeighbor(Cardinal.E);
        cardinalNeighbors[(int)Cardinal.W] = GetCardinalNeighbor(Cardinal.W);
        cardinalNeighbors[(int)Cardinal.U] = GetCardinalNeighbor(Cardinal.U);
        cardinalNeighbors[(int)Cardinal.D] = GetCardinalNeighbor(Cardinal.D);
        for(int i = 0; i < 6; i++) {
            Cell neighbor = cardinalNeighbors[i];
            if(neighbor != null) {
                neighbor.onSubdivide.AddListener(GetNeighbors);
                neighbor.onMerge.AddListener(GetNeighbors);
            }
        }

    }

    private Cell GetCardinalNeighbor(Cardinal target) {
        Stack<Vertex> directionMemoryStack = new Stack<Vertex>();
        Cell cell = this;
        while (cell != null) { //roll up the tree until cell is root or cell is opposite child of the direction we're seeking
            if (vertexComponents[cell.vertexInParent].Contains(oppositeCardinalDirection[target])) { //cell is opposite of child we're seeking
                directionMemoryStack.Push(vertexFromCardinals(vertexComponents[cell.vertexInParent].ConvertAll(x => (cardinalAxis[x] == cardinalAxis[target]) ? target : x)));
                cell = cell.parent;
                break;
            } else {
                directionMemoryStack.Push(vertexFromCardinals(vertexComponents[cell.vertexInParent].ConvertAll(x => (cardinalAxis[x] == cardinalAxis[target]) ? oppositeCardinalDirection[target] : x)));
                cell = cell.parent;
            }
        }
        while (cell != null && cell.children != null && cell.depth < this.depth) {
            Vertex directionMemory = directionMemoryStack.Pop();
            cell = cell.children[(int)directionMemory];
        }
        //cell.neighbors.Add(this);
        return cell;

    }

    private void GetEdgeNeighbors() {
        if (edgeNeighbors == null) { edgeNeighbors = new Cell[12]; }
        edgeNeighbors[(int)Edge.NU] = GetEdgeNeighbor(Edge.NU, Cardinal.U, Cardinal.N);
        edgeNeighbors[(int)Edge.EU] = GetEdgeNeighbor(Edge.EU, Cardinal.U, Cardinal.E);
        edgeNeighbors[(int)Edge.SU] = GetEdgeNeighbor(Edge.SU, Cardinal.U, Cardinal.S);
        edgeNeighbors[(int)Edge.WU] = GetEdgeNeighbor(Edge.WU, Cardinal.U, Cardinal.W);
        edgeNeighbors[(int)Edge.ND] = GetEdgeNeighbor(Edge.ND, Cardinal.D, Cardinal.N);
        edgeNeighbors[(int)Edge.ED] = GetEdgeNeighbor(Edge.ED, Cardinal.D, Cardinal.E);
        edgeNeighbors[(int)Edge.SD] = GetEdgeNeighbor(Edge.SD, Cardinal.D, Cardinal.S);
        edgeNeighbors[(int)Edge.WD] = GetEdgeNeighbor(Edge.WD, Cardinal.D, Cardinal.W);
        edgeNeighbors[(int)Edge.NE] = GetEdgeNeighbor(Edge.NE, Cardinal.E, Cardinal.N);
        edgeNeighbors[(int)Edge.SE] = GetEdgeNeighbor(Edge.SE, Cardinal.E, Cardinal.S);
        edgeNeighbors[(int)Edge.NW] = GetEdgeNeighbor(Edge.NW, Cardinal.W, Cardinal.N);
        edgeNeighbors[(int)Edge.SW] = GetEdgeNeighbor(Edge.SW, Cardinal.W, Cardinal.S);
        for(int i = 0; i < 12; i++) {
            Cell neighbor = edgeNeighbors[i];
            if (neighbor != null) {
                neighbor.onSubdivide.AddListener(GetNeighbors);
                neighbor.onMerge.AddListener(GetNeighbors);
            }
        }
        //neighbors.AddRange(edgeNeighbors);
    }

    private Cell GetEdgeNeighbor(Edge edge, Cardinal neighborDirection, Cardinal searchDirection) {
        Cell neighbor = cardinalNeighbors[(int)neighborDirection];
        if(neighbor == null || cardinalNeighbors[(int)searchDirection] == null) {
            return null; //boundary
        } else {
            Vector3 testPoint = this.center + directionVectors_Edge[edge] * radius * 2;
            Cell result;
            if (neighbor.InBounds(testPoint)) {
                result = neighbor;
            } else {
                if(neighbor.cardinalNeighbors == null) {
                    neighbor.GetCardinalNeighbors();
                }
                result = neighbor.cardinalNeighbors[(int)searchDirection];
            }
            //result.neighbors.Add(this);
            return result;
        }
    }

    private void GetVertexNeighbors() {

    }

    private void GetVertexNeighbor(Vertex vertex) {

    }

    public List<Cell> Neighbors() {
        List<Cell> result = new List<Cell>();
        result.AddRange(cardinalNeighbors);
        result.AddRange(edgeNeighbors);
        result.AddRange(vertexNeighbors);
        return result;
    }
    #endregion

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
            for(int i = 0; i < 8; i++) {
                Vertex vertex = (Vertex)i;
                Cell child = MakeCell();
                //Cell child = new Cell();
                child.vertexInParent = vertex;
                child.center = center + directionVectors_Vertex[vertex] * 0.5f * radius;
                child.radius = 0.5f * radius;
                child.octree = octree;
                child.parent = this;
                child.depth = depth + 1;
                for(int j = 0; j < transforms.Count; j++) {
                    Transform t = transforms[j];
                    if (child.InBounds(t.position)) {
                        child.transforms.Add(t);
                        octree.cellByTransform[t] = child;
                        continue;
                    }
                }
                children[i] = child;
            }
            transforms.Clear();
            for (int i = 0; i < 8; i++) {
                children[i].GetNeighbors();
                if (children[i].ShouldSubdivide()) {
                    children[i].Subdivide();
                }
            }
            if(onSubdivide != null) {
                onSubdivide.Invoke();
            }
        }
    }

    private bool ShouldMergeChildren() {
        //return depth >= octree.maxDepth;
        if (children == null) {
            return false;
        }
        int childSum = 0;
        for(int i = 0; i < 8; i++) { 
            childSum += children[i].transforms.Count;
            if (childSum > octree.maxParticlesPerCell || children[i].children != null) {
                return false;
            }
        }
        //print("Should merge - only " + childSum + " in children");
        return true;
    }

    private void MergeChildren() {
        for(int i = 0; i < 8; i++) {
            foreach (Transform t in children[i].transforms) {
                transforms.Add(t);
                continue;
            }
            if(children[i].onMerge != null) {
                children[i].onMerge.Invoke();
            }
            children[i].Deallocate();
        }
        //print("Successfully merged with " + transforms.Count);
        children = null;
        if (parent != null && parent.ShouldMergeChildren()) {
            parent.MergeChildren();
        }
    }

    private void DistributeRecursive(Transform t) {
        for(int i = 0; i < 8; i++) {
            if (children[i].InBounds(t.position)) {
                if (children[i].children != null) {
                    children[i].DistributeRecursive(t);
                } else {
                    children[i].transforms.Add(t);
                    octree.cellByTransform[t] = children[i];
                }
            }
        }
    }

    public void UpdateBoundsRecursive(Vector3 center, float radius) {
        this.center = center;
        this.radius = radius;
        if (children != null) {
            for(int i = 0; i < 8; i++) {
                children[i].UpdateBoundsRecursive(center + 0.5f * radius * directionVectors_Vertex[(Vertex)i], 0.5f * radius);
            }
        }
    }

    public void RedistributeRecursive() {
        if (children != null) {
            if (transforms.Count > 0) {
                foreach (Transform t in transforms) {
                    DistributeRecursive(t);
                }
                transforms.Clear();
            }
            for(int i = 0; i < 8; i++) {
                children[i].RedistributeRecursive();
            }
        } else {
            for (int i = transforms.Count - 1; i >= 0; i--) {
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
            for(int i = 0; i < 8; i++){
                children[i].ResizeRecursive();
            }
        } else {
            //print("Resizing");
            if (ShouldSubdivide()) {
                Subdivide();
            } else if (parent != null && parent.children != null && parent.children[0] == this && parent.ShouldMergeChildren()) {
                //print("Checking merger");
                parent.MergeChildren();
            }
        }
    }

    public IEnumerable<Cell> GetChildrenAndSelf() {
        if (children != null) {
            for(int i = 0; i < 8; i++) {
                foreach (Cell cell in children[i].GetChildrenAndSelf()) {
                    //yield return cell;
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
            for(int i = 0; i < 8; i++) {
                children[i].GatherDebugData();
            }
        }
    }
}
