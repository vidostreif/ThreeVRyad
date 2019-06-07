using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ParticleSystemBank
{
    private static PSResurse[] pSList = null;
    private static string pSFolder = "Prefabs/ParticleSystem";

    public static PSResurse[] PSList {
        get {
            CreatePSList();
            return pSList;
        }
    }

    private static void CreatePSList()
    {
        if (pSList == null)
        {
            pSList = new PSResurse[7];
            pSList[0] = new PSResurse(PSEnum.PSCollect, pSFolder, "PSCollect");
            pSList[1] = new PSResurse(PSEnum.PSCollectAll, pSFolder, "PSCollectAll");
            pSList[2] = new PSResurse(PSEnum.PSSelect, pSFolder, "PSSelect");
            pSList[3] = new PSResurse(PSEnum.PSAddPowerSuperBonus, pSFolder, "PSAddSuperBonus");
            pSList[4] = new PSResurse(PSEnum.PSBeatsSuperBonus, pSFolder, "PSBeatSuperBonus");
            pSList[5] = new PSResurse(PSEnum.PSDirt, pSFolder, "PSDirt");
            pSList[6] = new PSResurse(PSEnum.PSDirtNextAction, pSFolder, "PSDirtNextAction");            
        }
    }

    //асинхронная загрузка данных
    public static ResourceRequest GetPSAsync(PSEnum pSEnum)
    {
        CreatePSList();
        for (int i = 0; i < pSList.Length; i++)
        {
            if (pSList[i].PSEnum == pSEnum)
            {
                return GetPSAsync(pSList[i]);
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
        for (int i = 0; i < pSList.Length; i++)
        {
            if (pSList[i].PSEnum == pSEnum)
            {
                return GetPS(pSList[i]);
            }
        }
        return null;
    }

    public static GameObject GetPS(PSResurse pSResurse)
    {
        return Resources.Load<GameObject>(pSResurse.PSFolderName + "/" + pSResurse.PSName);
    }
}


public class PSResurse
{
    private PSEnum pSEnum;
    private string pSFolderName;
    private string pSName;

    public PSEnum PSEnum { get => pSEnum; }
    public string PSFolderName { get => pSFolderName; }
    public string PSName { get => pSName; }

    public PSResurse(PSEnum pSEnum, string pSFolderName, string pSName)
    {
        this.pSEnum = pSEnum;
        this.pSFolderName = pSFolderName;
        this.pSName = pSName;
    }
}
