using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip flip;
    public AudioClip match;
    public AudioClip mismatch;
    public AudioClip gameOver;

    private AudioSource source;

    private void Awake()
    {
        Instance = this;
        source = gameObject.AddComponent<AudioSource>();
    }

    public void PlayFlip() => source.PlayOneShot(flip);
    public void PlayMatch() => source.PlayOneShot(match);
    public void PlayMismatch() => source.PlayOneShot(mismatch);
    public void PlayGameOver() => source.PlayOneShot(gameOver);
}
