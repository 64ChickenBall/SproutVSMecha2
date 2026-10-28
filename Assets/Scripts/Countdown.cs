using UnityEngine;
using System.Collections;
using TMPro;

public class Countdown : MonoBehaviour
{
    [SerializeField] private TMP_Text countdownText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        // 3,2 1, GO!, starts the game
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        GetComponent<Canvas>().enabled = false;
        SceneStateManager.Instance.SetState(SceneState.Normal);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}