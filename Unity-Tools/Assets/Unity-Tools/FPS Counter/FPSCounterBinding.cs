using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FPSCounterBinding : MonoBehaviour {
    public Text TextElement;

	// Update is called once per frame
	void Update () {
        TextElement.text = "FPS: " + FPSCounter.Instance.FPS;
	}
}
