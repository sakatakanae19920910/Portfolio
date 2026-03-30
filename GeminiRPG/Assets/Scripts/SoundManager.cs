using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// サウンド管理クラス（改良版：BGM/BGS/ME/SE 対応）
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    // ==================== BGM ====================
    [Header("BGM (Background Music)")]
    public AudioSource bgmSource;
    public List<AudioClip> bgmClips = new List<AudioClip>();
    private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();

    // ==================== BGS ====================
    [Header("BGS (Background Sound - 環境音)")]
    public AudioSource bgsSource;
    public List<AudioClip> bgsClips = new List<AudioClip>();
    private Dictionary<string, AudioClip> bgsDict = new Dictionary<string, AudioClip>();

    // ==================== ME ====================
    [Header("ME (Music Effect - ジングル)")]
    public AudioSource meSource;
    public List<AudioClip> meClips = new List<AudioClip>();
    private Dictionary<string, AudioClip> meDict = new Dictionary<string, AudioClip>();

    // ==================== SE ====================
    [Header("SE (Sound Effect)")]
    public AudioSource[] seSourcePool;
    public List<AudioClip> seClips = new List<AudioClip>();
    private Dictionary<string, AudioClip> seDict = new Dictionary<string, AudioClip>();
    private int currentSEIndex = 0;

    // ==================== 音量設定 ====================
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 0.7f;
    [Range(0f, 1f)] public float bgsVolume = 0.5f;
    [Range(0f, 1f)] public float meVolume = 1f;
    [Range(0f, 1f)] public float seVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        // 辞書に登録
        RegisterClips(bgmClips, bgmDict, "BGM");
        RegisterClips(bgsClips, bgsDict, "BGS");
        RegisterClips(meClips, meDict, "ME");
        RegisterClips(seClips, seDict, "SE");

        LoadVolumeSettings();

        Debug.Log($"SoundManager 初期化完了: BGM {bgmDict.Count}, BGS {bgsDict.Count}, ME {meDict.Count}, SE {seDict.Count}");
    }

    void RegisterClips(List<AudioClip> clips, Dictionary<string, AudioClip> dict, string type)
    {
        foreach (var clip in clips)
        {
            if (clip != null)
            {
                dict[clip.name] = clip;
            }
        }
    }

    // ==================== BGM ====================

    public AudioSource PlayBGM(string bgmName, bool loop = true)
    {
        if (!bgmDict.ContainsKey(bgmName))
        {
            Debug.LogError($"BGM '{bgmName}' が見つかりません");
            return null;
        }

        bgmSource.clip = bgmDict[bgmName];
        bgmSource.loop = loop;
        bgmSource.volume = masterVolume * bgmVolume;
        bgmSource.Play();
        return bgmSource;
    }

    public AudioSource PlayBGMWithFade(string bgmName, float fadeDuration = 1.0f, bool loop = true)
    {
        if (!bgmDict.ContainsKey(bgmName))
        {
            Debug.LogError($"BGM '{bgmName}' が見つかりません");
            return null;
        }

        bgmSource.clip = bgmDict[bgmName];
        bgmSource.loop = loop;
        bgmSource.volume = 0f;
        bgmSource.Play();
        bgmSource.DOFade(masterVolume * bgmVolume, fadeDuration);
        return bgmSource;
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void StopBGMWithFade(float fadeDuration = 1.0f)
    {
        bgmSource.DOFade(0f, fadeDuration).OnComplete(() => bgmSource.Stop());
    }

    // ==================== BGS ====================

    public AudioSource PlayBGS(string bgsName, bool loop = true)
    {
        if (!bgsDict.ContainsKey(bgsName))
        {
            Debug.LogError($"BGS '{bgsName}' が見つかりません");
            return null;
        }

        bgsSource.clip = bgsDict[bgsName];
        bgsSource.loop = loop;
        bgsSource.volume = masterVolume * bgsVolume;
        bgsSource.Play();
        return bgsSource;
    }

    public AudioSource PlayBGSWithFade(string bgsName, float fadeDuration = 1.0f, bool loop = true)
    {
        if (!bgsDict.ContainsKey(bgsName))
        {
            Debug.LogError($"BGS '{bgsName}' が見つかりません");
            return null;
        }

        bgsSource.clip = bgsDict[bgsName];
        bgsSource.loop = loop;
        bgsSource.volume = 0f;
        bgsSource.Play();
        bgsSource.DOFade(masterVolume * bgsVolume, fadeDuration);
        return bgsSource;
    }

    public void StopBGS()
    {
        bgsSource.Stop();
    }

    public void StopBGSWithFade(float fadeDuration = 1.0f)
    {
        bgsSource.DOFade(0f, fadeDuration).OnComplete(() => bgsSource.Stop());
    }

    // ==================== ME ====================

    public void PlayME(string meName)
    {
        if (!meDict.ContainsKey(meName))
        {
            Debug.LogError($"ME '{meName}' が見つかりません");
            return;
        }

        // ME再生中はBGMを一時停止（オプション）
        bool wasBGMPlaying = bgmSource.isPlaying;
        if (wasBGMPlaying)
        {
            bgmSource.Pause();
        }

        meSource.clip = meDict[meName];
        meSource.volume = masterVolume * meVolume;
        meSource.Play();

        // ME終了後にBGMを再開
        if (wasBGMPlaying)
        {
            float meLength = meDict[meName].length;
            Invoke(nameof(ResumeBGMAfterME), meLength);
        }
    }

    void ResumeBGMAfterME()
    {
        bgmSource.UnPause();
    }

    // ==================== SE ====================

    public void PlaySE(string seName)
    {
        if (!seDict.ContainsKey(seName))
        {
            Debug.LogError($"SE '{seName}' が見つかりません");
            return;
        }

        AudioSource source = GetAvailableSESource();
        source.volume = masterVolume * seVolume;
        source.PlayOneShot(seDict[seName]);
    }

    public void PlaySE(string seName, float volume)
    {
        if (!seDict.ContainsKey(seName))
        {
            Debug.LogError($"SE '{seName}' が見つかりません");
            return;
        }

        AudioSource source = GetAvailableSESource();
        source.volume = masterVolume * seVolume * volume;
        source.PlayOneShot(seDict[seName]);
    }

    private AudioSource GetAvailableSESource()
    {
        if (seSourcePool == null || seSourcePool.Length == 0)
        {
            Debug.LogError("SE AudioSource が設定されていません");
            return null;
        }

        AudioSource source = seSourcePool[currentSEIndex];
        currentSEIndex = (currentSEIndex + 1) % seSourcePool.Length;
        return source;
    }

    // ==================== 音量調整 ====================

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolume();
        SaveVolumeSettings();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = masterVolume * bgmVolume;
        SaveVolumeSettings();
    }

    public void SetBGSVolume(float volume)
    {
        bgsVolume = Mathf.Clamp01(volume);
        if (bgsSource != null) bgsSource.volume = masterVolume * bgsVolume;
        SaveVolumeSettings();
    }

    public void SetMEVolume(float volume)
    {
        meVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
    }

    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
    }

    private void ApplyVolume()
    {
        if (bgmSource != null) bgmSource.volume = masterVolume * bgmVolume;
        if (bgsSource != null) bgsSource.volume = masterVolume * bgsVolume;
        if (seSourcePool != null)
        {
            foreach (var source in seSourcePool)
            {
                if (source != null) source.volume = masterVolume * seVolume;
            }
        }
    }

    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("BGSVolume", bgsVolume);
        PlayerPrefs.SetFloat("MEVolume", meVolume);
        PlayerPrefs.SetFloat("SEVolume", seVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
        bgsVolume = PlayerPrefs.GetFloat("BGSVolume", 0.5f);
        meVolume = PlayerPrefs.GetFloat("MEVolume", 1f);
        seVolume = PlayerPrefs.GetFloat("SEVolume", 1f);
        ApplyVolume();
    }

    // ==================== ユーティリティ ====================

    public void StopAllSounds()
    {
        StopBGM();
        StopBGS();
        if (meSource != null) meSource.Stop();
        if (seSourcePool != null)
        {
            foreach (var source in seSourcePool)
            {
                if (source != null) source.Stop();
            }
        }
    }
}