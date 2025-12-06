using UnityEngine;
using UnityEngine.Events;

public class Timer : MonoBehaviour
{
    public bool autoStart {get;private set;} = false;
    public bool oneShot {get; private set;} = true;
    public bool paused {get; private set;} = true;
    public float timeLeft {get; private set;}
    public float waitTime {get; private set;}
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

    public void SetWaitTime(float _waitTime)
    {
        waitTime = _waitTime;
    }

    public void SetRandomWaitTime(Vector2 _waitTime)
    {
        waitTime = Random.Range(_waitTime.x, _waitTime.y);
    }
}