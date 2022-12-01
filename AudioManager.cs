using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioMixer mixer;

    public static Music activeMusic = null;
    public List<AudioClip> musicIndex = new List<AudioClip>();
    public List<AudioClip> soundIndex = new List<AudioClip>();

    public static List<Music> allMusic = new List<Music>();
    public List<AudioSource> currentSFX = new List<AudioSource>();

    public float musicTransitionSpeed;
    public float pauseMusicLevel = 0.5f;

    public bool musicSmoothTransitions = true;
    public bool DDOL = false;

    [SerializeField]
    private AudioClip clickSFX;

    private int maxConcurrentSFX = 5;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            if (DDOL)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void PlayMusicFromList(string data)
    {
        print("Playing a single track");

        if (data != "")
        {
            foreach(AudioClip clip in musicIndex)
            {
                if (clip.name == data)
                {
                    AudioClip newClip = clip as AudioClip;
                    PlayMusic(newClip);
                    break;
                }
            }
        }
    }

    public void PlaySoundFromList(string data, float volume = 1f, float pitch = 1f, bool aux = false, bool loop = false)
    {
        if (data != "")
        {
            //foreach (AudioClip clip in soundIndex)
            //{
            //    if (clip.name == data)
            //    {
            //        AudioClip newClip = clip as AudioClip;
            //        PlaySFX(clip, volume, pitch, aux, loop);
            //        break;
            //    }
            //}


            AudioClip newClip = Resources.Load("Audio/Sounds/" + data) as AudioClip;
            PlaySFX(newClip, volume, pitch, aux, loop);

            

        }
    }

    public void Click(float volume = 1f, float pitch = 1f, bool aux = false, bool loop = false)
    {
        PlaySFX(clickSFX, volume, pitch, true, loop);
    }





    public void PlayMusic(AudioClip music, float maxVolume = 1f, float pitch = 1f, float startingVolume = 0f, bool playOnStart = true, bool loop = true, int mixerGroup = 0)
    {
        if (music != null)
        {
            for (int i = 0; i < allMusic.Count; i++)
            {
                Music m = allMusic[i];
                if (m.clip == music)
                {
                    activeMusic = m;
                    break;
                }
            }
            if (activeMusic == null || activeMusic.clip != music)
            {
                activeMusic = new Music(music, maxVolume, pitch, startingVolume, playOnStart, loop, mixerGroup);
            }
        }
        else
        {
            print("active music set to 'null'");

            activeMusic = null;
        }

        StopAllCoroutines();
        StartCoroutine(VolumeLevelling());
    }

    IEnumerator VolumeLevelling()
    {
        while (TransitionMusic())
        {
            yield return new WaitForEndOfFrame();
        }
    }

    bool TransitionMusic()
    {
        bool anyValueChanged = false;

        float speed = musicTransitionSpeed * Time.deltaTime;
        for (int i = allMusic.Count - 1; i >= 0; i--)
        {
            Music music = allMusic[i];
            if (music == activeMusic)
            {
                if (music.volume < music.maxVolume)
                {
                    music.volume = musicSmoothTransitions ? Mathf.Lerp(music.volume, music.maxVolume, speed) : Mathf.MoveTowards(music.volume, music.maxVolume, speed);
                    anyValueChanged = true;
                }
            }
            else
            {
                if (music.volume > 0)
                {
                    music.volume = musicSmoothTransitions ? Mathf.Lerp(music.volume, 0f, speed) : Mathf.MoveTowards(music.volume, 0f, speed);
                    anyValueChanged = true;
                }
                else
                {
                    allMusic.RemoveAt(i);
                    music.DestroyMusic();
                    continue;
                }
            }
        }

        return anyValueChanged;
    }

    public void PlaySFX(AudioClip sfx, float volume = 1f, float pitch = 1f, bool aux = false, bool loop = false)
    {
        AudioSource source = CreateNewAudioSource(string.Format("SFX [{0}]", sfx.name));
        source.clip = sfx;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = loop;


        source.outputAudioMixerGroup = mixer.FindMatchingGroups("GameplaySounds")[0];

        currentSFX.Add(source);
        SFXListCheck(source);
        source.Play();
        if (!loop)
        {
            Destroy(source.gameObject, sfx.length);
        }
    }

    public void SFXListCheck(AudioSource newSource)
    {
        int numberOfSameSounds = 0;

        for (int i = currentSFX.Count - 1; i >= 0; i--)
        {
            if (currentSFX[i] != null && currentSFX[i].name == newSource.name)
            {
                numberOfSameSounds += 1;

                if (numberOfSameSounds >= maxConcurrentSFX)
                {
                    var vol = (1 / (float)numberOfSameSounds);

                    currentSFX[i].volume = vol;
                }
            }
        }

        currentSFX.RemoveAll(sfx => sfx == null);

    }


    public void PauseAllSFX()
    {
        foreach (AudioSource source in currentSFX)
        {
            if (source != null && source.outputAudioMixerGroup == mixer.FindMatchingGroups("GameplaySounds")[0])
            {
                Debug.Log("Attempting to stop SFX playback for:" + source);
                source.Pause();
            }
            else
            {
                continue;
            }
        }
    }

    public void ResumeAllSFX()
    {
        foreach (AudioSource source in currentSFX)
        {
            source.UnPause();
        }
    }

    public void StopLoopingPlayback(string sfxName)
    {
        foreach (AudioSource source in instance.currentSFX)
        {
            if (source != null)
            {
                if (source.clip.name == sfxName)
                {
                    //AudioManager.instance.currentSFX.Remove(source);
                    Destroy(source.gameObject);
                    break;
                }
                else
                {
                    continue;
                }
            }
            else
            {
                continue;
            }
        }
    }


    public static AudioSource CreateNewAudioSource(string _name)
    {
        AudioSource newAudioSource = new GameObject(_name).AddComponent<AudioSource>();
        newAudioSource.transform.SetParent(instance.transform);
        return newAudioSource;
    }

    [System.Serializable]
    public class Music
    {
        public AudioSource source;
        public AudioClip clip { get { return source.clip; } set { source.clip = value; } }
        public float maxVolume = 1f;

        public Music(AudioClip clip, float _maxVolume, float pitch, float startingVolume, bool playOnStart, bool loop, int mixerGroup)
        {
            source = AudioManager.CreateNewAudioSource(string.Format("Music [{0}]", clip.name));
            source.clip = clip;
            source.volume = startingVolume;
            maxVolume = _maxVolume;
            source.pitch = pitch;
            source.playOnAwake = playOnStart;
            source.loop = loop;

            source.outputAudioMixerGroup = AudioManager.instance.mixer.FindMatchingGroups("Music")[mixerGroup];

            AudioManager.allMusic.Add(this);

            if (playOnStart)
            {
                source.Play();
            }
        }

        public float volume
        {
            get { return source.volume; }
            set { source.volume = value; }
        }
        public float pitch
        {
            get { return source.volume; }
            set { source.volume = value; }
        }

        public void Play()
        {
            source.Play();
        }
        public void Stop()
        {
            source.Stop();
        }
        public void Pause()
        {
            source.Pause();
        }
        public void UnPause()
        {
            source.UnPause();
        }
        public void DestroyMusic()
        {
            AudioManager.allMusic.Remove(this);
            DestroyImmediate(source.gameObject);
        }



    }

}
