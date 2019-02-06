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
            if (Grid.Instance.ThisStandardBlockWithoutElement(destinationBlock) || Grid.Instance.ThisStandardBlockWithoutElement(touchingBlock))
            {
                //ищем блок куда переместили элемент в массиве
                blockToCreateBonus = findedBlockInLine.Find(item => item == destinationBlock);
                //если не нашли, ищем в том месте откуда переместили
                if (blockToCreateBonus == null)
                    blockToCreateBonus = findedBlockInLine.Find(item => item == touchingBlock);
            }

            //если не нашли, или блоки изначально == null, то определяем случайное место
            if (blockToCreateBonus == null)
            {
                for (int i = (findedBlockInLine.Count - 1); i >= 1; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    //int j = random.Next(i + 1);
                    // обменять значения data[j] и data[i]
                    var temp = findedBlockInLine[j];
                    findedBlockInLine[j] = findedBlockInLine[i];
                    findedBlockInLine[i] = temp;
                }

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
                //делаем анимацию перемещения
                foreach (Block item in findedBlockInLine)
                    MainAnimator.Instance.AddElementForSmoothMove(item.Element.thisTransform, blockToCreateBonus.thisTransform.position, 6);
                //выбираем случайный бонус и выдаем его
                int random = UnityEngine.Random.Range(0, findedBonus.Count);
                CreatBonus(findedBonus[random], blockToCreateBonus);
            }
        }
    }

    private void CreatBonus(Bonus bonus, Block blockToCreateBonus) {
        blockToCreateBonus.CreatElement(Grid.Instance.prefabElement, bonus.Shape, bonus.Type);
    }
}
