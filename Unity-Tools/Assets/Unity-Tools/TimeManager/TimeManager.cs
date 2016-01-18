using UnityEngine;
using System.Collections.Generic;

public class TimeManager {
    public static TimeManager GameplayTime = new TimeManager();
    public static TimeManager UITime = new TimeManager();

    public float deltaTime
    {
        get
        {
            if (this.isPaused)
            {
                return 0f;
            }
            else
            {
                return Time.deltaTime * this.timeScale;
            }
        }
    }

    public float fixedDeltaTime
    {
        get
        {
            if (this.isPaused)
            {
                return 0f;
            }
            else
            {
                return Time.fixedDeltaTime * this.timeScale;
            }
        }
    }

    float _timeScale = 1.0f;
    public float timeScale
    {
        get { return _timeScale; }
        set { _timeScale = value; }
    }

    bool _isPaused;
    public bool isPaused
    {
        get { return this._isPaused; }
        set { this._isPaused = value; }
    }
}
