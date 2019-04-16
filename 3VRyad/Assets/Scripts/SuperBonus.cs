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

    private int strikesOnBlocks = 0;//количество ударов по блокам
    private int bonusPower = 0;
    private int charges = 0; //заряды супербонуса
    private bool activated = false;//активируется в текущий момент
    private bool activateSuperBonusOnEnd = false;
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
                            strikesOnBlocks++;
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

                //если ударили по всем блокам или ударили столько же раз сколько ракет в одном супер бонусе
                if (HitSuperBonusList.Count == 0 || strikesOnBlocks >= beats)
                {
                    GridBlocks.Instance.Move();
                    strikesOnBlocks = 0;
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

    private IEnumerator CurActivateSuperBonusOnEnd()
    {
        activateSuperBonusOnEnd = true;

            //запускаем супер бонус пока есть ходы
            while (Tasks.Instance.SubMoves())
            {
                charges++;
                //если супер бонус выполняет действие, то ожидаем
                do
                {
                    yield return new WaitForSeconds(0.2f);
                } while (activated);

                ActivateSuperBonus();
            }

            //если еще остались заряды, то и их используем
            while (charges > 0)
            {
                //если супер бонус выполняет действие, то ожидаем
                do
                {
                    yield return new WaitForSeconds(0.2f);
                } while (activated);

                ActivateSuperBonus();
            }

        //ожидаем, пока все ракеты не долетят до своих целей
        do
        {
            yield return new WaitForSeconds(0.01f);
        } while (HitSuperBonusList.Count > 0);

        activateSuperBonusOnEnd = false;
    }

    public bool ActivateSuperBonusOnEnd()
    {
        //если супер бонус разрешен на уровне  и остались доступные ходы, то конвертируем их в заряды супер бонуса
        //и возвращяем true до тех пор пока выполняем процедцрц конвертации и запуска всех ракет
        if (allow)
        {
            if (!activateSuperBonusOnEnd && (Tasks.Instance.Moves > 0 || charges > 0))
            {
                StartCoroutine(CurActivateSuperBonusOnEnd());
                return true;
            }
            else if (activateSuperBonusOnEnd)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void ActivateSuperBonus()
    {
        //активируем супер бонус если есть заряды и он разрешен на уровне
        if (allow && charges > 0 && !activated)
        {
            charges--;
            //activated = true;
            tankImage.fillAmount = 1;
            Block[] blocks = GridBlocks.Instance.GetAllBlocksWithStandartElements();

            if (blocks.Length > 1)
            {
                //перемешиваем
                SupportFunctions.MixArray(blocks);
                List<Block> blocksForWork = new List<Block>();
                for (int i = 0; i < beats; i++)
                {
                    if (blocks.Length > i)
                    {
                        //если блок сейчас не обрабатывается
                        if (!GridBlocks.Instance.BlockInProcessing(blocks[i]))
                        {
                            blocks[i].Blocked = true;//предварительно блокируем  
                            blocksForWork.Add(blocks[i]);
                        }
                    }
                }
                //запускаем дальнейшую обработку блоков
                StartCoroutine(CreatingEffects(blocksForWork));
            }
            //activated = false;
            FilledImage();
        }        
    }

    private IEnumerator CreatingEffects(List<Block> blocks) {

        activated = true;
        foreach (Block block in blocks)
        {
            //подсветка
            GameObject backlight = Instantiate(MainParticleSystem.Instance.pSSelect, block.transform);
            backlight.transform.position = block.transform.position;
            HitSuperBonusList.Add(new HitSuperBonus(backlight, CreateBeatsSuperBonus(block.transform), block));//добавляем в список для последующей обработки                        
            yield return new WaitForSeconds(0.1f);
        }
        activated = false;
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
