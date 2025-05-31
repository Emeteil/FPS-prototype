using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickChange : MonoBehaviour
{
    [SerializeField] private List<GameObject> objs;
    [SerializeField] private ScrollRect scrollRect;

    private int index = 1;

    private void Start()
    {        
        for (int i = 0; i < objs.Count; i++)
            objs[i].SetActive(i == 0);

        scrollRect.content = objs[0].GetComponent<RectTransform>();
    }

    public void SwitchObjects()
    {
        for (int i = 0; i < objs.Count; i++)
            objs[i].SetActive(i == index);

        scrollRect.content = objs[index].GetComponent<RectTransform>();

        if (index != (objs.Count - 1))
            index++;
        else
            index = 0;
    }
}