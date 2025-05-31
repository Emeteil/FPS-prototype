using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChangeSceneTrigger : MonoBehaviour
{
    [SerializeField] private int scene = -1;
    [SerializeField] private bool waitDialoge = true;

    private void OnTriggerEnter(Collider other)
    {
        if (DialogSystem.Instance.inDialogue && waitDialoge) return;

        SceneChanger.Instance.ChangeScene(scene);
    }

    private void OnTriggerStay(Collider other)
    {
        if (DialogSystem.Instance.inDialogue && waitDialoge) return;

        SceneChanger.Instance.ChangeScene(scene);
    }
}
