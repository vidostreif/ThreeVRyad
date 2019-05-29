using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class MainParticleSystem /*: MonoBehaviour*/
{
    private static PSResurse[] pSList = null;
    private static string pSFolder = "Prefabs/ParticleSystem";

    //public static MainParticleSystem Instance; // Синглтон
    //public GameObject pSMagicalTail;
    //public GameObject pSSelect;
    //public GameObject pSAddPowerSuperBonus;
    //public GameObject pSBeatsSuperBonus;

    private static void CreatePSList()
    {
        if (pSList == null)
        {
            pSList = new PSResurse[5];
            pSList[0] = new PSResurse(PSEnum.PSCollect, pSFolder, "PSCollect");
            pSList[1] = new PSResurse(PSEnum.PSCollectAll, pSFolder, "PSCollectAll");
            pSList[2] = new PSResurse(PSEnum.PSSelect, pSFolder, "PSSelect");
            pSList[3] = new PSResurse(PSEnum.PSAddPowerSuperBonus, pSFolder, "PSAddSuperBonus");
            pSList[4] = new PSResurse(PSEnum.PSBeatsSuperBonus, pSFolder, "PSBeatSuperBonus");
        }
    }

    //прогрев всех GO
    public static void WarmingUp() {

        GameObject warmingUpPS= new GameObject();
        warmingUpPS.transform.position = new Vector3(1000, 1000, 0);
        GameObject.Destroy(warmingUpPS, 5);

        CreatePSList();
        for (int i = 0; i < pSList.Length; i++)
        {
            CreatePSAsync(warmingUpPS.transform, pSList[i].PSEnum, 3);
        }
    }

    //асинхронная загрузка данных
    private static ResourceRequest GetPSAsync(PSEnum pSEnum) {
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

    private static ResourceRequest GetPSAsync(PSResurse pSResurse)
    {
        return Resources.LoadAsync<GameObject>(pSResurse.PSFolderName + "/" + pSResurse.PSName);
    }

    public static IEnumerator CreatePSAsync(Transform parentTransform, PSEnum pSEnum, float lifeTime = 0)
    {
        ResourceRequest request = GetPSAsync(pSEnum);
        //ожидаем загрузки ресурса
        while (request != null && !request.isDone)
        {
            yield return null;
        }

        //если не нашли, или не смогли загрузить, то выходим
        if (request == null)
        {
            Debug.Log("Эффект не загружен: " + pSEnum);
            yield break;
        }

        GameObject requestGO = (GameObject)request.asset;
        if (requestGO == null)
        {
            Debug.Log("Эффект не загружен: " + pSEnum);
            yield break;
        }

        if (parentTransform != null)
        {
            //создаем эффект 
            GameObject psGO = GameObject.Instantiate(requestGO, parentTransform);
            //время жизни эффекта
            if (lifeTime != 0)
            {
                GameObject.Destroy(psGO, lifeTime);
            }
        }        
    }

    //синхронная загрузка GO
    private static GameObject GetPS(PSEnum pSEnum)
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

    private static GameObject GetPS(PSResurse pSResurse)
    {
        return Resources.Load<GameObject>(pSResurse.PSFolderName + "/" + pSResurse.PSName);
    }

    public static GameObject CreatePS(Transform parentTransform, PSEnum pSEnum, float lifeTime = 0)
    {
        GameObject requestGO = GetPS(pSEnum);
        if (requestGO == null)
        {
            Debug.Log("Эффект не загружен: " + pSEnum);
            return null;
        }

        //создаем эффект 
        GameObject psGO = GameObject.Instantiate(requestGO, parentTransform);
        //время жизни эффекта
        if (lifeTime != 0)
        {
            GameObject.Destroy(psGO, lifeTime);
        }

        return psGO;
    }

    public static void CreateCollectAllEffect(Transform parentTransform, Image image)
    {
        //создаем эффект 
        GameObject psGO = CreatePS(parentTransform, PSEnum.PSCollectAll, 4);
        //изменяем цвет
        ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
        ps.textureSheetAnimation.AddSprite(image.sprite);
    }

    public static void CreateCollectEffect(Transform parentTransform, Image image)
    {
        //создаем эффект 
        GameObject psGO = CreatePS(parentTransform, PSEnum.PSCollect, 4);
        //изменяем цвет
        ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
        ps.textureSheetAnimation.AddSprite(image.sprite);
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
