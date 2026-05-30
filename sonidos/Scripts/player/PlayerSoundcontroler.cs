using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSoundcontroler : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource audioSourceSfx;
    public AudioSource audioSourceLoop;
    public AudioClip audioattack;
    public AudioClip audiodead;
    public AudioClip audiojump;
    public AudioClip audiorun;
    public AudioClip audiospawn;

    void Awake()
    {
        if (audioSourceSfx == null)
            audioSourceSfx = GetComponent<AudioSource>();

        if (audioSourceLoop == null)
        {
            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            if (audioSourceSfx != null)
            {
                audioSourceLoop.outputAudioMixerGroup = audioSourceSfx.outputAudioMixerGroup;
                audioSourceLoop.volume = audioSourceSfx.volume;
                audioSourceLoop.pitch = audioSourceSfx.pitch;
                audioSourceLoop.spatialBlend = audioSourceSfx.spatialBlend;
                audioSourceLoop.playOnAwake = false;
            }
        }
    }

    public void PlayAttack()
    {
        if (audioSourceSfx != null && audioattack != null)
        {
            audioSourceSfx.PlayOneShot(audioattack);
        }
    }

    public void PlayDead()
    {
        if (audioSourceSfx != null && audiodead != null)
        {
            audioSourceSfx.PlayOneShot(audiodead);
        }
    }

    public void PlayJump()
    {
        if (audioSourceSfx != null && audiojump != null)
        {
            audioSourceSfx.PlayOneShot(audiojump);
        }
    }

    public void PlayRun()
    {
        if (audioSourceLoop == null || audiorun == null) return;
        if (audioSourceLoop.isPlaying && audioSourceLoop.clip == audiorun) return;
        audioSourceLoop.clip = audiorun;
        audioSourceLoop.loop = true;
        audioSourceLoop.Play();
    }

    public void StopRun()
    {
        if (audioSourceLoop == null) return;
        if (audioSourceLoop.isPlaying && audioSourceLoop.clip == audiorun)
            audioSourceLoop.Stop();
    }

    public void PlaySpawn()
    {
        if (audioSourceSfx != null && audiospawn != null)
        {
            audioSourceSfx.PlayOneShot(audiospawn);
        }
    }
}
