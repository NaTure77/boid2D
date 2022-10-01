using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIManagement : MonoBehaviour
{
    public TextMeshProUGUI text;

    public GameObject cohesionPanel;
    public GameObject alignmentPanel;
    public GameObject separationPanel;
    public GameObject directionSensPanel;
    public GameObject speedPanel;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetMainText(float imageSize, int particleCount, int neighborRadius)
    {
        text.text = "Image Size: " + imageSize + "x" + imageSize + "\n" +
                    "Boid Count: " + particleCount + "\n" +
                    "Boid Range: " + neighborRadius;
    }
    public void SetCohesionSlider(UnityAction<float> action, float startValue)
    {
        TextMeshProUGUI cohesionText = cohesionPanel.GetComponentInChildren<TextMeshProUGUI>();
        Slider cohesionSlider = cohesionPanel.GetComponentInChildren<Slider>();
        cohesionSlider.onValueChanged.AddListener((val) =>
        {
            action(val);
            cohesionText.text = "Coheision: " + val;
        });
        cohesionSlider.maxValue = 10;
        cohesionSlider.minValue = 0;
        cohesionSlider.value = startValue;
    }
    public void SetAlignmentSlider(UnityAction<float> action, float startValue)
    {
        TextMeshProUGUI alignmentText = alignmentPanel.GetComponentInChildren<TextMeshProUGUI>();
        Slider alignmenttSlider = alignmentPanel.GetComponentInChildren<Slider>();
        alignmenttSlider.onValueChanged.AddListener((val) =>
        {
            action(val);
            alignmentText.text = "Alignment: " + val;
        });
        alignmenttSlider.maxValue = 10;
        alignmenttSlider.minValue = 0;
        alignmenttSlider.value = startValue;
    }
    public void SetSeparationSlider(UnityAction<float> action, float startValue)
    {
        TextMeshProUGUI separationText = separationPanel.GetComponentInChildren<TextMeshProUGUI>();
        Slider separationSlider = separationPanel.GetComponentInChildren<Slider>();
        separationSlider.onValueChanged.AddListener((val) =>
        {
            action(val);
            separationText.text = "Separation: " + val;
        });
        separationSlider.maxValue = 10;
        separationSlider.minValue = 0;
        separationSlider.value = startValue;
    }
    public void SetSpeedSlider(UnityAction<float> action, float startValue)
    {
        TextMeshProUGUI speedText = speedPanel.GetComponentInChildren<TextMeshProUGUI>();
        Slider speedSlider = speedPanel.GetComponentInChildren<Slider>();
        speedSlider.onValueChanged.AddListener((val) =>
        {
            action(val);
            speedText.text = "Speed: " + val;
        });
        speedSlider.maxValue = 10;
        speedSlider.minValue = 0;
        speedSlider.value = startValue;
    }
    public void SetDirectionSensSlider(UnityAction<float> action, float startValue)
    {
        TextMeshProUGUI dirText = directionSensPanel.GetComponentInChildren<TextMeshProUGUI>();
        Slider dirSlider = directionSensPanel.GetComponentInChildren<Slider>();
        dirSlider.onValueChanged.AddListener((val) =>
        {
            action(val);
            dirText.text = "DirectionSenstivity: " + val;
        });
        dirSlider.maxValue = 1;
        dirSlider.minValue = 0;
        dirSlider.value = startValue;
    }
}
