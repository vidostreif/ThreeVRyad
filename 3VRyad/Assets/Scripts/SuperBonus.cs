using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SuperBonus : MonoBehaviour, IESaveAndLoad
{
    public static SuperBonus Instance; // Синглтон
    public bool allow;//разрешен на уровне
    public int maxBonusPower;//мощность для набора одного заряда
    public int beats;//количество ударов в одном заряде
    private int bonusPower = 0;
    private int charges = 0; //заряды супербонуса
    private bool activated = false;//активируетс в текущий момент
    private Image tankImage;
    private List<HitSuperBonus> HitSuperBonusList = new List<HitSuperBonus>();
    private List<HitSuperBonus> HitSuperBonusListForDelete = new List<HitSuperBonus>();

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров SuperBonus!");
        }

        Instance = this;
        tankImage = transform.GetComponent(typeof(Image)) as Image;
        FilledImage();
    }

    private void Update()
    {
        if (allow)
        {
            //активируем супер бонус если есть заряды и еще не закончили игру
            if (charges > 0 && !activated && !Tasks.Instance.endGame)
            {
                StartCoroutine(ActivateSuperBonus());
            }

            //обрабатываем ракеты которые долетели до нужного элемента
            if (HitSuperBonusList.Count > 0)
            {
                foreach (HitSuperBonus item in HitSuperBonusList)
                {
                    if (item.gameObjectBeat == null)
                    {
                        if (item.Block != null)
                        {
                            item.Block.Hit();
                            item.Block.Blocked = false;
                            Destroy(item.backlight, 0.1f);
                        }
                        HitSuperBonusListForDelete.Add(item);
                    }
                }

                foreach (HitSuperBonus item in HitSuperBonusListForDelete)
                {
                    HitSuperBonusList.Remove(item);
                }
                HitSuperBonusListForDelete.Clear();

                //если ударили по всем блокам
                if (HitSuperBonusList.Count == 0)
                {
                    GridBlocks.Instance.Move();
                }
            }
        }
    }

    public void ResetParameters()
    {
        //Сбрасываем значения
        bonusPower = 0;
        charges = 0;        
        FilledImage();
    }
    //эффек добавления в чан
    public void CreatePowerSuperBonus(Vector3 position, int power)
    {
        if (allow)
        {
            GameObject powerSuperBonus = new GameObject();
            powerSuperBonus.transform.parent = transform;
            GameObject element = Instantiate(MainParticleSystem.Instance.pSAddPowerSuperBonus, powerSuperBonus.transform);
            powerSuperBonus.transform.position = position;
            MainAnimator.Instance.AddElementForSmoothMove(powerSuperBonus.transform, transform.position, 1, SmoothEnum.InLineWithAcceleration, smoothTime: 0.7f, destroyAfterMoving: true);
            AddBonusPower(power);
        }        
    }

    //ракета
    private GameObject CreateBeatsSuperBonus(Transform targetTransform)
    {
        GameObject beatsSuperBonus = new GameObject();
        beatsSuperBonus.transform.parent = targetTransform;
        GameObject element = Instantiate(MainParticleSystem.Instance.pSBeatsSuperBonus, beatsSuperBonus.transform);
        beatsSuperBonus.transform.position = transform.position;
        MainAnimator.Instance.AddElementForSmoothMove(beatsSuperBonus.transform, targetTransform.position, 1, SmoothEnum.InArc, smoothTime: 0.1f, destroyAfterMoving: true);
        return beatsSuperBonus;
    }

    //увеличение заполненности бонуса
    private void AddBonusPower(int power)
    {
        if ((bonusPower + power) < maxBonusPower)
        {
            this.bonusPower += power;            
        }
        else
        {
            this.bonusPower = bonusPower + power - maxBonusPower;
            charges++;
        }
        FilledImage();
    }

    //заполнение картинки, в соответсвии с заполнением бонуса
    private void FilledImage() {


        if (maxBonusPower != 0 && tankImage != null)
        {
            tankImage.fillAmount = (float)bonusPower / (float)maxBonusPower;
        }        
    }

    private IEnumerator ActivateSuperBonus()
    {
        charges--;
        activated = true;
        tankImage.fillAmount = 1;
        Block[] blocks = GridBlocks.Instance.GetAllBlocksWithStandartElements();

        if (blocks.Length > 1)
        {
            //перемешиваем
            SupportFunctions.MixArray(blocks);
            for (int i = 0; i < beats; i++)
            {
                if (blocks.Length > i)
                {
                    //если блок сейчас не обрабатывается
                    if (!GridBlocks.Instance.BlockInProcessing(blocks[i]))
                    {
                        blocks[i].Blocked = true;//блокируем  
                        //подсветка
                        GameObject backlight = Instantiate(MainParticleSystem.Instance.pSSelect, blocks[i].transform);
                        backlight.transform.position = blocks[i].transform.position;
                        HitSuperBonusList.Add(new HitSuperBonus(backlight, CreateBeatsSuperBonus(blocks[i].transform), blocks[i]));//добавляем в список для последующей обработки                        
                        yield return new WaitForSeconds(0.1f);
                    }                    
                }
            }            
        }
        activated = false;
        FilledImage();
    }


    //сохранение и заргрузка
    public Type GetClassName()
    {
        return this.GetType();
    }
    //передаем данные о на стройках в xml формате
    public XElement GetXElement()
    {
        XElement XElement = new XElement(this.GetType().ToString());
        XElement.Add(new XElement("maxBonusPower", maxBonusPower));
        XElement.Add(new XElement("beats", beats));
        XElement.Add(new XElement("allow", allow));
        return XElement;
    }

    public void RecoverFromXElement(XElement tasksXElement)
    {
        //восстанавливаем значения
        this.maxBonusPower = int.Parse(tasksXElement.Element("maxBonusPower").Value);
        this.beats = int.Parse(tasksXElement.Element("beats").Value);
        try { this.allow = bool.Parse(tasksXElement.Element("allow").Value); } catch (Exception) { }        
        ResetParameters();
    }
}

public class HitSuperBonus {
    public GameObject gameObjectBeat;
    public GameObject backlight;
    public Block Block;

    public HitSuperBonus(GameObject backlight, GameObject gameObjectBeat, Block block)
    {
        this.backlight = backlight;
        this.gameObjectBeat = gameObjectBeat;
        Block = block;
    }
}
