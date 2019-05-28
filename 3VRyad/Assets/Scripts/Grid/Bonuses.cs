using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Xml.Linq;

//класс для создания бонусов на игровом поле после хода игрока
public class Bonuses : MonoBehaviour, IESaveAndLoad
{
    public static Bonuses Instance; // Синглтон
    public List<Bonus> bonusesList;

    private bool activateBonusOnEnd;

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
            Debug.LogError("Несколько экземпляров Bonuses!");

        Instance = this;
        activateBonusOnEnd = false;
    }

    public void CheckBonuses(List<Block> findedBlockInLine, Block touchingBlock, Block destinationBlock)
    {

        //ищем бонус подходящий под нашу длинну совпадающей линии
        int count = findedBlockInLine.Count;
        List<Bonus> findedBonus = new List<Bonus>();
        for (int i = 0; i < findedBlockInLine.Count; i++)
        {
            //ищем нужный бонус постепенно уменьшая стоимость
            findedBonus = bonusesList.FindAll(x => x.Cost == count - i);
            if (findedBonus.Count > 0)
                break;
        }

        if (findedBonus.Count > 0)
        {
            //определяем место выдачи бонуса
            Block blockToCreateBonus = null;
            if (BlockCheck.ThisStandardBlockWithoutElement(destinationBlock) || BlockCheck.ThisStandardBlockWithoutElement(touchingBlock))
            {
                //ищем блок куда переместили элемент в массиве
                blockToCreateBonus = findedBlockInLine.Find(item => item == destinationBlock);
                //если не нашли, ищем в том месте откуда переместили
                if (blockToCreateBonus == null)
                    blockToCreateBonus = findedBlockInLine.Find(item => item == touchingBlock);
            }

            //если не нашли, или блоки изначально == null, то определяем место на пересечении
            if (blockToCreateBonus == null)
            {
                //пробегаемся по всему массиву и находим блок на пересечении
                foreach (Block item in findedBlockInLine)
                {
                    if (BlockCheck.ThisStandardBlockWithoutElement(item))//если блок без элемента
                    {
                        NeighboringBlocks neighboringBlocks = GridBlocks.Instance.GetNeighboringBlocks(item.PositionInGrid);
                        List<Block> matchedBlocks = new List<Block>();
                        foreach (Block neighboringBlock in neighboringBlocks.allBlockField)
                        {
                            //ищем соседние блоки в основном массиве
                            if (findedBlockInLine.Contains(neighboringBlock))
                                matchedBlocks.Add(neighboringBlock);
                        }

                        //если нашли два блока по соседству, то проверяем как они расположены
                        if (matchedBlocks.Count == 2)
                        {
                            blockToCreateBonus = item;
                            //Если на против друг друга
                            if ((matchedBlocks[0] == neighboringBlocks.Left && matchedBlocks[1] == neighboringBlocks.Right) || (matchedBlocks[1] == neighboringBlocks.Left && matchedBlocks[0] == neighboringBlocks.Right) ||
                                (matchedBlocks[0] == neighboringBlocks.Up && matchedBlocks[1] == neighboringBlocks.Down) || (matchedBlocks[1] == neighboringBlocks.Up && matchedBlocks[0] == neighboringBlocks.Down))
                            {
                                continue;//продолжаем поиски
                            }
                            else
                            {
                                break;//иначе выбираем этот блок
                            }
                        }
                        else if (matchedBlocks.Count > 2)//если больше двух 
                        {
                            blockToCreateBonus = item;
                            break;//выбираем этот блок
                        }
                    }
                }

            }
            if (blockToCreateBonus != null)
            {
                //делаем анимацию перемещения уничтоженных элементов
                foreach (Block item in findedBlockInLine)
                {
                    if (item.Element != null)
                    {
                        MainAnimator.Instance.AddElementForSmoothMove(item.Element.thisTransform, blockToCreateBonus.thisTransform.position, 6, SmoothEnum.InLineWithSlowdown, smoothTime: 0.1f, destroyAfterMoving: true);
                        //AnimatorElement animatorElement = item.Element.GetComponent<AnimatorElement>();
                        //animatorElement.StopDestroyAnimation();
                        item.Element = null;
                    }                    
                }

                //выбираем случайный бонус и выдаем его
                int random = UnityEngine.Random.Range(0, findedBonus.Count);
                CreatBonus(findedBonus[random], blockToCreateBonus);
            }
            else
            {
                //Debug.Log("Не удалось создать бонус! 1");
            }
        }
        else
        {
            //Debug.Log("Не удалось создать бонус! 2");
        }
    }

    private void CreatBonus(Bonus bonus, Block blockToCreateBonus)
    {
        blockToCreateBonus.CreatElement(GridBlocks.Instance.prefabElement, bonus.Shape, bonus.Type);
    }

    private IEnumerator CurActivateBonusOnEnd(List<Block> blocks)
    {
        activateBonusOnEnd = true;

        foreach (Block block in blocks)
        {
            block.Hit();            
            yield return new WaitForSeconds(0.1f);
        }
        GridBlocks.Instance.Move();
        activateBonusOnEnd = false;
    }

    public bool ActivateBonusOnEnd()
    {
        //если на поле остались бонусы то находим их и активируем
        if (!activateBonusOnEnd)
        {
            List<Block> blocks = GetAllBlockWithBonus();

            if (blocks.Count > 0)
            {
                StartCoroutine(CurActivateBonusOnEnd(blocks));
                return true;
            }
            else
            {
                return false;
            }

        }
        else
        {
            return true;
        }
    }

    //получаем все блоки с бонусами
    public List<Block> GetAllBlockWithBonus() {

        List<Block> blocks = new List<Block>();
        Block[] curBlocks;
        foreach (Bonus bonus in bonusesList)
        {
            curBlocks = GridBlocks.Instance.GetAllBlocksWithCurElements(bonus.Type);
            foreach (Block curBlock in curBlocks)
            {
                blocks.Add(curBlock);
            }
        }
        return blocks;
    }

    //сохранение и заргрузка
    //передаем данные о на стройках в xml формате
    public Type GetClassName()
    {
        return this.GetType();
    }


    public XElement GetXElement()
    {
        XElement bonusesXElement = new XElement(this.GetType().ToString());

        //записываем все внешности и количество
        XElement bonusesListXElement = new XElement("bonusesList");
        foreach (Bonus bonus in bonusesList)
        {
            XAttribute type = new XAttribute("type", bonus.Type);
            XAttribute shape = new XAttribute("shape", bonus.Shape);
            XAttribute cost = new XAttribute("cost", bonus.Cost);
            XElement bonusXElement = new XElement("bonus", shape, type, cost);
            bonusesListXElement.Add(bonusXElement);
        }
        bonusesXElement.Add(bonusesListXElement);

        return bonusesXElement;
    }

    public void RecoverFromXElement(XElement bonusesXElement)
    {
        //восстанавливаем значения
        bonusesList.Clear();
        foreach (XElement bonusesListXElement in bonusesXElement.Element("bonusesList").Elements("bonus"))
        {
            ElementsTypeEnum type = (ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), bonusesListXElement.Attribute("type").Value);
            AllShapeEnum shape = (AllShapeEnum)Enum.Parse(typeof(AllShapeEnum), bonusesListXElement.Attribute("shape").Value);
            int cost = int.Parse(bonusesListXElement.Attribute("cost").Value);
            Bonus bonus = new Bonus(type, shape, cost);
            this.bonusesList.Add(bonus);
        }

