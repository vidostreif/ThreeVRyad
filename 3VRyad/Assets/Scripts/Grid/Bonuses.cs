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

    void Awake()
    {
        // регистрация синглтона
        if (Instance != null)
            Debug.LogError("Несколько экземпляров Bonuses!");

        Instance = this;
    }

    public void CheckBonuses(List<Block> findedBlockInLine, Block touchingBlock, Block destinationBlock) {

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
            if (GridBlocks.Instance.ThisStandardBlockWithoutElement(destinationBlock) || GridBlocks.Instance.ThisStandardBlockWithoutElement(touchingBlock))
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
                    if (GridBlocks.Instance.ThisStandardBlockWithoutElement(item))//если блок без элемента
                    {
                        NeighboringBlocks neighboringBlocks = GridBlocks.Instance.DeterminingNeighboringBlocks(GridBlocks.Instance.FindPosition(item));
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
                foreach (Block item in findedBlockInLine) {
                    MainAnimator.Instance.AddElementForSmoothMove(item.Element.thisTransform, blockToCreateBonus.thisTransform.position, 6, SmoothEnum.InLine,smoothTime: 0.2f , destroyAfterMoving: true);
                    //AnimatorElement animatorElement = item.Element.GetComponent<AnimatorElement>();
                    //animatorElement.StopDestroyAnimation();
                    item.Element = null;
                }

                //выбираем случайный бонус и выдаем его
                int random = UnityEngine.Random.Range(0, findedBonus.Count);
                CreatBonus(findedBonus[random], blockToCreateBonus);
            }
            else
            {
                Debug.Log("Не удалось создать бонус! 1" );
            }
        }
        else
        {
            Debug.Log("Не удалось создать бонус! 2");
        }
    }

    private void CreatBonus(Bonus bonus, Block blockToCreateBonus) {
        blockToCreateBonus.CreatElement(GridBlocks.Instance.prefabElement, bonus.Shape, bonus.Type);
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

        Debug.Log(bonusesXElement);

        return bonusesXElement;
    }

    public void RecoverFromXElement(XElement bonusesXElement)
    {
        //восстанавливаем значения
        bonusesList.Clear();
        foreach (XElement bonusesListXElement in bonusesXElement.Element("bonusesList").Elements("bonus"))
        {
            ElementsTypeEnum type = (ElementsTypeEnum)Enum.Parse(typeof(ElementsTypeEnum), bonusesListXElement.Attribute("type").Value);
            ElementsShapeEnum shape = (ElementsShapeEnum)Enum.Parse(typeof(ElementsShapeEnum), bonusesListXElement.Attribute("shape").Value);
            int cost = int.Parse(bonusesListXElement.Attribute("cost").Value);
            Bonus bonus = new Bonus(type, shape, cost);
            this.bonusesList.Add(bonus);
        }
    }
}
