using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
    [SerializeField] private int scene = 0;
    [SerializeField] private bool currentScene = true;
    [SerializeField] private bool unPaused = true;

    [SerializeField] private Pause pause;
    private Text loadingText;

    private void Start()
    {
        loadingText = GameObject.Find("LoadingText").GetComponent<Text>();
    }

    public void ChangeScene()
    {
        if (unPaused) pause.PauseGame(false);

        loadingText.enabled = true;

        if (currentScene)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }
        SceneManager.LoadScene(scene);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
