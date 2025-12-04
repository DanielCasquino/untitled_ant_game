using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public bool autoStart = false;
    public bool oneShot = true;
    public bool paused = true;
    public float timeLeft;
    public float waitTime;
    public UnityEvent whenTimeout;

    public void Initialize(bool autoStart = false, float waitTime = 1f)
    {
        paused = true;
        this.autoStart = autoStart;
        this.waitTime = waitTime;
        timeLeft = waitTime;
    }

    void Start()
    {
        if (autoStart)
            Play();
    }

    void Update()
    {
        if (paused)
            return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            whenTimeout?.Invoke();
            if (oneShot)
                Stop();
            else
                Play();
        }
    }

    public void Play()
    {
        timeLeft = waitTime;
        paused = false;
    }

    public void Pause()
    {
        paused = true;
    }

    public void Stop()
    {
        timeLeft = 0;
        paused = true;
    }
}