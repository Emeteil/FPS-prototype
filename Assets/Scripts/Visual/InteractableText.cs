using UnityEngine;

[RequireComponent(typeof(Follow3DObjectWithText))]
public class InteractableText : MonoBehaviour, IInteractable
{
    private Follow3DObjectWithText follow3DObjectWithText;

    private void Start()
    {
        follow3DObjectWithText = GetComponent<Follow3DObjectWithText>();
        follow3DObjectWithText.enableDistanceCheck = false;
    }

    private void Update()
    {
        if (follow3DObjectWithText.textObject != null)
        {
            follow3DObjectWithText.textObject.SetActive(false);
            this.enabled = false;
        }
    }

    public void OnHoverEnter()
    {
        if (follow3DObjectWithText != null && follow3DObjectWithText.textObject != null)
        {
            follow3DObjectWithText.textObject.SetActive(true);
        }
    }

    public void OnHoverExit()
    {
        if (follow3DObjectWithText != null && follow3DObjectWithText.textObject != null)
        {
            follow3DObjectWithText.textObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (follow3DObjectWithText != null && follow3DObjectWithText.textObject != null)
        {
            follow3DObjectWithText.textObject.SetActive(false);
            Destroy(follow3DObjectWithText.textObject);
        }
    }

    private void OnDisable()
    {
        follow3DObjectWithText.textObject?.SetActive(false);
    }
}