using UnityEngine;
using TMPro;
using System;
public class TimeTextScript : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    public void UpdateTimeText(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string timeText = string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
        this.timeText.text = timeText;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimeText(Time.timeSinceLevelLoad);
    }
}
