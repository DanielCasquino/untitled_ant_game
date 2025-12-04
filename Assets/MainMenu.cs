using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject fader;

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Play()
    {
        StartCoroutine(LoadMainSceneAsync());
    }

    private System.Collections.IEnumerator LoadMainSceneAsync()
    {
        fader.GetComponent<Animator>().SetTrigger("fadeOut");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime(1f);

        asyncLoad.allowSceneActivation = true;
    }
}