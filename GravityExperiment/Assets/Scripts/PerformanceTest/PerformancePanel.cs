using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PerformancePanel : MonoBehaviour {

    public Text frameRateText;
    public InputField countInput;

    public PerformanceGPU performanceTest;

    private float frameRate = 0;

    private void Start() {
        StartCoroutine(updateUI(0.3f));
        countInput.text = performanceTest.n.ToString();
        countInput.onValueChanged.AddListener(ListenCountInput);
    }

    // Update is called once per frame
    void Update () {
        frameRate = 1 / Time.deltaTime;
	}

    private void ListenCountInput(string strInput) {
        int.TryParse(strInput, out performanceTest.n);
    }

    private IEnumerator updateUI(float delay) {
        while (true) {
            frameRateText.text = "Frame Rate: " + frameRate.ToString("0.0");
            yield return new WaitForSeconds(delay);
        }
    }
}
