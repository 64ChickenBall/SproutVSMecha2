using System.Collections;
using UnityEngine;

public class BackgroundMusicPlayer : MonoBehaviour
{

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip introMusic;
    [SerializeField] private AudioClip backgroundMusic1;
    [SerializeField] private AudioClip backgroundMusic2Scared;
    [SerializeField] private AudioClip backgroundMusic3Dead;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(PlayIntroAndBackgroundMusic());
        if (SceneStateManager.Instance != null)
        {
            SceneStateManager.Instance.OnSceneStateChanged += HandleStateChange;
        }
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

    private void HandleStateChange(SceneState newState)
    {
        switch (newState)
        {
            case SceneState.Normal:
                audioSource.clip = backgroundMusic1;
                audioSource.loop = true;
                audioSource.Play();
                break;
            case SceneState.Scared:
                audioSource.clip = backgroundMusic2Scared;
                audioSource.loop = true;
                audioSource.Play();
                break;
        }
    }

    public void PlayDeadMusic()
    {
        audioSource.clip = backgroundMusic3Dead;
        audioSource.loop = false;
        audioSource.Play();
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
