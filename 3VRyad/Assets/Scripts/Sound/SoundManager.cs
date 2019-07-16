using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

//[ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    public static SoundManager _instance;
    private List<AudioSource> soundsList;
    private List<SoundResurse> loadSoundList;
    private AudioMixer audioMixer;
    private AudioMixerGroup audMixThisCompressor;
    private AudioMixerGroup audMixThisoutCompressor;
    private bool soundToDeleteSearchIdle;
    private bool createSoundsListComplite;
    private bool createPoolComplite;

    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = GameObject.Find("GameHelper");
                SoundManager soundManager = go.GetComponent<SoundManager>();
                if (soundManager == null)
                {
                    Debug.Log("Добавляемтся новый SoundManager");
                    soundManager = go.AddComponent<SoundManager>();
                }
                else
                {
                    Debug.Log("Берется существующий SoundManager");
                }
                soundManager.CreateSoundsList();
                _instance = soundManager;
            }            
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance)
        {
            Destroy(this); //Delete duplicate
            return;
        }
        else
        {
            _instance = this; //Make this object the only instance            
        }

        if (Application.isPlaying)
        {
            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy
        }
        CreateSoundsList();
    }

    private void Start()
    {
        //создание пула звуков
        PoolManager.Instance.PoolsSetupAddPool("Sound", PrefabBank.SoundGO, 35);
        createPoolComplite = true;
    }

    private void SoundToDeleteSearch()
    {
        if (!soundToDeleteSearchIdle)
        {
            StartCoroutine(CurSoundToDeleteSearch());
        }
    }

    //куротина для возврата арендованных объектов
    private IEnumerator CurSoundToDeleteSearch()
    {
        soundToDeleteSearchIdle = true;
        while (soundsList != null && soundsList.Count > 0)
        {
            yield return new WaitForSeconds(0.05f);

            // каждый проход удаляем только один законившийся звук
            AudioSource soundToDelete = null;

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
                soundToDelete.clip = null;
                PoolManager.Instance.ReturnObjectToPool(soundToDelete.gameObject);
            }
        }
        soundToDeleteSearchIdle = false;
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
        createSoundsListComplite = true;
    }

    private bool IsSoundFinished(AudioSource sound)
    {
        if (sound.isPlaying)
            return false;

        return true;
    }

    public void PlaySoundInternal(SoundsEnum soundName, bool thisOutCompress = false)
    {
        if (!Application.isPlaying || !createSoundsListComplite || !createPoolComplite)
        {
            return;
        }
        SoundResurse soundResurse = SoundBank.GetSoundResurse(soundName);
        if (soundResurse == null)
        {
            return;
        }

        int sameCountGuard = 0;
        foreach (AudioSource audioSource in soundsList)
        {
            if (audioSource != null && audioSource.clip != null && audioSource.clip.name == soundResurse.SoundName)
                if (audioSource.time < 0.05f) 
                {
                    //Debug.Log(audioSource.clip.name + " " + audioSource.time);
                    return;
                } 
                sameCountGuard++;
        }
        //в загрузке
        foreach (SoundResurse inLoadSoundResurse in loadSoundList)
        {
            if (inLoadSoundResurse == soundResurse)
                return;
        }

        if (sameCountGuard > 10)
        {
            //Debug.Log("Слишком много звуков: " + soundName);
            return;
        }

        if (soundsList.Count > 30)
        {
            //Debug.Log("Вообще слишком много звуков!");
            return;
        }
        StartCoroutine(PlaySoundInternalSoon(soundResurse, thisOutCompress));
    }

    private IEnumerator PlaySoundInternalSoon(SoundResurse soundResurse, bool thisOutCompress)
    {
        AudioClip soundClip = soundResurse.AudioClip;

        //если потеряяли аудио, то предзагружаем его асинхронно
        if (soundClip == null)
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

            soundClip = (AudioClip)request.asset;
            if (soundClip == null)
            {
                Debug.Log("Звук не загружен: " + soundResurse.SoundName);
                yield break;
            }
        }

        GameObject sound = PoolManager.Instance.GetObjectToRent("Sound", transform.position, transform);
        //sound.transform.parent = transform;
        //sound.transform.position = transform.position;

        AudioSource soundSource = sound.GetComponent<AudioSource>();
        soundSource.mute = !SettingsController.SoundOn;
        soundSource.clip = soundClip;
        soundSource.outputAudioMixerGroup = thisOutCompress ? audMixThisoutCompressor : audMixThisCompressor;
        soundSource.Play();

        soundsList.Add(soundSource);
        SoundToDeleteSearch();
    }

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

    public void PlayCreateElement(AllShapeEnum allShapeEnum)
    {
        if (allShapeEnum == AllShapeEnum.Apple)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.CreateElement);
        }
        else if (allShapeEnum == AllShapeEnum.Camomile)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.CreateElement);
        }
        else if (allShapeEnum == AllShapeEnum.Mushroom)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.CreateElement);
        }
        else if (allShapeEnum == AllShapeEnum.Orange)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.CreateElement);
        }
        else if (allShapeEnum == AllShapeEnum.Plum)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.CreateElement);
        }
        else if (allShapeEnum == AllShapeEnum.Strawberry)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.CreateElement);
        }
        else if (allShapeEnum == AllShapeEnum.Dirt)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Dirt_create);
        }
        else if (allShapeEnum == AllShapeEnum.Liana)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Create_liana);
        }
        else if (allShapeEnum == AllShapeEnum.Web)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Create_web);
        }
        else if (allShapeEnum == AllShapeEnum.WildPlant)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Create_wildplant);
        }
    }

    public void PlayHitElement(AllShapeEnum allShapeEnum)
    {
        if (allShapeEnum == AllShapeEnum.WildPlant)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Hit_5);
        }
    }

    public void PlayDestroyElement(AllShapeEnum allShapeEnum)
    {
        if (allShapeEnum == AllShapeEnum.Apple)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.DestroyElement_3);
        }
        else if (allShapeEnum == AllShapeEnum.Camomile)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.DestroyElement_3);
        }
        else if (allShapeEnum == AllShapeEnum.Mushroom)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.DestroyElement_3);
        }
        else if (allShapeEnum == AllShapeEnum.Orange)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.DestroyElement_3);
        }
        else if (allShapeEnum == AllShapeEnum.Plum)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.DestroyElement_3);
        }
        else if (allShapeEnum == AllShapeEnum.Strawberry)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.DestroyElement_3);
        }
        else if (allShapeEnum == AllShapeEnum.Grass)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.LeafRustling);
        }
        else if (allShapeEnum == AllShapeEnum.Liana)
        {
            int randomNumber = UnityEngine.Random.Range(1, 4);
            if (randomNumber == 1)
            {
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Destroy_liana);
            }
            if (randomNumber == 2)
            {
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Destroy_liana_2);
            }
            else
            {
                SoundManager.Instance.PlaySoundInternal(SoundsEnum.Destroy_liana_3);
            }
            
        }
        else if (allShapeEnum == AllShapeEnum.Bush)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.LeafRustling);
        }
        else if (allShapeEnum == AllShapeEnum.Brick)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Destroy_brick);
        }
        else if (allShapeEnum == AllShapeEnum.BigFlask)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Boom_big);
        }
        else if (allShapeEnum == AllShapeEnum.MediumFlask)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Boom_big);
        }
        else if (allShapeEnum == AllShapeEnum.SmallFlask)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Boom);
        }
        else if (allShapeEnum == AllShapeEnum.Dirt)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Dirt_destroy);
        }
        else if (allShapeEnum == AllShapeEnum.Web)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Destroy_web);
        }
        else if (allShapeEnum == AllShapeEnum.WildPlant)
        {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Destroy_wildplant);
        }

    }
}
