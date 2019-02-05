using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProportionalWheelSelection
{
    //public static Random rnd = new Random();

    // Static method for using from anywhere. You can make its overload for accepting not only List, but arrays also: 
    // функция поиска элемента в соответствии с указанным приоритетом
    public static ElementsPriority SelectElement(List<ElementsPriority> elementsShapeAndPriority)
    {
        // Суммируем все приоритеты
        int poolSize = 0;
        for (int i = 0; i < elementsShapeAndPriority.Count; i++)
        {
            poolSize += elementsShapeAndPriority[i].priority;
        }

        // Get a random integer from 0 to PoolSize.
        int randomNumber = UnityEngine.Random.Range(0, poolSize) + 1;

        // Определяем элемент
        int accumulatedProbability = 0;
        for (int i = 0; i < elementsShapeAndPriority.Count; i++)
        {
            accumulatedProbability += elementsShapeAndPriority[i].priority;
            if (randomNumber <= accumulatedProbability)
                return elementsShapeAndPriority[i];
        }
        return null; 
    }
}
