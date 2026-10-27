using UnityEngine;
using UnityEngine.UI;

public class AnimatedBorder : MonoBehaviour
{
    [SerializeField] private float animationSpeed = 1.0f;
    [SerializeField] private Image borderImage;
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.black;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float t = Mathf.PingPong(Time.time * animationSpeed, 1.0f);
        borderImage.color = Color.Lerp(startColor, endColor, t);
    }
}
