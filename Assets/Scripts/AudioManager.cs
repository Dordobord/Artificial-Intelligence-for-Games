using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager main;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic; 

    [Header("Game")]
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip leaderKillClip;
    [SerializeField] private AudioClip leaderDeathClip;
    [SerializeField] private AudioClip collectPawnClip;

    private void Awake()
    {
        if (main != null)
        {
            Destroy(gameObject);
            return;
        }

        main = this;

        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayVictory()
    {
        sfxSource.PlayOneShot(victoryClip);
    }

    public void PlayGameOver()
    {
        sfxSource.PlayOneShot(gameOverClip);
    }

    public void PlayLeaderKill()
    {
        sfxSource.PlayOneShot(leaderKillClip);
    }

    public void PlayLeaderDeath()
    {
        sfxSource.PlayOneShot(leaderDeathClip);
    }

    public void PlayAddUnit()
    {
        sfxSource.PlayOneShot(collectPawnClip);
    }
}