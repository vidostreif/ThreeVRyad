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
            //берем только те элементы, которые можно создавать
            if (elementsShapeAndPriority[i].limitOnAmountCreated > 0 && elementsShapeAndPriority[i].maxAmountOnField > ElementsList.GetAmountOfThisShapeElemets(elementsShapeAndPriority[i].ElementsShape))
            {
                //складываем все приоритеты
                poolSize += elementsShapeAndPriority[i].Priority;
            }            
        }

        //если не осталось элементов для заполнения, сбрасывает приоритеты
        if (poolSize == 0)
        {
            //Debug.Log("Сброс приоритетов!");
            foreach (ElementsPriority elementsPriorityItem in elementsShapeAndPriority)
            {
                elementsPriorityItem.ResetPriority();
            }

            for (int i = 0; i < elementsShapeAndPriority.Count; i++)
            {
                //берем только те элементы, которые можно создавать
                if (elementsShapeAndPriority[i].limitOnAmountCreated > 0 && elementsShapeAndPriority[i].maxAmountOnField > ElementsList.GetAmountOfThisShapeElemets(elementsShapeAndPriority[i].ElementsShape))
                {
                    //складываем все приоритеты
                    poolSize += elementsShapeAndPriority[i].Priority;
                }
            }
        }

        // Get a random integer from 0 to PoolSize.
        int randomNumber = UnityEngine.Random.Range(0, poolSize);

        // Определяем элемент
        int accumulatedProbability = 0;
        for (int i = 0; i < elementsShapeAndPriority.Count; i++)
        {
            //берем только те элементы, которые можно создавать
            if (elementsShapeAndPriority[i].limitOnAmountCreated > 0 && elementsShapeAndPriority[i].maxAmountOnField > ElementsList.GetAmountOfThisShapeElemets(elementsShapeAndPriority[i].ElementsShape))
            {
                accumulatedProbability += elementsShapeAndPriority[i].Priority;
                if (randomNumber < accumulatedProbability)
                {
                    elementsShapeAndPriority[i].Priority--;
                    return elementsShapeAndPriority[i];
                }                    
            }
        }
        return null; 
    }

    public static ElementsPriority SelectStandartElement(List<ElementsPriority> elementsShapeAndPriority)
    {
        // Суммируем все приоритеты
        int poolSize = 0;
        for (int i = 0; i < elementsShapeAndPriority.Count; i++)
        {
            //берем только те элементы, которые можно создавать
            if (elementsShapeAndPriority[i].elementsType == ElementsTypeEnum.StandardElement && elementsShapeAndPriority[i].limitOnAmountCreated > 0 && elementsShapeAndPriority[i].maxAmountOnField > ElementsList.GetAmountOfThisShapeElemets(elementsShapeAndPriority[i].ElementsShape))
            {
                //складываем все приоритеты
                poolSize += elementsShapeAndPriority[i].Priority;
            }
        }

        //если не осталось элементов для заполнения, сбрасывает приоритеты
        if (poolSize == 0)
        {
            //Debug.Log("Сброс приоритетов!");
            foreach (ElementsPriority elementsPriorityItem in elementsShapeAndPriority)
            {
                if (elementsPriorityItem.elementsType == ElementsTypeEnum.StandardElement)
                {
                    elementsPriorityItem.ResetPriority();
                }                
            }

            for (int i = 0; i < elementsShapeAndPriority.Count; i++)
            {
                //берем только те элементы, которые можно создавать
                if (elementsShapeAndPriority[i].elementsType == ElementsTypeEnum.StandardElement && elementsShapeAndPriority[i].limitOnAmountCreated > 0 && elementsShapeAndPriority[i].maxAmountOnField > ElementsList.GetAmountOfThisShapeElemets(elementsShapeAndPriority[i].ElementsShape))
                {
                    //складываем все приоритеты
                    poolSize += elementsShapeAndPriority[i].Priority;
                }
            }
        }

        // Get a random integer from 0 to PoolSize.
        int randomNumber = UnityEngine.Random.Range(0, poolSize);

        // Определяем элемент
        int accumulatedProbability = 0;
        for (int i = 0; i < elementsShapeAndPriority.Count; i++)
        {
            //берем только те элементы, которые можно создавать
            if (elementsShapeAndPriority[i].elementsType == ElementsTypeEnum.StandardElement && elementsShapeAndPriority[i].limitOnAmountCreated > 0 && elementsShapeAndPriority[i].maxAmountOnField > ElementsList.GetAmountOfThisShapeElemets(elementsShapeAndPriority[i].ElementsShape))
            {
                accumulatedProbability += elementsShapeAndPriority[i].Priority;
                if (randomNumber < accumulatedProbability)
                {
                    elementsShapeAndPriority[i].Priority--;
                    return elementsShapeAndPriority[i];
                }
            }
        }
        return null;
    }
}
