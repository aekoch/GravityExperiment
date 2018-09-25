using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeRenderer : MonoBehaviour {
    public static OctreeRenderer instance = null;

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

    public Material material;
    public List<Octree> octrees = new List<Octree>();

    private void Awake() {
        if(instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void RegisterOctree(Octree octree) {
        octrees.Add(octree);
    }

    public void UnregisterOctree(Octree octree) {
        if (octrees.Contains(octree)) {
            octrees.Remove(octree);
        } else {
            Debug.LogWarning("Tried to remove non-existant octree from renderer");
        }
        
    }

    private Material mat;

    // Will be called from camera after regular rendering is done.
    /*public void OnPostRender() {
        if (!mat) {
            mat = material;
        }

        GL.PushMatrix();
        GL.LoadIdentity();
        GL.LoadProjectionMatrix(Camera.main.projectionMatrix);
        GL.MultMatrix(Camera.main.cameraToWorldMatrix);

        // activate the first shader pass (in this case we know it is the only pass)
        mat.SetPass(0);
        // draw a quad over whole screen
        GL.Begin(GL.LINES);
        foreach(Octree octree in octrees) {
            foreach (Vector3 v in new RenderCube(octree.center, octree.boundingRadius).lineVertices) {
                GL.Vertex(v);
            }
        }
        GL.End();

        GL.PopMatrix();
    }*/
}
