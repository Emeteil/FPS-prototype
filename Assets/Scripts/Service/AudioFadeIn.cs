using System.Collections;
using UnityEngine;

public class AudioFadeIn : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float startDelay = 2.0f;
    [SerializeField] private float fadeDuration = 5.0f;
    [SerializeField] private float targetVolume = 1.0f;

    private bool hasStarted = false;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                Debug.LogError("AudioSource component not found. Please attach an AudioSource component to the GameObject or set it in the Inspector.");
                enabled = false;
                return;
            }
        }

        StartCoroutine(FadeInAudio());
    }

    IEnumerator FadeInAudio()
    {
        yield return new WaitForSeconds(startDelay);
        
        audioSource.Play();

        float currentTime = 0.0f;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            float currentVolume = Mathf.Clamp01(currentTime / fadeDuration) * targetVolume;
            audioSource.volume = currentVolume;
            yield return null;
        }

        audioSource.volume = targetVolume;
        hasStarted = false;
    }
}
