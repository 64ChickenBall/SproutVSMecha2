using System.Collections;
using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip backgroundMusic1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlayIntroAndBackgroundMusic());
    }

    private IEnumerator PlayIntroAndBackgroundMusic()
    {
        audioSource.clip = introMusic;
        audioSource.Play();
        yield return new WaitForSeconds(3f);
        audioSource.Stop();
        audioSource.clip = backgroundMusic1;
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
