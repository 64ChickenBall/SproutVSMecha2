using UnityEngine;

public class SillySpeedUp : MonoBehaviour
{
    public float speedup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speedup = 0.2f;
    }

    // Update is called once per frame
    void Update()
    {
        if (speedup < 10f){
            speedup += 0.3f*Time.deltaTime;
        }
        
    }
}
