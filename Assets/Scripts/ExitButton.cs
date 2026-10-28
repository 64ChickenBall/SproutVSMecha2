using UnityEngine;
using UnityEngine.SceneManagement;
public class ExitButton : MonoBehaviour
{

    public void OnExitButtonClick()
    {
        Debug.Log("Exit Button clicked!");
        SceneManager.LoadScene("StartScene");
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
