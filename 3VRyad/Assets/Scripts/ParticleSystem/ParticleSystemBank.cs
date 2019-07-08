using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ParticleSystemBank
{
    private static PSResurse[] pSArray = null;
    private static string pSFolder = "Prefabs/ParticleSystem";

    public static PSResurse[] PSArray {
        get {
            CreatePSList();
            return pSArray;
        }
    }

    private static void CreatePSList()
    {
        if (pSArray == null)
        {
            List<PSResurse> pSResurseList = new List<PSResurse>();
            pSResurseList.Add(new PSResurse(PSEnum.PSCollect, pSFolder, "PSCollect"));
            pSResurseList.Add(new PSResurse(PSEnum.PSCollectAll, pSFolder, "PSCollectAll"));
            pSResurseList.Add(new PSResurse(PSEnum.PSSelect, pSFolder, "PSSelect"));
            pSResurseList.Add(new PSResurse(PSEnum.PSMagicalTail, pSFolder, "PSMagicTail"));
            pSResurseList.Add(new PSResurse(PSEnum.PSAddPowerSuperBonus, pSFolder, "PSAddSuperBonus"));
            pSResurseList.Add(new PSResurse(PSEnum.PSBeatsSuperBonus, pSFolder, "PSBeatSuperBonus"));
            pSResurseList.Add(new PSResurse(PSEnum.PSDirt, pSFolder, "PSDirt"));
            pSResurseList.Add(new PSResurse(PSEnum.PSDirtNextAction, pSFolder, "PSDirtNextAction"));
            pSResurseList.Add(new PSResurse(PSEnum.PSWeb, pSFolder, "PSWeb"));
            pSResurseList.Add(new PSResurse(PSEnum.PSLiana, pSFolder, "PSLiana"));
            pSResurseList.Add(new PSResurse(PSEnum.PSWildPlantNextAction, pSFolder, "PSWildPlantNextAction"));
            pSResurseList.Add(new PSResurse(PSEnum.PSRocket, pSFolder, "Rocket"));
            pSResurseList.Add(new PSResurse(PSEnum.PSAddSuperBonusFromLevels, pSFolder, "PSAddSuperBonusFromLevels"));
            pSResurseList.Add(new PSResurse(PSEnum.PSSuperBonusActiveted, pSFolder, "PSSuperBonusActiveted"));
            pSResurseList.Add(new PSResurse(PSEnum.PSSelectTargetBlock, pSFolder, "PSSelectTargetBlock"));
            pSResurseList.Add(new PSResurse(PSEnum.PSSelectTargetBlockBlue, pSFolder, "PSSelectTargetBlockBlue"));
            

            pSArray = pSResurseList.ToArray();
        }
    }

    //предзагрузка всех PS
    public static void Preload()
    {
        CreatePSList();
    }

    //асинхронная загрузка данных
    public static ResourceRequest GetPSAsync(PSEnum pSEnum)
    {
        CreatePSList();
        for (int i = 0; i < pSArray.Length; i++)
        {
            if (pSArray[i].PSEnum == pSEnum)
            {
                return GetPSAsync(pSArray[i]);
            }
        }
        return null;
    }

    public static ResourceRequest GetPSAsync(PSResurse pSResurse)
    {
        return Resources.LoadAsync<GameObject>(pSResurse.PSFolderName + "/" + pSResurse.PSName);
    }

    //синхронная загрузка GO
    public static GameObject GetPS(PSEnum pSEnum)
    {
        CreatePSList();
        for (int i = 0; i < pSArray.Length; i++)
        {
            if (pSArray[i].PSEnum == pSEnum)
            {
                return GetPS(pSArray[i]);
            }
        }
        return null;
    }

    public static PSResurse GetPSResurse(PSEnum pSEnum)
    {
        CreatePSList();
        for (int i = 0; i < pSArray.Length; i++)
        {
            if (pSArray[i].PSEnum == pSEnum)
            {
                return pSArray[i];
            }
        }
        return null;
    }

    public static GameObject GetPS(PSResurse pSResurse)
    {
        if (pSResurse.Go != null)
        {
            return pSResurse.Go;
        }
        else
        {
            return Resources.Load<GameObject>(pSResurse.PSFolderName + "/" + pSResurse.PSName);
        }        
    }
}


public class PSResurse
{
    private PSEnum pSEnum;
    private string pSFolderName;
    private string pSName;
    private GameObject go;

    public PSEnum PSEnum { get => pSEnum; }
    public string PSFolderName { get => pSFolderName; }
    public string PSName { get => pSName; }
    public GameObject Go { get => go; }

    public PSResurse(PSEnum pSEnum, string pSFolderName, string pSName)
    {
        this.pSEnum = pSEnum;
        this.pSFolderName = pSFolderName;
        this.pSName = pSName;
        this.go = ParticleSystemBank.GetPS(this);
    }
}
