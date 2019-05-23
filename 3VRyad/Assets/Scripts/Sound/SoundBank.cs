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
            soundsList.Add( new SoundResurse(SoundsEnum.CreateElement, soundFolder, "klick_quiet"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_1, soundFolder, "Socapex"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_2, soundFolder, "Socapex1"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_3, soundFolder, "Socapex2"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_4, soundFolder, "Socapex3"));
            soundsList.Add(new SoundResurse(SoundsEnum.Bite, soundFolder, "bite"));
            soundsList.Add(new SoundResurse(SoundsEnum.LeafRustling, soundFolder, "leafRustling"));
            soundsList.Add(new SoundResurse(SoundsEnum.Boom, soundFolder, "boom"));
            soundsList.Add(new SoundResurse(SoundsEnum.Boom_mini_1, soundFolder, "boom_mini"));
            soundsList.Add(new SoundResurse(SoundsEnum.Boom_mini_2, soundFolder, "boom_mini_2"));
            soundsList.Add(new SoundResurse(SoundsEnum.Boom_mini_3, soundFolder, "boom_mini_3"));
            soundsList.Add(new SoundResurse(SoundsEnum.Boom_mini_4, soundFolder, "boom_mini_4"));
            soundsList.Add(new SoundResurse(SoundsEnum.Boom_big, soundFolder, "boom_big"));
            soundsList.Add(new SoundResurse(SoundsEnum.SuperBonusRocket, soundFolder, "rocket"));
            soundsList.Add(new SoundResurse(SoundsEnum.SuperBonusActiveted, soundFolder, "SuperBonusActiveted"));
            soundsList.Add(new SoundResurse(SoundsEnum.CollectElement, soundFolder, "magic"));
            soundsList.Add(new SoundResurse(SoundsEnum.Magic, soundFolder, "magic_2"));
            soundsList.Add(new SoundResurse(SoundsEnum.Star, soundFolder, "Rise02"));
            soundsList.Add(new SoundResurse(SoundsEnum.Applause, soundFolder, "Applause"));
            soundsList.Add(new SoundResurse(SoundsEnum.ClickButton, soundFolder, "Click"));
            soundsList.Add(new SoundResurse(SoundsEnum.Victory, soundFolder, "glassbell"));
            soundsList.Add(new SoundResurse(SoundsEnum.Defeat, soundFolder, "crush"));
            soundsList.Add(new SoundResurse(SoundsEnum.Coin, soundFolder, "coin"));
            soundsList.Add(new SoundResurse(SoundsEnum.Ring_1, soundFolder, "ring"));
            soundsList.Add(new SoundResurse(SoundsEnum.Ring_2, soundFolder, "ring_2"));
            soundsList.Add(new SoundResurse(SoundsEnum.Laser, soundFolder, "laser"));
            soundsList.Add(new SoundResurse(SoundsEnum.Card, soundFolder, "wood"));
            soundsList.Add(new SoundResurse(SoundsEnum.Wind, soundFolder, "wind"));
            soundsList.Add(new SoundResurse(SoundsEnum.Wind_active, soundFolder, "wind_active"));
            soundsList.Add(new SoundResurse(SoundsEnum.Shovel, soundFolder, "shovel"));
            soundsList.Add(new SoundResurse(SoundsEnum.LawnMowerStart, soundFolder, "lawnMowerStart"));
            soundsList.Add(new SoundResurse(SoundsEnum.LawnMowerWork, soundFolder, "lawnMowerWork"));
            soundsList.Add(new SoundResurse(SoundsEnum.Repainting, soundFolder, "repainting"));
            soundsList.Add(new SoundResurse(SoundsEnum.Repainting_ring, soundFolder, "repainting_ring"));
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
