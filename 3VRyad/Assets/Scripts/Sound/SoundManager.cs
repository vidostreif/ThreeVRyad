using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    private List<AudioSource> soundsList;
    private List<SoundResurse> loadSoundList;
    private AudioMixer audioMixer;
    private AudioMixerGroup audMixThisCompressor;
    private AudioMixerGroup audMixThisoutCompressor;

    public static SoundManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            // Do not modify _instance here. It will be assigned in awake
            GameObject go = new GameObject("(singleton) SoundManager");
            SoundManager soundManager = go.AddComponent<SoundManager>();
            soundManager.CreateSoundsList();
            return soundManager;
        }
    }

    void Awake()
    {
        // Only one instance of SoundManager at a time!
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        if (Application.isPlaying)
        {
            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy
        }
        CreateSoundsList();
        //_sounds = new List<AudioSource>();
    }

    void Update()
    {
        // каждый фрей удаляем только один законившийся звук
        AudioSource soundToDelete = null;
        //CreateSoundsList();
        foreach (AudioSource sound in soundsList)
        {
            if (IsSoundFinished(sound))
            {
                soundToDelete = sound;
                break;
            }
        }

        if (soundToDelete != null)
        {
            soundsList.Remove(soundToDelete);
            Destroy(soundToDelete.gameObject);
        }
    }

    private void CreateSoundsList() {
        if (soundsList == null)
        {
            soundsList = new List<AudioSource>();

        }
        if (loadSoundList == null)
        {
            loadSoundList = new List<SoundResurse>();
        }
        if (audioMixer == null)
        {
            audioMixer = Resources.Load<AudioMixer>("Sound/AudioMixer");

            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups("ThisCompress");
            audMixThisCompressor = audioMixer.FindMatchingGroups("ThisCompress")[0];

            audMixThisoutCompressor = audioMixer.FindMatchingGroups("NoCompress")[0];
        }
    }

    private bool IsSoundFinished(AudioSource sound)
    {
        if (sound.isPlaying)
            return false;

        return true;
    }

    public void PlaySoundInternal(SoundsEnum soundName, bool thisOutCompress = false)
    {
        SoundResurse soundResurse = SoundBank.GetSoundResurse(soundName);
        if (soundResurse == null)
        {
            return;
        }

        int sameCountGuard = 0;
        foreach (AudioSource audioSource in soundsList)
        {
            if (audioSource.clip.name == soundResurse.SoundName)
                sameCountGuard++;
        }
        //в загрузке
        foreach (SoundResurse inLoadSoundResurse in loadSoundList)
        {
            if (inLoadSoundResurse == soundResurse)
                sameCountGuard++;
        }

        if (sameCountGuard > 10)
        {
            //Debug.Log("Слишком много звуков: " + soundName);
            return;
        }

        if (soundsList.Count > 20)
        {
            //Debug.Log("Вообще слишком много звуков!");
            return;
        }
        StartCoroutine(PlaySoundInternalSoon(soundResurse, thisOutCompress));
    }

    private IEnumerator PlaySoundInternalSoon(SoundResurse soundResurse, bool thisOutCompress)
    {
        ResourceRequest request = SoundBank.GetSoundAsync(soundResurse);
        loadSoundList.Add(soundResurse);
        while (request != null && !request.isDone)
        {
            yield return null;
        }
        loadSoundList.Remove(soundResurse);

        //если не нашли, или не смогли загрузить, то выходим
        if (request == null)
        {
            Debug.Log("Звук не загружен: " + soundResurse.SoundName);
            yield break;
        }

        AudioClip soundClip = (AudioClip)request.asset;
        if (soundClip == null)
        {
            Debug.Log("Звук не загружен: " + soundResurse.SoundName);
            yield break;
        }

        //GameObject sound = (GameObject)Instantiate(soundPrefab);
        GameObject sound = new GameObject();
        sound.transform.parent = transform;
        sound.transform.position = transform.position;

        ////определяем громкость
        //float volume = 1;
        //if (countSoundsAlreadyPlayed != 0)
        //{
        //    volume = 1.2f/((float)countSoundsAlreadyPlayed + 1);
        //    SetVolumeForSoundResurseType(soundResurse.SoundName, volume);
        //}

        AudioSource soundSource = sound.AddComponent<AudioSource>();
        soundSource.mute = !SettingsController.SoundOn;
        //soundSource.volume = volume;//уменьшаем громкость на количество уже воспроизводимых таких звуков
        soundSource.clip = soundClip;
        if (thisOutCompress)
        {
            Debug.Log("thisOutCompress");
        }
        soundSource.outputAudioMixerGroup = thisOutCompress ? audMixThisoutCompressor : audMixThisCompressor;
        soundSource.Play();
        //soundSource.ignoreListenerPause = !pausable;

        soundsList.Add(soundSource);
    }

    //public void SetVolumeForSoundResurseType(string name, float volume)
    //{
    //    //изменяем громкость для всех соундов с таким названием
    //    foreach (AudioSource item in soundsList)
    //    {
    //        if (item.clip.name == name)
    //        {
    //            item.volume = volume;
    //            Debug.Log("Громкость " + volume);
    //        }
    //    }
    //}

    public void SoundMute(bool mute) {
        //CreateSoundsList();
        foreach (AudioSource item in soundsList)
        {
            item.mute = mute;
        }
    }

    public void PlayClickButtonSound(){
        PlaySoundInternal(SoundsEnum.ClickButton);
    }
    
}
