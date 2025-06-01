using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogueElement
{
    [Tooltip("Имя персонажа.")]
    public string characterName;

    [Tooltip("Аватар персонажа.")]
    public Sprite characterTexture;

    [TextArea(3, 10)]
    [Tooltip("Текст диалога.")]
    public string dialogueText;

    [Tooltip("Голос.")]
    public AudioClip dialogueClip;

    [Tooltip("Остановка на звуке.")]
    public bool stopAudio = false;

    [Tooltip("Звук при отображении букв.")]
    public bool useLetterSound = true;

    [Tooltip("Задержка при отображении букв.")]
    public float defaultDelayLetters = 0.07f;

    [Tooltip("Задержка.")]
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

    private Queue<List<DialogueElement>> dialogueQueue = new Queue<List<DialogueElement>>();
    private Coroutine currentDialogueCoroutine;

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

    private IEnumerator ProcessDialogueQueue()
    {
        while (dialogueQueue.Count > 0)
        {
            List<DialogueElement> dialogueElements = dialogueQueue.Dequeue();
            yield return StartCoroutine(PlayDialogue(dialogueElements));
        }
    }

    private IEnumerator PlayDialogue(List<DialogueElement> dialogueElements, float delay = 0f)
    {
        if (dialogueElements.Count == 0) yield break;

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

    public bool StartDialogue(string dialogueKey, float delay = 0f, bool isPriority = false) =>
        AddDialogueToQueue(dialogueKey, delay, isPriority);

    public bool AddDialogueToQueue(string dialogueKey, float delay = 0f, bool isPriority = false)
    {
        List<DialogueElement> dialogueElements = GetDialogueElements(dialogueKey);
        if (dialogueElements.Count == 0) return false;

        if (isPriority)
        {
            StopAllCoroutines();
            dialogueQueue.Clear();
            currentDialogueCoroutine = StartCoroutine(PlayDialogue(dialogueElements, delay));
        }
        else
        {
            dialogueQueue.Enqueue(dialogueElements);
            if (!inDialogue && currentDialogueCoroutine == null)
            {
                currentDialogueCoroutine = StartCoroutine(ProcessDialogueQueue());
            }
        }

        return true;
    }

    public void ClearDialogueQueue()
    {
        StopAllCoroutines();
        dialogueQueue.Clear();
        inDialogue = false;
        dialogueWindow.SetActive(false);
        currentDialogueCoroutine = null;
    }
}