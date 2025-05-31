using UnityEngine;

public class CheckDebug : MonoBehaviour
{
    [HideInInspector] public bool _block = false;
    private bool _ignorePause = false;

    public void Block(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = true;
        _block = true;
    }

    public void Unblock(bool pause = false)
    {
        if (pause && _ignorePause) return;
        if (!pause) _ignorePause = false;
        _block = false;
    }

    private void Start()
    {
        Pause.Instance.AddScript(this);
    }
    
    private void OnDisable() {}

    private void OnEnable() {}
}
