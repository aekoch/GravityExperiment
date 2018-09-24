using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour {

    public Text octreeCellCountText;

	// Use this for initialization
	void Start () {
        StartCoroutine(updateDebugInfo(0.5f));
	}

    private IEnumerator updateDebugInfo(float delay) {
        while (true) {
            octreeCellCountText.text = "Octree Cells: " + OctreeStaticDebug.cellCount.ToString();
            yield return new WaitForSeconds(delay);
        }
    }
}
