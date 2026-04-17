using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Xml.Serialization;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject transitionsContainer;

    private SceneTransition[] transitions;

    private void Awake()        // singleton
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()    // get scene transition components that are children of transition container
    {
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
    }

    public void LoadScene(string sceneName, string transitionName)      // public method to load scene with transition
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)     // coroutine to handle asynchronous scene loading with transition and progress bar
    {                                                                                       // asynchronous meaning - next scene loads while current scene's ui continue to run
        SceneTransition transition = transitions.First(t => t.name == transitionName);

        yield return transition.AnimateTransitionIn();

        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);      // load next scene
        scene.allowSceneActivation = false;     // pause next scene activation until ready

        progressBar.gameObject.SetActive(true);     // show progress bar

        do
        {
            progressBar.value = scene.progress;     // scene progress 0 to 0.9
            yield return null;      // wait for next frame
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;      // allow next scene to activate since loading is (almost) complete
        progressBar.gameObject.SetActive(false);    // hide progress bar

        yield return null;      // wait one frame

        yield return transition.AnimateTransitionOut();     // play transition
    }
}
