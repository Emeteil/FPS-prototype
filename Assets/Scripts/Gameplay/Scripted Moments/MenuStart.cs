using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

public class MenuStart : MonoBehaviour
{
    [SerializeField] private AudioClip buttonAudio;
    
    [SerializeField] private GameObject videoPlayerObj;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string clip;
    
    [SerializeField] private int scene = 0;
    [SerializeField] private float timeDelay = 0.2f;
    [SerializeField] private GameObject dimmerButtons;
    [SerializeField] private float dimmerButtonsDelay = 3.0f;

    [SerializeField] private AudioSource audioSourceBackground;
    
    [SerializeField] private GameObject menu;

    private AudioSource audioSource;
    private bool play = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private IEnumerator StarterVolume(float duration)
    {
        float startVolume = audioSourceBackground.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            audioSourceBackground.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        audioSourceBackground.volume = 0f;
    }
    private IEnumerator Starter(Image image, float duration, bool fadeIn)
    {
        Color color = image.color;
        float startAlpha = color.a;
        float endAlpha = fadeIn ? 1f : 0f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            image.color = color;
            yield return null;
        }

        color.a = endAlpha;
        image.color = color;
        menu.SetActive(false);
        dimmerButtons.SetActive(false);

        yield return new WaitForSeconds(timeDelay);
        
        videoPlayerObj.SetActive(true);
        videoPlayer.url = Path.Combine(Application.streamingAssetsPath, clip);
        videoPlayer.Play();
        
        yield return new WaitForSeconds(1f);

        while (videoPlayer.isPlaying)
            yield return new WaitForSeconds(0.1f);

        SceneChanger.Instance.ChangeScene(scene);
    }

    public void PlayGame()
    {
        if (play) return;
        play = true;

        if (audioSource && buttonAudio)
            audioSource.PlayOneShot(buttonAudio);

        StartCoroutine(Starter(dimmerButtons.GetComponent<Image>(), dimmerButtonsDelay, true));
        StartCoroutine(StarterVolume(dimmerButtonsDelay + timeDelay));
    }
}
