using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    private Text loadingText;

    public static SceneChanger Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        loadingText = GameObject.Find("LoadingText").GetComponent<Text>();
    }

    public void ChangeScene(int sceneBuildIndex = -1)
    {
        if (loadingText)
            loadingText.enabled = true;

        SceneManager.LoadScene(
            sceneBuildIndex != -1 ? sceneBuildIndex : SceneManager.GetActiveScene().buildIndex
        );
    }
}
