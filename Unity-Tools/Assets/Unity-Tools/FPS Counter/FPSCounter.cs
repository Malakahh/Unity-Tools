using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour {
    public static FPSCounter Instance;

    public float UpdateFrequency = 4.0f;
    public float FPS { get; private set; }
    
    float dt = .0f;
    float frameCount;
    float inverseUpdateFrequency = 0.0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

	void Update () {
        ++frameCount;
        dt += Time.deltaTime;
        inverseUpdateFrequency = 1.0f / UpdateFrequency;
        
        if (dt > inverseUpdateFrequency)
        {
            FPS = frameCount / inverseUpdateFrequency;
            frameCount = 0;
            dt -= inverseUpdateFrequency;
        }
	}
}
