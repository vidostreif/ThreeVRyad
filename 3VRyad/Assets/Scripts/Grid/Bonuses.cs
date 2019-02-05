using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//класс для создания бонусов на игровом поле после хода игрока
public class Bonuses : MonoBehaviour {
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
            if (Grid.Instance.ThisStandardBlockWithoutElement(destinationBlock))
                blockToCreateBonus = destinationBlock;
            else if (Grid.Instance.ThisStandardBlockWithoutElement(touchingBlock))
                blockToCreateBonus = touchingBlock;
            else
            {
                //for (int i = (findedBlockInLine.Count - 1); i >= 1; i--)
                //{
                //    //int j = UnityEngine.Random.Range(0, i + 1);
                //    int j = random.Next(i + 1);
                //    // обменять значения data[j] и data[i]
                //    var temp = findedBlockInLine[j];
                //    findedBlockInLine[j] = findedBlockInLine[i];
                //    findedBlockInLine[i] = temp;
                //}

                foreach (Block item in findedBlockInLine)
                {
                    //создаем элемент в первом свободном блоке, если такой есть
                    if (Grid.Instance.ThisStandardBlockWithoutElement(item))
                    {
                        blockToCreateBonus = item;
                        break;
                    }
                }
            }
            if (blockToCreateBonus != null)
            {
                //выбираем случайный бонус и выдаем его
                int random = UnityEngine.Random.Range(0, findedBonus.Count);
                GiveBonus(findedBonus[random], blockToCreateBonus);
                //return true;
            }
        }
        //return false;
    }

    private void GiveBonus(Bonus bonus, Block blockToCreateBonus) {

        blockToCreateBonus.CreatElement(Grid.Instance.prefabElement, bonus.Shape, bonus.Type);

        //switch (bonus.Type)
        //{
        //    case BonusesEnum.Empty:
                
        //        break;
        //    case BonusesEnum.Bomb:
                
        //        break;
        //    case BonusesEnum.Wall:
                
        //        break;
        //    default:
        //        Debug.LogError("Неудалось определить тип бонуса!");
        //        break;
        //}
    }
}
