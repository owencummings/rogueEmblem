using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UILibrary;

[RequireComponent(typeof(UIDocument))]
public class LevelTransitionComponent : MonoBehaviour
{

    CircleTransitionElement m_CircleTransitionElement;
    float transitionTime = 1f;
    float currTime = 0f;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        m_CircleTransitionElement = (CircleTransitionElement)root[0];
        //m_CircleTransitionElement = new CircleTransitionElement();
        //root.Add(m_CircleTransitionElement);
    }

    void Update()
    {
        // TODO: improve easing and timing, etc.
        currTime += Time.deltaTime;
        m_CircleTransitionElement.progress = currTime/transitionTime * 100f;
    }
}