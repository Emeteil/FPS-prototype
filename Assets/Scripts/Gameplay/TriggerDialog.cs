using UnityEngine;

public class TriggerDialog : MonoBehaviour
{
    [SerializeField] private string dialogueKey;
    [SerializeField] private float delay = 0f;

    [SerializeField] private GameObject nextDialogEvant;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        bool cheak = false;
        if (!DialogSystem.Instance.inDialogue)
            cheak = DialogSystem.Instance.StartDialogue(dialogueKey, delay);

        if (!cheak) return;

        if (nextDialogEvant) nextDialogEvant.SetActive(true);

        gameObject.SetActive(false);
    }
}
