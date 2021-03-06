﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleSystemManager : MonoBehaviour
{
    public static ParticleSystemManager Instance; // Синглтон

    void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject); //Delete duplicate
            return;
        }
        else
        {
            Instance = this; //Make this object the only instance            
        }

        if (Application.isPlaying)
        {
            DontDestroyOnLoadManager.DontDestroyOnLoad(gameObject); //Set as do not destroy
        }
    }

    private void Start()
    {
        Preload();
    }

    //прогрев всех GO
    public void Preload()
    {
        for (int i = 0; i < ParticleSystemBank.PSArray.Length; i++)
        {
            PoolManager.Instance.PoolsSetupAddPool(ParticleSystemBank.PSArray[i].PSEnum.ToString(), ParticleSystemBank.GetPS(ParticleSystemBank.PSArray[i].PSEnum), ParticleSystemBank.PSArray[i].MinNumberOnStage);
        }
    }

    //private IEnumerator CurCreatePSAsync(Transform parentTransform, PSEnum pSEnum, float lifeTime = 0)
    //{
    //    GameObject requestGO;
    //    PSResurse pSResurse = ParticleSystemBank.GetPSResurse(pSEnum);
    //    if (pSResurse.Go != null)
    //    {
    //        requestGO = pSResurse.Go;
    //    }
    //    else
    //    {
    //        ResourceRequest request = ParticleSystemBank.GetPSAsync(pSResurse);
    //        //ожидаем загрузки ресурса
    //        while (request != null && !request.isDone)
    //        {
    //            yield return null;
    //        }

    //        //если не нашли, или не смогли загрузить, то выходим
    //        if (request == null)
    //        {
    //            Debug.Log("Эффект не загружен: " + pSEnum);
    //            yield break;
    //        }

    //        requestGO = (GameObject)request.asset;
    //        if (requestGO == null)
    //        {
    //            Debug.Log("Эффект не загружен: " + pSEnum);
    //            yield break;
    //        }
    //    }


    //    if (parentTransform != null)
    //    {
    //        //создаем эффект 
    //        GameObject psGO = GameObject.Instantiate(requestGO, parentTransform);
    //        //время жизни эффекта
    //        if (lifeTime != 0)
    //        {
    //            GameObject.Destroy(psGO, lifeTime);
    //        }
    //    }
    //}

    //public void CreatePSAsync(Transform parentTransform, PSEnum pSEnum, float lifeTime = 0)
    //{
    //    StartCoroutine(CurCreatePSAsync(parentTransform, pSEnum, lifeTime));
    //}

    public GameObject CreatePS(Transform parentTransform, PSEnum pSEnum, float lifeTime = 0)
    {
        //GameObject requestGO = ParticleSystemBank.GetPS(pSEnum);
        //if (requestGO == null)
        //{
        //    Debug.Log("Эффект не загружен: " + pSEnum);
        //    return null;
        //}

        //создаем эффект 
        //GameObject psGO = GameObject.Instantiate(requestGO, parentTransform);
        GameObject psGO = PoolManager.Instance.GetObjectToRent(pSEnum.ToString(), parentTransform.position, parentTransform, lifeTime);
        //время жизни эффекта
        //if (lifeTime != 0)
        //{
        //    GameObject.Destroy(psGO, lifeTime);
        //}

        return psGO;
    }

    public void CreateCollectAllEffect(Transform parentTransform, Image image)
    {
        //создаем эффект 
        GameObject psGO = CreatePS(parentTransform, PSEnum.PSCollectAll, 4);
        //изменяем цвет
        ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
        ps.textureSheetAnimation.SetSprite(0, image.sprite);
    }

    public void CreateCollectEffect(Transform parentTransform, Image image)
    {
        //создаем эффект 
        GameObject psGO = CreatePS(parentTransform, PSEnum.PSCollect, 4);
        //изменяем цвет
        ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
        ps.textureSheetAnimation.SetSprite(0, image.sprite);
    }

    public void CreateCollectAllEffect(Transform parentTransform, Sprite sprite)
    {
        //создаем эффект 
        GameObject psGO = CreatePS(parentTransform, PSEnum.PSCollectAll, 4);
        //изменяем цвет
        ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
        ps.textureSheetAnimation.SetSprite(0, sprite);
    }

    public void CreateCollectEffect(Transform parentTransform, Sprite sprite)
    {
        //создаем эффект 
        GameObject psGO = CreatePS(parentTransform, PSEnum.PSCollect, 4);
        //изменяем цвет
        ParticleSystem ps = psGO.GetComponent<ParticleSystem>();
        ps.textureSheetAnimation.SetSprite(0, sprite);
    }
}

