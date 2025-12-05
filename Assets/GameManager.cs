using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance {get; private set;}
    public float playTime;
    public bool gameInProgress = true;
    public GameObject gameOverCanvas;
    public GameObject escapedCanvas;
    public SoundController sound;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (gameInProgress)
        {
            playTime += Time.deltaTime;
        }
    }

    public void PlayerDied()
    {
        gameOverCanvas.SetActive(true);
        gameInProgress = false;
        Time.timeScale = 0f;
    }

    public void PlayerEscaped()
    {
        sound.VictoryAnt();
        escapedCanvas.SetActive(true);
        gameInProgress = false;
        Time.timeScale = 0f;
    }
}
