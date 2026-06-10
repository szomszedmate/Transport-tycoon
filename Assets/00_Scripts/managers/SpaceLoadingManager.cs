using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceLoadingManager : MonoBehaviour
{
    public ParticleSystem[] particles;

    private static string targetScene;

    public static void LoadScene(string sceneName)
    {
        targetScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    void Start()
    {
        StartCoroutine(LoadAsync());
    }

    IEnumerator LoadAsync()
    {
        yield return new WaitForSeconds(0.5f);

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        yield return new WaitForSeconds(3.5f);
        op.allowSceneActivation = true;
    }
}
