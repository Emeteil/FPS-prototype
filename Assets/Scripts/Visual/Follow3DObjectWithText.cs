using System;
using UnityEngine;
using UnityEngine.UI;

public class Follow3DObjectWithText : MonoBehaviour
{
    [Header("Text Positioning")]
    [SerializeField] private Vector2 offset = new Vector2(0, 0);
    [SerializeField] private float yPluss = 0.0f;
    [SerializeField] public bool enableDistanceCheck = true;
    [SerializeField] private float maxDistance = 4f;

    [Header("Text Appearance")]
    [SerializeField] private string textContent = "Press [F]";
    [SerializeField] private int fontSize = 36;
    [SerializeField] private Font textFont;
    [SerializeField] private FontStyle fontStyle;
    [SerializeField] private TextAnchor textAlignment = TextAnchor.MiddleCenter;
    [SerializeField] private Color textColor = Color.black;
    [SerializeField] private Vector2 textSize = new Vector2(170, 45);

    [SerializeField] private Vector2 anchorMin = new Vector2(0, 0);
    [SerializeField] private Vector2 anchorMax = new Vector2(0, 0);

    [Header("Cooldowns")]
    [SerializeField] private float distanceCheckCooldown = 0.1f;
    [SerializeField] private float screenPositionCheckCooldown = 0.01f;

    [NonSerialized] public GameObject textObject;

    private Camera mainCamera;
    private RectTransform textRectTransform;
    private Text textComponent;
    private GameObject canvasObj;
    private Canvas canvas;

    private float lastDistanceCheckTime = 0f;
    private float lastScreenPositionCheckTime = 0f;

    private float currentDistance = float.MaxValue;
    private Vector3 currentScreenPosition;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) return;

        canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;

        canvas = canvasObj.GetComponent<Canvas>();

        textObject = new GameObject("WorldText");
        textObject.transform.SetParent(canvasObj.transform);
        textObject.transform.SetAsFirstSibling();

        if (enableDistanceCheck)
            textObject.SetActive(false);

        textRectTransform = textObject.AddComponent<RectTransform>();
        textRectTransform.sizeDelta = textSize;
        textRectTransform.anchorMin = anchorMin;
        textRectTransform.anchorMax = anchorMax;

        textComponent = textObject.AddComponent<Text>();
        textComponent.text = textContent;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.font = textFont != null ? textFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.alignment = textAlignment;
        textComponent.color = textColor;
    }

    public string GetText() => textContent;

    public void EditText(string text)
    {
        textContent = text;
        textComponent.text = textContent;
    }

    private void Update()
    {
        if (mainCamera == null || textRectTransform == null) return;

        Vector3 objectCenter = new Vector3(
            transform.position.x,
            transform.position.y + yPluss,
            transform.position.z
        );

        if (enableDistanceCheck && GetDistance(mainCamera.transform.position, objectCenter) > maxDistance)
        {
            textObject.SetActive(false);
            return;
        }

        if (enableDistanceCheck)
            textObject.SetActive(true);

        Vector3 screenPosition = GetScreenPosition(objectCenter);

        if (screenPosition.z < 0)
        {
            textObject.SetActive(false);
            return;
        }

        float scaleFactor = canvas.scaleFactor;

        textRectTransform.anchoredPosition = new Vector2(
            (screenPosition.x + offset.x) / scaleFactor,
            (screenPosition.y + offset.y) / scaleFactor
        );
    }

    private float GetDistance(Vector3 cameraPosition, Vector3 objectCenter)
    {
        if (Time.time - lastDistanceCheckTime >= distanceCheckCooldown)
        {
            lastDistanceCheckTime = Time.time;
            currentDistance = Vector3.Distance(cameraPosition, objectCenter);
        }
        return currentDistance;
    }

    private Vector3 GetScreenPosition(Vector3 objectCenter)
    {
        if (Time.time - lastScreenPositionCheckTime >= screenPositionCheckCooldown)
        {
            lastScreenPositionCheckTime = Time.time;
            currentScreenPosition = mainCamera.WorldToScreenPoint(objectCenter);
        }
        return currentScreenPosition;
    }
}