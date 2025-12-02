using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    [SerializeField] bool autoStart = true;
    [SerializeField] bool oneShot = true;
    public bool paused { get; private set; }
    public float timeLeft { get; private set; }
    public float waitTime { get; private set; }
    public UnityEvent whenTimeout;

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
            Stop();
            whenTimeout?.Invoke();
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

    public void SetWaitTime(float v)
    {
        if (v < 0)
            return;
        waitTime = v;
    }
}