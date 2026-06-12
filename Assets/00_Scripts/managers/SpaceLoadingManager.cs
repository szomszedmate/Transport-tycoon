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

    [SerializeField] private float loadStartDelay = 0.5f;
    [SerializeField] private float minDisplayTime = 3.5f;
    private const float AsyncLoadCompleteThreshold = 0.9f; // Unity stops at 0.9 when allowSceneActivation = false

    IEnumerator LoadAsync()
    {
        yield return new WaitForSeconds(loadStartDelay);

        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        while (op.progress < AsyncLoadCompleteThreshold)
            yield return null;

        yield return new WaitForSeconds(minDisplayTime);
        op.allowSceneActivation = true;
    }
}
