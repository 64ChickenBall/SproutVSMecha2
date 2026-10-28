using UnityEngine;
using TMPro;

public class ScoreController : MonoBehaviour
{
    public int Score { get; private set; } = 0;
    [SerializeField] private TMP_Text scoreText;

    public void UpdateScore(int addedScore)
    {
        this.Score += addedScore;
        this.scoreText.text = this.Score.ToString("D6");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
