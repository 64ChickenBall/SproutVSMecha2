using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void EnableChange()
    {
        GetComponent<Canvas>().enabled = true;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
        
    }
    public void Start()
    {
        if (SceneStateManager.Instance != null)
        {
            SceneStateManager.Instance.OnSceneStateChanged += HandleStateChange;
        }
    }

    private void HandleStateChange(SceneState newState)
    {
        switch (newState)
        {
            case SceneState.GameOver:
                Debug.Log("Time TO CHANGE IDIOT");
                EnableChange();
                break;
        }
    }
}
