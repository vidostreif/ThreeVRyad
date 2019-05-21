using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundBank 
{
    private static List<SoundResurse> soundsList = null;
    private static string soundFolder = "Sound";

    //здесь указываем enum для подсказок
    private static void CreateSoundList()
    {
        if (soundsList == null)
        {
            soundsList = new List<SoundResurse>();
            soundsList.Add( new SoundResurse(SoundsEnum.CreateElement, soundFolder, "Click"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElemen, soundFolder, "Socapex3"));
        }
    }

    public static ResourceRequest GetSoundAsync(SoundsEnum soundName) {
        CreateSoundList();
        foreach (SoundResurse soundResurse in soundsList)
        {
            if (soundResurse.SoundEnum == soundName)
            {
                return GetSoundAsync(soundResurse);
            }
        }
        return null;
    }

    public static ResourceRequest GetSoundAsync(SoundResurse soundResurse)
    {
        return Resources.LoadAsync<AudioClip>(soundResurse.SoundFolderName + "/" + soundResurse.SoundName);
    }

    public static SoundResurse GetSoundResurse(SoundsEnum soundName)
    {
        CreateSoundList();
        foreach (SoundResurse soundResurse in soundsList)
        {
            if (soundResurse.SoundEnum == soundName)
            {
                return soundResurse;
            }
        }
        return null;
    }

}

public class SoundResurse {
    private SoundsEnum soundEnum;
    private string soundFolderName;
    private string soundName;

    public SoundsEnum SoundEnum { get => soundEnum; }
    public string SoundFolderName { get => soundFolderName; }
    public string SoundName { get => soundName; }

    public SoundResurse(SoundsEnum soundEnum, string soundFolderName, string soundName)
    {
        this.soundEnum = soundEnum;
        this.soundFolderName = soundFolderName;
        this.soundName = soundName;
    }
}
