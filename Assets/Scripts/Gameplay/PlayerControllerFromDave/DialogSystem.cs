using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogueElement
{
    [Tooltip("Character Name.")]
    public string characterName;

    [Tooltip("Character Avatar.")]
    public Sprite characterTexture;

    [TextArea(3, 10)]
    [Tooltip("Dialogue Text.")]
    public string dialogueText;

    [Tooltip("Voiceover.")]
    public AudioClip dialogueClip;

    [Tooltip("Stopping on audio.")]
    public bool stopAudio = false;

    [Tooltip("Sound when displaying letters.")]
    public bool useLetterSound = true;

    [Tooltip("DefaultDelayLetters.")]
    public float defaultDelayLetters = 0.07f;

    [Tooltip("Delay.")]
    public float delay = 0f;
}

[Serializable]
public class Dialogue
{
    [Tooltip("Key Name.")]
    public string key;

    [Tooltip("Dialogue.")]
    public List<DialogueElement> dialogue = new List<DialogueElement>();
}

public class DialogSystem : MonoBehaviour
{
    [SerializeField] private DialogueData dialogueData;

    [HideInInspector]
    public bool inDialogue = false;

    [SerializeField] private AudioSource audioSourceLetters;
    [SerializeField] private AudioClip audioClipLetters;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject dialogueWindow;
    [SerializeField] private Text textObj;
    [SerializeField] private Image imgObj;
    [SerializeField] private Text nameObj;

    public static DialogSystem Instance { get; private set; }

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
        dialogueWindow.SetActive(false);
    }

    private List<DialogueElement> GetDialogueElements(string key)
    {
        foreach (Dialogue dialogue in dialogueData.dialogues)
        {
            if (dialogue.key != key) continue;

            return dialogue.dialogue;
        }

        return new List<DialogueElement>();
    }

    private IEnumerator _StartDialogue(List<DialogueElement> dialogueElements, float delay = 0f)
    {
        if (dialogueElements.Count == 0) yield break;
        if (inDialogue) yield break;

        inDialogue = true;
        yield return new WaitForSeconds(delay);
        dialogueWindow.SetActive(true);

        foreach (DialogueElement dialogueElement in dialogueElements)
        {
            textObj.text = "";
            imgObj.sprite = dialogueElement.characterTexture;
            nameObj.text = dialogueElement.characterName;

            float delayLetters;
            if (dialogueElement.dialogueClip != null)
            {
                delayLetters = !dialogueElement.stopAudio ?
                    dialogueElement.dialogueClip.length / dialogueElement.dialogueText.Length :
                    dialogueElement.defaultDelayLetters;
                audioSource.clip = dialogueElement.dialogueClip;
                audioSource.Play();
            }
            else
            {
                delayLetters = dialogueElement.defaultDelayLetters;
            }

            foreach (char c in dialogueElement.dialogueText)
            {
                textObj.text += c;

                if (dialogueElement.useLetterSound)
                    audioSourceLetters.PlayOneShot(audioClipLetters);

                if (dialogueElement.stopAudio && !audioSource.isPlaying)
                    break;

                yield return new WaitForSeconds(delayLetters);
            }

            while (dialogueElement.stopAudio && audioSource.isPlaying)
                yield return new WaitForSeconds(0.01f);

            audioSource.Stop();

            yield return new WaitForSeconds(dialogueElement.delay);
        }

        inDialogue = false;
        dialogueWindow.SetActive(false);
    }

    public bool StartDialogue(string dialogueKey, float delay = 0f)
    {
        List<DialogueElement> dialogueElements = GetDialogueElements(dialogueKey);
        if (dialogueElements.Count == 0) return false;
        if (inDialogue) return false;

        StartCoroutine(_StartDialogue(dialogueElements, delay));
        return true;
    }
}