#if UNITY_EDITOR
        //если не в игре, то показываем
        if (!Application.isPlaying)
        {
            string parentName = "Bonuses";
            GameObject parentGO = GameObject.Find(parentName);
            if (parentGO != null)
            {
                DestroyImmediate(parentGO);
            }
            parentGO = new GameObject();
            parentGO.name = parentName;
            GameObject canvasGame = GameObject.Find("GameHelper");
            parentGO.transform.SetParent(canvasGame.transform, false);
            parentGO.transform.localPosition = new Vector3(-canvasGame.GetComponent<RectTransform>().rect.width / 2, canvasGame.GetComponent<RectTransform>().rect.height / 2);

            int i = 0;
            foreach (Bonus item in bonusesList)
            {
                GameObject imageElementAndParameters = Instantiate(Resources.Load("Prefabs/Canvas/GameCanvas/ImageElementAndParameters") as GameObject, parentGO.transform);
                imageElementAndParameters.transform.position = new Vector3(imageElementAndParameters.transform.position.x + i * 3, imageElementAndParameters.transform.position.y + 3, 0);
                Image image = imageElementAndParameters.GetComponent(typeof(Image)) as Image;
                image.sprite = SpriteBank.SetShape(item.Shape);
                Text Text = imageElementAndParameters.GetComponentInChildren<Text>();
                Text.text = "Тип: " + item.Type + "\n Цена: " + item.Cost;
                i++;
            }
        }
#endif
    }
}
