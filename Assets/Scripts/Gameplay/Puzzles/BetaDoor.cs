using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Switch))]
public class BetaDoor : MonoBehaviour
{
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string closeTriggerName = "Close";

    private Animator animator;
    private Switch switchScript;

    private void Start()
    {
        animator = GetComponent<Animator>();
        switchScript = GetComponent<Switch>();

        if (animator == null || switchScript == null)
        {
            Debug.LogError("BetaDoor script requires an Animator and a Switch script on the same GameObject.");
            return;
        }
    }

    private void Update()
    {
        bool isToggleOn = switchScript.toggle;

        if (isToggleOn)
            animator.SetTrigger(openTriggerName);
        else
            animator.SetTrigger(closeTriggerName);
    }
}
