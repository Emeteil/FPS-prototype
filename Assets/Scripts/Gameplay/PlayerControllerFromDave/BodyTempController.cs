using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BodyTempController : MonoBehaviour
{
    [SerializeField] private float criticalTemp = 20f;
    [SerializeField] private float maxTemp = 36.6f;
    [SerializeField] private bool onGUI = false;

    [SerializeField] private float coldExposureTime = 60f;

    [SerializeField] private float warmupRate = 0.2f;
    [SerializeField] private float transitionTime = 4f;

    [Header("GUI")]
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private List<Color> colors;
    [SerializeField] private Image statusImg;
    [SerializeField] private GameObject isWarm;
    [SerializeField] private Text text;

    private float currentTemp;
    private bool isNearWarmObj;
    private bool inTransition;
    private float transitionTimer;
    private bool previousIsNearWarmObj;
    private bool cheakObjects = true;
    private Life life;

    public static BodyTempController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private bool _block = false;
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

    public Dictionary<Transform, float> warmObjs = new Dictionary<Transform, float>();

    public void AddWarmObj(Transform transform, float distance)
    {
        warmObjs[transform] = distance;
    }

    public void RemoveWarmObj(Transform transform)
    {
        warmObjs.Remove(transform);
    }

    private void Start()
    {
        life = GetComponent<Life>();
        currentTemp = maxTemp;
    }

    public void inWarmForced(bool status)
    {
        if (status) isNearWarmObj = true;
        cheakObjects = !status;
    }

    private void Update()
    {
        if (_block) return;

        GUIUpdate();

        if (cheakObjects)
            isNearWarmObj = CheckIfNearWarmObj();

        if (isNearWarmObj != previousIsNearWarmObj)
        {
            if (transitionTimer > 0)
                transitionTimer = 0;
            else
                transitionTimer = transitionTime;

            previousIsNearWarmObj = isNearWarmObj;
        }

        if (transitionTimer > 0)
        {
            transitionTimer -= Time.deltaTime;
            inTransition = true;
        }
        else
        {
            inTransition = false;
            transitionTimer = 0;
        }

        if (inTransition) return;

        if (isNearWarmObj)
        {
            if (currentTemp < maxTemp)
            {
                currentTemp += warmupRate * Time.deltaTime;
                if (currentTemp > maxTemp) currentTemp = maxTemp;
            }
        }
        else
        {
            float cooldownRate = (maxTemp - criticalTemp) / coldExposureTime;

            currentTemp -= cooldownRate * Time.deltaTime;
            if (currentTemp <= criticalTemp)
            {
                life.Kill();
                Block();
            }
        }

        UpdatePlayerSpeed();
    }

    private bool CheckIfNearWarmObj()
    {
        foreach (var warmObj in warmObjs)
        {
            if (Vector3.Distance(transform.position, warmObj.Key.position) <= warmObj.Value)
                return true;
        }
        return false;
    }

    private void UpdatePlayerSpeed()
    {
        float tempRatio = (currentTemp - criticalTemp) / (maxTemp - criticalTemp);
        
        if (tempRatio >= 0.85f)
        {
            life.ResetDecelerate();
            return;
        }

        if (tempRatio < 0.35f)
        {
            life.StronglyDecelerate();
            return;
        }

        life.Decelerate();
    }

    private void GUIUpdate()
    {
        float tempRatio = (currentTemp - criticalTemp) / (maxTemp - criticalTemp);

        text.text = $"{Math.Round(currentTemp, 1)}°";

        isWarm.SetActive(isNearWarmObj);
        
        int index = (int)Math.Round(tempRatio * (sprites.Count - 1));
        index = Mathf.Clamp(index, 0, sprites.Count - 1);
        
        statusImg.sprite = sprites[index];
        text.color = colors[index];
    }
    
    private void OnGUI()
    {
        if (!onGUI) return;

        GUI.Label(new Rect(0, 15, 300, 30), $"Temp = {Math.Round(currentTemp, 2)}");
        GUI.Label(new Rect(0, 30, 300, 30), $"Warm now = {isNearWarmObj}");
    }
}