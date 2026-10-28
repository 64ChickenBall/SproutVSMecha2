using UnityEngine;
using TMPro;
public class SetLevel1Scores : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText1;
    [SerializeField] private TMP_Text scoreText1;
    [SerializeField] private TMP_Text timeText2;
    [SerializeField] private TMP_Text scoreText2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(timeText1.text);
        // Level 1
        int score1 = PlayerPrefs.GetInt("Level1_HighScore", 0);
        int bestTime1 = PlayerPrefs.GetInt("Level1_BestTime", 0);
        scoreText1.text = score1.ToString("D6");
        timeText1.text = FormatTime(bestTime1);
        // Level 2
        int score2 = PlayerPrefs.GetInt("Level2_HighScore", 0);
        int bestTime2 = PlayerPrefs.GetInt("Level2_BestTime", 0);

        scoreText2.text = score2.ToString("D6");
        timeText2.text = FormatTime(bestTime2);
    }

    private string FormatTime(int totalCentiseconds)
    {
        int minutes = totalCentiseconds / 6000;
        int seconds = (totalCentiseconds / 100) % 60;
        int centiseconds = totalCentiseconds % 100;

        return $"{minutes:00}:{seconds:00}:{centiseconds:00}";
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
