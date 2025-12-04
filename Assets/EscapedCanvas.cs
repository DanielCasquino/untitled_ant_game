using System.Collections;
using TMPro;
using UnityEngine;

public class EscapedCanvas : MonoBehaviour
{
    public TextMeshProUGUI timeSurvived;
    public Animator panelAnimator;

    public void OnEnable()
    {
        timeSurvived.text = "Time Played: " + GameManager.instance.playTime.ToString("F2") + "s";
    }

    public void RestartGame()
    {
        StartCoroutine(RestartCoroutine());
    }

    public IEnumerator RestartCoroutine()
    {
        panelAnimator.SetTrigger("fadeOut");
    yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }

    public void QuitToMainMenu()
    {
        StartCoroutine(QuitToMainMenuCoroutine());
    }

    public IEnumerator QuitToMainMenuCoroutine()
    {
        panelAnimator.SetTrigger("fadeOut");
    yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
