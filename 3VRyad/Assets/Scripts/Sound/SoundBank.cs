using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundBank 
{
    private static SoundResurse[] soundsArray = null;
    private static string soundFolder = "Sound";

    //здесь указываем enum для подсказок
    private static void CreateSoundList()
    {
        if (soundsArray == null)
        {
            List<SoundResurse> soundsList = new List<SoundResurse>();
            soundsList.Add(new SoundResurse(SoundsEnum.CreateElement, soundFolder, "klick_quiet"));
            soundsList.Add(new SoundResurse(SoundsEnum.Create_liana, soundFolder, "Create_liana"));
            soundsList.Add(new SoundResurse(SoundsEnum.Destroy_liana, soundFolder, "Destroy_liana"));
            soundsList.Add(new SoundResurse(SoundsEnum.Destroy_liana_2, soundFolder, "Destroy_liana_2"));
            soundsList.Add(new SoundResurse(SoundsEnum.Destroy_liana_3, soundFolder, "Destroy_liana_3"));
            soundsList.Add(new SoundResurse(SoundsEnum.Spread_liana, soundFolder, "Spread_liana"));            
            soundsList.Add(new SoundResurse(SoundsEnum.Create_wildplant, soundFolder, "Create_wildplant"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_1, soundFolder, "Socapex"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_2, soundFolder, "Socapex1"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_3, soundFolder, "Socapex2"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_4, soundFolder, "Socapex3"));
            soundsList.Add(new SoundResurse(SoundsEnum.DestroyElement_5, soundFolder, "Socapex4"));
            soundsList.Add(new SoundResurse(SoundsEnum.CreateBonus, soundFolder, "CreateBonus"));
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
            soundsList.Add(new SoundResurse(SoundsEnum.StartGame, soundFolder, "bird"));
            soundsList.Add(new SoundResurse(SoundsEnum.AddMove, soundFolder, "Rise"));
            soundsList.Add(new SoundResurse(SoundsEnum.Dirt_create, soundFolder, "Dirt_create"));
            soundsList.Add(new SoundResurse(SoundsEnum.Dirt_destroy, soundFolder, "Dirt_destroy"));
            soundsList.Add(new SoundResurse(SoundsEnum.Dirt_swelling, soundFolder, "Dirt_swelling"));
            soundsList.Add(new SoundResurse(SoundsEnum.Closed_chest, soundFolder, "closed_chest"));
            soundsList.Add(new SoundResurse(SoundsEnum.Zero_moves, soundFolder, "Zero_moves"));
            soundsList.Add(new SoundResurse(SoundsEnum.Two_moves, soundFolder, "Two_moves"));
            soundsList.Add(new SoundResurse(SoundsEnum.Life_add, soundFolder, "Life_add"));
            soundsList.Add(new SoundResurse(SoundsEnum.Hit_1, soundFolder, "Hit_1"));
            soundsList.Add(new SoundResurse(SoundsEnum.Hit_2, soundFolder, "Hit_2"));
            soundsList.Add(new SoundResurse(SoundsEnum.Hit_3, soundFolder, "Hit_3"));
            soundsList.Add(new SoundResurse(SoundsEnum.Hit_4, soundFolder, "Hit_4"));
            soundsList.Add(new SoundResurse(SoundsEnum.Hit_5, soundFolder, "Hit_5"));
            soundsList.Add(new SoundResurse(SoundsEnum.Spider_1, soundFolder, "Spider_1"));
            soundsList.Add(new SoundResurse(SoundsEnum.Spider_2, soundFolder, "Spider_2"));
            soundsList.Add(new SoundResurse(SoundsEnum.Create_web, soundFolder, "Create_web"));
            soundsList.Add(new SoundResurse(SoundsEnum.Destroy_web, soundFolder, "Destroy_web"));
            soundsList.Add(new SoundResurse(SoundsEnum.Destroy_wildplant, soundFolder, "Destroy_wildplant"));
            soundsList.Add(new SoundResurse(SoundsEnum.Preparation_wildplant, soundFolder, "Preparation_wildplant"));
            soundsList.Add(new SoundResurse(SoundsEnum.SeedBarrel_collect, soundFolder, "SeedBarrel_collect"));

            //soundsArray = soundsList.ToArray();

            //упоряд. массив
            int count = Enum.GetNames(typeof(SoundsEnum)).Length;
            soundsArray = new SoundResurse[count];

            foreach (SoundResurse item in soundsList)
            {
                soundsArray[(int)item.SoundEnum] = item;
            }  
        }        
    }

    //предзагрузка всех звуков
    public static void Preload()
    {
        CreateSoundList();
        //foreach (SoundResurse item in soundsList)
        //{
        //    GetSound(item);
        //}
    }

    public static ResourceRequest GetSoundAsync(SoundsEnum soundName) {
        CreateSoundList();
        //foreach (SoundResurse soundResurse in soundsArray)
        //{
            if (soundsArray[(int)soundName] != null)
            {
                return GetSoundAsync(soundsArray[(int)soundName]);
            }
        //}
        return null;
    }

    public static ResourceRequest GetSoundAsync(SoundResurse soundResurse)
    {
        return Resources.LoadAsync<AudioClip>(soundResurse.SoundFolderName + "/" + soundResurse.SoundName);
    }

    public static AudioClip GetSound(SoundResurse soundResurse)
    {
        return Resources.Load<AudioClip>(soundResurse.SoundFolderName + "/" + soundResurse.SoundName);
    }

    public static AudioClip GetSound(SoundsEnum soundName)
    {
        CreateSoundList();
        if (soundsArray[(int)soundName] != null)
        {
            return soundsArray[(int)soundName].AudioClip;
        }
        return null;
    }

    public static SoundResurse GetSoundResurse(SoundsEnum soundName)
    {
        CreateSoundList();
        if (soundsArray[(int)soundName] != null)
        {
            return soundsArray[(int)soundName];
        }
        return null;
    }
}

public class SoundResurse {
    private SoundsEnum soundEnum;
    private string soundFolderName;
    private string soundName;
    private AudioClip audioClip;

    public SoundsEnum SoundEnum { get => soundEnum; }
    public string SoundFolderName { get => soundFolderName; }
    public string SoundName { get => soundName; }
    public AudioClip AudioClip { get => audioClip; }

    public SoundResurse(SoundsEnum soundEnum, string soundFolderName, string soundName)
    {
        this.soundEnum = soundEnum;
        this.soundFolderName = soundFolderName;
        this.soundName = soundName;
        audioClip = SoundBank.GetSound(this);
    }
}
