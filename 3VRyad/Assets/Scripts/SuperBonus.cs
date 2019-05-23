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
    private bool activated;//активируется в текущий момент
    private bool activateSuperBonusOnEnd;
    private ParticleSystem.MainModule mmSuperBonus;
    private float psSuperBonusMaxSpeed;//максимальная скорость частиц в эффекте
    private ParticleSystem.EmissionModule eMSuperBonus;
    private float eMSuperBonusMaxRateOverTime;//максимальное количество частиц создаваемое эффектом в единицу времени
    private Image superBonusPowerImage;

    private List<HitSuperBonus> HitSuperBonusList;
    private List<HitSuperBonus> HitSuperBonusListForDelete;

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
        {
            Debug.LogError("Несколько экземпляров SuperBonus!");
        }

        Instance = this;
        activated = false;//активируется в текущий момент
        activateSuperBonusOnEnd = false;
        HitSuperBonusList = new List<HitSuperBonus>();
        HitSuperBonusListForDelete = new List<HitSuperBonus>();
        superBonusPowerImage = transform.Find("SuperBonusPowerImage").GetComponent(typeof(Image)) as Image;
        ParticleSystem psSuperBonus = transform.GetComponentInChildren<ParticleSystem>();
        mmSuperBonus = psSuperBonus.main;
        psSuperBonusMaxSpeed = mmSuperBonus.startSpeed.constant;
        eMSuperBonus = psSuperBonus.emission;
        eMSuperBonusMaxRateOverTime = eMSuperBonus.rateOverTime.constant;
        FilledImage();
    }

    private void Update()
    {
        //обрабатываем только если супер бонус разрешен и не показывается подсказка в текущий момент
        if (allow && !HelpToPlayer.HelpActive())
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
                            MainAnimator.Instance.AddExplosionEffect(item.Block.thisTransform.position, 0.5f);
                            int randomNumber = UnityEngine.Random.Range(1, 5);
                            //Debug.Log(randomNumber);                            
                            SoundManager.Instance.PlaySoundInternal((SoundsEnum)Enum.Parse(typeof(SoundsEnum), "Boom_mini_" + randomNumber.ToString()));
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
        StopAllCoroutines();
        activated = false;
        activateSuperBonusOnEnd = false;
        strikesOnBlocks = 0;
        bonusPower = 0;
        charges = 0;
        HitSuperBonusList = new List<HitSuperBonus>();
        HitSuperBonusListForDelete = new List<HitSuperBonus>();
        FilledImage();        
    }
    //эффек добавления в чан
    public void CreatePowerSuperBonus(Vector3 position, int power)
    {
        //если разрешен и не конец игры
        if (allow && !Tasks.Instance.endGame)
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
        
        if (maxBonusPower != 0 && allow && eMSuperBonus.enabled)
        {
            if (charges > 0 || activated)
            {
                eMSuperBonus.rateOverTime = eMSuperBonusMaxRateOverTime;
                mmSuperBonus.startSpeed = psSuperBonusMaxSpeed;
                superBonusPowerImage.fillAmount = 1;
            }
            else
            {
                float fill = (float)bonusPower / (float)maxBonusPower;
                eMSuperBonus.rateOverTime = fill * eMSuperBonusMaxRateOverTime;
                mmSuperBonus.startSpeed = fill * psSuperBonusMaxSpeed;
                superBonusPowerImage.fillAmount = fill;
            }            
        }        
    }

    private IEnumerator CurActivateSuperBonusOnEnd()
    {
        activateSuperBonusOnEnd = true;
        GameObject movesText = GameObject.Find("movesText");
            //запускаем супер бонус пока есть ходы
            while (Tasks.Instance.SubMoves())
            {
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.Repainting_ring);
            GameObject psAddSuperBonusFromLevels = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSAddSuperBonusFromLevels") as GameObject, movesText.transform);
            MainAnimator.Instance.AddElementForSmoothMove(psAddSuperBonusFromLevels.transform, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), 1, SmoothEnum.InArc, smoothTime: 0.1f, destroyAfterMoving: false);
            Destroy(psAddSuperBonusFromLevels, 5);
            charges++;
            //если супер бонус выполняет действие, то ожидаем
            yield return new WaitForSeconds(0.4f);
            while (activated)
            {
                    yield return new WaitForSeconds(0.2f);
            }

            ActivateSuperBonus();
        }

            ////если еще остались заряды, то и их используем
            //while (charges > 0)
            //{
            //    //если супер бонус выполняет действие, то ожидаем
            //    do
            //    {
            //        yield return new WaitForSeconds(0.2f);
            //    } while (activated);

            //    ActivateSuperBonus();
            //}

        //ожидаем, пока все ракеты не долетят до своих целей
        do
        {
            yield return new WaitForSeconds(0.01f);
        } while (HitSuperBonusList.Count > 0);

        //визуально деактивируем
        bonusPower = 0;
        FilledImage();

        activateSuperBonusOnEnd = false;
    }

    public bool ActivateSuperBonusOnEnd()
    {
        //если супер бонус разрешен на уровне  и остались доступные ходы, то конвертируем их в заряды супер бонуса
        //и возвращяем true до тех пор пока выполняем процедцрц конвертации и запуска всех ракет
        if (allow)
        {
            if (!activateSuperBonusOnEnd && (Tasks.Instance.Moves > 0 ))
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
            
            //eMSuperBonus.rateOverTime = 1000;
            //superBonusPowerImage.fillAmount = 1;
            Block[] blocks = GridBlocks.Instance.GetAllBlocksWithStandartElements();
            //получаем все возможные блоки для следующего хода
            List<ElementsForNextMove> elementsForNextMove = GridBlocks.Instance.CheckElementsForNextMove();

            if (blocks.Length > 1)
            {
                //перемешиваем
                SupportFunctions.MixArray(blocks);
                List<Block> blocksForWork = new List<Block>();
                int foundedBlock = 0;
                int iteration = 0;
                //ищем блоки для удара, пока не найдем нужное количество
                do
                {
                    if (blocks.Length > iteration)
                    {
                        //если блок сейчас не обрабатывается
                        if (!GridBlocks.Instance.BlockInProcessing(blocks[iteration]))
                        {
                            //и его нет в списке для следующего хода
                            bool found = false;
                            foreach (ElementsForNextMove item in elementsForNextMove)
                            {
                                foreach (Element element in item.elementsList)
                                {
                                    if (element != null && GridBlocks.Instance.GetBlock(element) == blocks[iteration])
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (found)
                                {
                                    break;
                                }
                            }

                            if (!found)
                            {
                                blocks[iteration].Blocked = true;//предварительно блокируем  
                                blocksForWork.Add(blocks[iteration]);
                                foundedBlock++;
                            }
                        }
                    }
                    else
                    {
                        break;
                    }

                    iteration++;
                } while (foundedBlock < beats);
                    
                
                //запускаем дальнейшую обработку блоков
                StartCoroutine(CreatingEffects(blocksForWork));
            }            
        }        
    }

    private IEnumerator CreatingEffects(List<Block> blocks) {

        activated = true;
        FilledImage();

        SoundManager.Instance.PlaySoundInternal(SoundsEnum.SuperBonusActiveted);
        GameObject psSuperBonusActiveted = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSSuperBonusActiveted") as GameObject, transform);
        Destroy(psSuperBonusActiveted, 5);
        yield return new WaitForSeconds(0.2f);

        foreach (Block block in blocks)
        {
            //если сбросили параметры, то останавливаем работу
            if (!activated)
            {
                break;
            }
            //подсветка
            //GameObject backlight = Instantiate(MainParticleSystem.Instance.pSSelect, block.transform);
            SoundManager.Instance.PlaySoundInternal(SoundsEnum.SuperBonusRocket);
            GameObject backlight = GameObject.Instantiate(Resources.Load("Prefabs/ParticleSystem/PSSelectTargetBlock") as GameObject, block.transform);
            backlight.transform.position = block.transform.position;
            HitSuperBonusList.Add(new HitSuperBonus(backlight, CreateBeatsSuperBonus(block.transform), block));//добавляем в список для последующей обработки 
            float randomNumber = UnityEngine.Random.Range(0.2f, 0.4f);
            yield return new WaitForSeconds(randomNumber);
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
