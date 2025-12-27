using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip flip;
    public AudioClip match;
    public AudioClip mismatch;
    public AudioClip gameOverWon;
    public AudioClip gameOverLoss;

    private AudioSource source;

    private void Awake()
    {
        Instance = this;
        source = gameObject.AddComponent<AudioSource>();
    }

    public void PlayFlip() => source.PlayOneShot(flip);
    public void PlayMatch() => source.PlayOneShot(match);
    public void PlayMismatch() => source.PlayOneShot(mismatch);
    public void PlayGameOverWon() => source.PlayOneShot(gameOverWon);
    public void PlayGameOverLoss() => source.PlayOneShot(gameOverLoss);
}
