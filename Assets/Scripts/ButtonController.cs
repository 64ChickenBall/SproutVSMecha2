using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{

    public void OnLevel1ButtonClick()
    {
        Debug.Log("Level 1 Button clicked!");
        SceneManager.LoadScene("Level1");
    }

    public void OnLevel2ButtonClick()
    {
        Debug.Log("Level 2 Button clicked!");
        SceneManager.LoadScene("Level2");
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
