using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LivesBarScript : MonoBehaviour
{
    [SerializeField] private GameObject lifeIconPrefab;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.3f); // Transparent for depleted lives

    private List<Image> lifeIcons = new List<Image>();

    public void InitializeLives(int totalLives)
    {

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        lifeIcons.Clear();

        for (int i = 0; i < totalLives; i++)
        {
            GameObject lifeIcon = Instantiate(lifeIconPrefab, transform);
            Image iconImage = lifeIcon.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.color = activeColor;
                lifeIcons.Add(iconImage);
            }
        }
    }

    public void UpdateLives(int currentLives)
    {
        for (int i = 0; i < lifeIcons.Count; i++)
        {
            if (i < currentLives)
            {
                lifeIcons[i].color = activeColor;
            }
            else
            {
                lifeIcons[i].color = inactiveColor;
            }
        }
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
