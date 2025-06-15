using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("BGM")]
    public AudioClip bgmClip;            // Kéo clip nhạc nền vào
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;
    private AudioSource bgmSource;

    [Header("SFX Clips")]
    public AudioClip sfxMove;            // di chuyển 1 ô
    public AudioClip sfxEatBanana;
    public AudioClip sfxEatMedicine;
    public AudioClip sfxSnakeFall;
    public AudioClip sfxItemFall;
    // (Bạn có thể thêm clip khác nếu muốn)

    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tạo AudioSource cho BGM
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.playOnAwake = false;

            // Tạo AudioSource cho SFX
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;

            PlayBGM();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Bắt đầu phát nhạc nền (loop).
    /// </summary>
    public void PlayBGM()
    {
        if (bgmSource != null && bgmClip != null && !bgmSource.isPlaying)
        {
            bgmSource.volume = bgmVolume;
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// Dừng BGM (nếu cần).
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// Phát 1 clip SFX (một lần).
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// Điều chỉnh volume BGM.
    /// </summary>
    public void SetBGMVolume(float vol)
    {
        bgmVolume = Mathf.Clamp01(vol);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }

    /// <summary>
    /// Điều chỉnh volume SFX.
    /// </summary>
    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
}
