using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Cell {
    #region Directions
    public const int N_CARDINAL = 6;
    public const int N_EDGE = 12;
    public const int N_VERTEX = 8;
    public enum Axis { X, Y, Z }
    public enum Cardinal { N, E, S, W, U, D }
    public enum Edge { NU, EU, SU, WU, ND, ED, SD, WD, NE, NW, SE, SW }
    public enum Vertex { NEU, NWU, SEU, SWU, NED, NWD, SED, SWD }
    private static readonly Dictionary<int, Axis> cardinalIntToAxis = new Dictionary<int, Axis>() {
        { (int)Cardinal.N, Axis.Z },
        { (int)Cardinal.S, Axis.Z },
        { (int)Cardinal.E, Axis.X },
        { (int)Cardinal.W, Axis.X },
        { (int)Cardinal.U, Axis.Y },
        { (int)Cardinal.D, Axis.Y }
    };
    private static readonly Dictionary<Axis, List<Cardinal>> axisCardinals = new Dictionary<Axis, List<Cardinal>>() {
        { Axis.X, new List<Cardinal>( new Cardinal[]{ Cardinal.E, Cardinal.W }) },
        { Axis.Y, new List<Cardinal>( new Cardinal[]{ Cardinal.U, Cardinal.D }) },
        { Axis.Z, new List<Cardinal>( new Cardinal[]{ Cardinal.N, Cardinal.S }) },
    };
    private static readonly Dictionary<int, Cardinal> cardinalIntToOpposite = new Dictionary<int, Cardinal>() {
            { (int)Cardinal.N, Cardinal.S },
            { (int)Cardinal.S, Cardinal.N },
            { (int)Cardinal.E, Cardinal.W },
            { (int)Cardinal.W, Cardinal.E },
            { (int)Cardinal.U, Cardinal.D },
            { (int)Cardinal.D, Cardinal.U },
        };
    private static readonly Dictionary<int, List<Cardinal>> vertexIntToCardinalComponents = new Dictionary<int, List<Cardinal>>() {
        { (int)Vertex.NEU, new List<Cardinal>(new Cardinal[] { Cardinal.N, Cardinal.E, Cardinal.U}) },
        { (int)Vertex.NWU, new List<Cardinal>(new Cardinal[] { Cardinal.N, Cardinal.W, Cardinal.U}) },
        { (int)Vertex.SEU, new List<Cardinal>(new Cardinal[] { Cardinal.S, Cardinal.E, Cardinal.U}) },
        { (int)Vertex.SWU, new List<Cardinal>(new Cardinal[] { Cardinal.S, Cardinal.W, Cardinal.U}) },
        { (int)Vertex.NED, new List<Cardinal>(new Cardinal[] { Cardinal.N, Cardinal.E, Cardinal.D}) },
        { (int)Vertex.NWD, new List<Cardinal>(new Cardinal[] { Cardinal.N, Cardinal.W, Cardinal.D}) },
        { (int)Vertex.SED, new List<Cardinal>(new Cardinal[] { Cardinal.S, Cardinal.E, Cardinal.D}) },
        {(int) Vertex.SWD, new List<Cardinal>(new Cardinal[] { Cardinal.S, Cardinal.W, Cardinal.D}) },
    };
    public static readonly Dictionary<int, int[]> edgeIntToCardinalComponentInts = new Dictionary<int, int[]>() {
        { (int)Edge.NU, new int[]{ (int)Cardinal.N, (int)Cardinal.U } },
        { (int)Edge.EU, new int[]{ (int)Cardinal.E, (int)Cardinal.U } },
        { (int)Edge.SU, new int[]{ (int)Cardinal.S, (int)Cardinal.U } },
        { (int)Edge.WU, new int[]{ (int)Cardinal.W, (int)Cardinal.U } },
        { (int)Edge.ND, new int[]{ (int)Cardinal.N, (int)Cardinal.D } },
        { (int)Edge.ED, new int[]{ (int)Cardinal.E, (int)Cardinal.D } },
        { (int)Edge.SD, new int[]{ (int)Cardinal.S, (int)Cardinal.D } },
        { (int)Edge.WD, new int[]{ (int)Cardinal.W, (int)Cardinal.D } },
        { (int)Edge.NE, new int[]{ (int)Cardinal.N, (int)Cardinal.E } },
        { (int)Edge.NW, new int[]{ (int)Cardinal.N, (int)Cardinal.W } },
        { (int)Edge.SE, new int[]{ (int)Cardinal.S, (int)Cardinal.E } },
        { (int)Edge.SW, new int[]{ (int)Cardinal.S, (int)Cardinal.W } },
    };
    public static readonly Dictionary<int, int[]> edgeIntToVertexComponentInts = new Dictionary<int, int[]>() {
        { (int)Edge.NU, new int[]{ (int)Vertex.NEU, (int)Vertex.NWU } },
        { (int)Edge.EU, new int[]{ (int)Vertex.NEU, (int)Vertex.SEU } },
        { (int)Edge.SU, new int[]{ (int)Vertex.SEU, (int)Vertex.SWU } },
        { (int)Edge.WU, new int[]{ (int)Vertex.NWU, (int)Vertex.SWU } },
        { (int)Edge.ND, new int[]{ (int)Vertex.NED, (int)Vertex.NWD } },
        { (int)Edge.ED, new int[]{ (int)Vertex.NED, (int)Vertex.SED } },
        { (int)Edge.SD, new int[]{ (int)Vertex.SED, (int)Vertex.SWD } },
        { (int)Edge.WD, new int[]{ (int)Vertex.NWD, (int)Vertex.SWD } },
        { (int)Edge.NE, new int[]{ (int)Vertex.NEU, (int)Vertex.NED } },
        { (int)Edge.NW, new int[]{ (int)Vertex.NWU, (int)Vertex.NWD } },
        { (int)Edge.SE, new int[]{ (int)Vertex.SEU, (int)Vertex.SED } },
        { (int)Edge.SW, new int[]{ (int)Vertex.SWU, (int)Vertex.SWD } },
    };
    public static readonly Dictionary<int, Vector3> vertexIntToDirectionVector = new Dictionary<int, Vector3> {
            { (int)Vertex.NEU, new Vector3(1, 1, 1) },
            { (int)Vertex.NWU, new Vector3(-1, 1, 1) },
            { (int)Vertex.SEU, new Vector3(1, 1, -1) },
            { (int)Vertex.SWU, new Vector3(-1, 1, -1) },
            { (int)Vertex.NED, new Vector3(1, -1, 1) },
            { (int)Vertex.NWD, new Vector3(-1, -1, 1) },
            { (int)Vertex.SED, new Vector3(1, -1, -1) },
            { (int)Vertex.SWD, new Vector3(-1, -1, -1) },
        };
    public static readonly Dictionary<int, Vector3> cardinalIntToDirectionVector = new Dictionary<int, Vector3>() {
            { (int)Cardinal.N, Vector3.forward },
            { (int)Cardinal.S, Vector3.back },
            { (int)Cardinal.E, Vector3.right },
            { (int)Cardinal.W, Vector3.left },
            { (int)Cardinal.U, Vector3.up },
            { (int)Cardinal.D, Vector3.down },
        };
    public static readonly Dictionary<int, Vector3> edgeIntToDirectionVector = new Dictionary<int, Vector3>() {
            { (int)Edge.NU, new Vector3(0, 1, 1) },
            { (int)Edge.EU, new Vector3(1, 0, 1) },
            { (int)Edge.SU, new Vector3(0, -1, 1) },
            { (int)Edge.WU, new Vector3(-1, 0, 1) },
            { (int)Edge.ND, new Vector3(0, 1, -1) },
            { (int)Edge.ED, new Vector3(1, 0, -1) },
            { (int)Edge.SD, new Vector3(0, -1, -1) },
            { (int)Edge.WD, new Vector3(-1, 0, -1) },
            { (int)Edge.NE, new Vector3(1, 1, 0) },
            { (int)Edge.NW, new Vector3(-1, 1, 0) },
            { (int)Edge.SE, new Vector3(1, -1, 0) },
            { (int)Edge.SW, new Vector3(-1, -1, 0) }
        };
    private Vertex vertexFromCardinals(Cardinal cardinal1, Cardinal cardinal2, Cardinal cardinal3) {
        for(int i = 0; i < N_VERTEX; i++) {
            if (vertexIntToCardinalComponents[i].Contains(cardinal1)
            && vertexIntToCardinalComponents[i].Contains(cardinal2)
            && vertexIntToCardinalComponents[i].Contains(cardinal3)) {
                return (Vertex)i;
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

    private bool isRoot = false;
    private bool isStem = false;
    private bool isLeaf = false;

    #region Instance Management
    private static Queue<Cell> pool;
    private static int cellInstances = 0;

    public Cell() {
        if(pool == null) { pool = new Queue<Cell>(); }
        cellInstances += 1;
        transforms = new List<Transform>();
    }

    public Cell RecycleOrCreateCell() {
        Debug.Log(pool.Count);
        if(pool.Count > 0) {
            Debug.Log("Recycled");
            return pool.Dequeue();
        } else {
            Debug.Log("Created new");
            return new Cell();
        }
    }

    public void Deallocate() {
        if(children != null) {
            for(int i = 0; i < 8; i++) {
                children[i].Deallocate();
            }
        }
        transforms.Clear();
        if(this != octree.root) {
            pool.Enqueue(this);
        }
    }
    #endregion

    private bool InBounds(Vector3 vector) {
        return vector.x >= center.x - radius && vector.x <= center.x + radius
            && vector.y >= center.y - radius && vector.y <= center.y + radius
            && vector.z >= center.z - radius && vector.z <= center.z + radius;
    }

    #region Recursive Manipulation Methods
    public void RecalculateBounds(Vector3 center, float radius) {
        this.center = center;
        this.radius = radius;
        if(children != null) {//Recursive
            for(int i = 0; i < 8; i++) {
                children[i].RecalculateBounds(center + 0.5f * radius * vertexIntToDirectionVector[i], 0.5f * radius);
            }
        }
    }

    public void MergeOrSubdivide() {
        if(children != null) {
            for(int i = 0; i < 8; i++) {
                children[i].MergeOrSubdivide();
            }
        } else {
            if (ShouldSubdivide()) {
                Subdivide();
            } else if (ShouldMerge()) {
                //Merge();
            }
        }
    }

    public void RedistributeRecursive() {
        if (children != null) {
            for (int i = 0; i < 8; i++) {
                children[i].RedistributeRecursive();
            }
        } if(transforms.Count != 0) {
            for (int i = transforms.Count - 1; i >= 0; i--) {
                Redistribute(transforms[i]);
            }
        }
    }

    #endregion

    #region Redistribution
    public void Redistribute(Transform t) {
        //Debug.Log("Distributing transform at " + t.position);
        if (InBounds(t.position) && children == null) {
            //Debug.Log(t.position + " should stay in current cell");
            return;
        } 
        else {
            Debug.Log("Transform should change cells");
            Cell cell = this;
            //Roll up
            while (cell.parent != null && !cell.InBounds(t.position)) {
                cell = cell.parent;
            }
            //Roll down
            bool shouldBreak = false;
            while (cell.children != null && !shouldBreak) {
                shouldBreak = true;
                for (int i = 0; i < 8; i++) {
                    if (cell.children[i].InBounds(t.position)) {
                        cell = cell.children[i];
                        shouldBreak = false;
                        break;
                    }
                }
                if (shouldBreak) {
                    Debug.Log("Could not distribute transform at depth " + depth + " and position " + t.position);
                }
            }
            if (cell == null) {
                Debug.DrawLine(Vector3.zero, t.position, Color.red, 1000000);
                Debug.Log("Could not redistribute transform" + t.position);
                Debug.Log(t.position.magnitude + " ??? " + octree.boundingRadius);
            }
            if (t == null) {
                Debug.Log("Transform does not exist");
            }
            this.transforms.Remove(t);
            cell.transforms.Add(t);
            octree.cellByTransform[t] = cell;
        }
        
        
    }

    public void RedistributeDown(Transform t) {
        if (children != null) {
            for(int i = 0; i < 8; i++) {
                if (children[i].InBounds(t.position)) {
                    children[i].RedistributeDown(t);
                    break;
                }
            }
        } else {
            if(octree.cellByTransform[t] != this) {
                transforms.Add(t);
                octree.cellByTransform[t].transforms.Remove(t);
                octree.cellByTransform[t] = this;
            }
        }
    }
    #endregion

    #region Subdivision
    private bool ShouldSubdivide() {
        return transforms.Count > octree.maxParticlesPerCell && depth + 1 < octree.maxDepth;
    }

    private void Subdivide() {
        if(depth > octree.maxDepth) { return; }
        children = new Cell[8];
        for (int i = 0; i < 8; i++) {
            //Cell child = RecycleOrCreateCell();
            Cell child = new Cell();
            child.octree = octree;
            child.parent = this;
            child.vertexInParent = (Vertex)i;
            child.center = center + 0.5f * radius * vertexIntToDirectionVector[i];
            child.radius = 0.5f * radius;
            child.depth = depth + 1;
            children[i] = child;
        }
        for(int i = transforms.Count-1; i >= 0; i--) {
            Redistribute(transforms[i]);
        }
        transforms.Clear();
        for(int i = 0; i < 8; i++) {
            if (children[i].ShouldSubdivide()) {
                children[i].Subdivide();
            }
        }

    }
    #endregion

    #region Merging

    private bool ShouldMerge() {
        if(vertexInParent != 0) {
            return false;
        } else {
            int siblingDataCount = 0;
            for(int i = 0; i < 8; i++) {
                siblingDataCount += parent.children[i].transforms.Count;
            }
        }
        return false;
    }

    private void Merge() {
        
    }
    #endregion

    #region Recursive Access Methods
    public IEnumerable<Cell> GetChildrenAndSelf() {
        if (children != null) {
            for (int i = 0; i < 8; i++) {
                foreach (Cell cell in children[i].GetChildrenAndSelf()) {
                    yield return cell;
                }
            }
        }
        yield return this;
    }
    #endregion

    #region Debug
    public void GatherDebugData() {
        octree.cellCount++;
        octree.cellCountAtDepth[depth]++;
        octree.dataCount += transforms.Count;
        octree.dataCountAtDepth[depth] += transforms.Count;
        if (children != null) {
            for (int i = 0; i < 8; i++) {
                children[i].GatherDebugData();
            }
        }
    }
    #endregion
}
