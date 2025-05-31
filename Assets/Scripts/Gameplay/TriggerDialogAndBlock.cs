using System.Collections;
using UnityEngine;

public class TriggerDialogAndBlock : MonoBehaviour
{
    [SerializeField] private string dialogueKey;
    [SerializeField] private float delay = 2f;
    [SerializeField] private bool waitBefore = true;

    [SerializeField] private GameObject nextDialogEvant;

    private IEnumerator DialogWaitAndStartNew()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        
        while (DialogSystem.Instance.inDialogue)
            yield return new WaitForSeconds(0.01f);

        bool cheak = false;
        if (!DialogSystem.Instance.inDialogue)
            cheak = DialogSystem.Instance.StartDialogue(dialogueKey, delay);

        if (!cheak) yield break;

        StartCoroutine(DialogWait());
    }
    private IEnumerator DialogWait()
    {
        while (DialogSystem.Instance.inDialogue)
            yield return new WaitForSeconds(0.01f);

        gameObject.SetActive(false);

        yield return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (waitBefore)
        {
            StartCoroutine(DialogWaitAndStartNew());
            if (nextDialogEvant) nextDialogEvant.SetActive(true);
            return;
        }

        bool cheak = false;
        if (!DialogSystem.Instance.inDialogue)
            cheak = DialogSystem.Instance.StartDialogue(dialogueKey, delay);

        if (!cheak) return;

        gameObject.GetComponent<BoxCollider>().enabled = false;

        StartCoroutine(DialogWait());

        if (nextDialogEvant) nextDialogEvant.SetActive(true);
    }
}
