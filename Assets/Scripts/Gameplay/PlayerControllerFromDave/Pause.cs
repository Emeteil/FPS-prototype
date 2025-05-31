using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    [SerializeField]
    private List<MonoBehaviour> scripts;

    [SerializeField]
    [Tooltip("Menu object")]
    private GameObject menu;

    private bool _block = false;

    public static Pause Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    [HideInInspector]
    public bool _paused = false;

    public void Block(bool block = true)
    {
        if (_block == block) return;

        foreach (var script in scripts)
        {
            if (script == null) continue;

            var type = script.GetType();

            string methodName = block ? "Block" : "Unblock";
            var method = type.GetMethod(methodName);

            if (method == null)
            {
                Debug.LogWarning($"Method {methodName} not found in component {type.Name}");
                continue;
            }

            method.Invoke(script, new object[] { true });
        }

        _block = block;
    }

    public void PauseGame(bool pause = true)
    {
        if (_paused == pause) return;

        menu.SetActive(pause);
        Block(pause);

        Time.timeScale = pause ? 0 : 1;
        Physics.autoSimulation = !pause;
        AudioListener.pause = pause;
        if (pause)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


        _paused = pause;
    }

    public void AddScript(MonoBehaviour script)
    {
        if (scripts.Contains(script)) return;
        scripts.Add(script);
    }

    private void Update()
    {
        bool esc = Input.GetKeyDown(KeyCode.Escape);

        if (esc) PauseGame(!_paused);
    }
}
