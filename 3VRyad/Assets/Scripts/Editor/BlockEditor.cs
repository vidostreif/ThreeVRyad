﻿#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Block), true)]
public class BlockEditor : Editor
{

    Block block;
    GridBlocks grid;

    void OnEnable() { EditorApplication.update += Update;
        block = (Block)target;
        grid = GameObject.Find("Grid").GetComponent<GridBlocks>();
        block.thisTransform = block.transform;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        EditorGUILayout.LabelField("Настройка блока:", EditorStyles.boldLabel);
        block.Type = (BlockTypeEnum)EditorGUILayout.EnumPopup("Тип блока", block.Type);

        if (block.Type == BlockTypeEnum.Standard) {
            block.GeneratorElements = EditorGUILayout.Toggle("Генератор элементов", block.GeneratorElements);

            //условия для указания элемента на заднем фоне, иначе элемент удаляем
            EditorGUILayout.LabelField("Настройка элемента на заднем плане:", EditorStyles.boldLabel);
            BehindElementsTypeEnum BehindelementType;//тип элемента            
            BehindElementsShapeEnum BehindelementShape;//внешний вид элемента
            if (block.BehindElement == null)
            {
                BehindelementType = BehindElementsTypeEnum.Empty;
                BehindelementShape = BehindElementsShapeEnum.Empty;
            }
            else
            {
                BehindelementType = block.BehindElement.Type;
                BehindelementShape = block.BehindElement.Shape;
            }
            BehindelementType = (BehindElementsTypeEnum)EditorGUILayout.EnumPopup("  Тип элемента", BehindelementType);
            BehindelementShape = (BehindElementsShapeEnum)EditorGUILayout.EnumPopup("  Внешность элемента", BehindelementShape);

            //создаем элемент если нужно
            if (BehindelementType != BehindElementsTypeEnum.Empty && block.BehindElement == null)
            {
                //подгрузить префаб из настроек сетки
                block.CreatBehindElement(grid.prefabElement, BehindelementShape, BehindelementType);
            }
            else if (BehindelementType == BehindElementsTypeEnum.Empty && block.BehindElement != null)
            {
                //удаляем элемент
                DestroyImmediate(block.BehindElement.gameObject);
            }
            //пересоздаем новый элемент
            else if (BehindelementType != BehindElementsTypeEnum.Empty && block.BehindElement != null && block.BehindElement.Type != BehindelementType)
            {
                //удаляем элемент
                DestroyImmediate(block.BehindElement.gameObject);
                //подгрузить префаб из настроек сетки
                block.CreatBehindElement(grid.prefabElement, BehindelementShape, BehindelementType);
            }

            if (block.BehindElement != null && block.BehindElement.Shape != BehindelementShape)
            {
                block.BehindElement.Shape = BehindelementShape;
            }

            //условия для указания элемента в блоке, иначе элемент удаляем
            EditorGUILayout.LabelField("Настройка элемента:", EditorStyles.boldLabel);
            ElementsTypeEnum elementType;//тип элемента            
            ElementsShapeEnum elementShape;//внешний вид элемента
            if (block.Element == null) {
                elementType = ElementsTypeEnum.Empty;
                elementShape = ElementsShapeEnum.Empty;
            }
            else {
                elementType = block.Element.Type;
                elementShape = block.Element.Shape;
            }                
            elementType = (ElementsTypeEnum)EditorGUILayout.EnumPopup("  Тип элемента", elementType);      
            elementShape = (ElementsShapeEnum)EditorGUILayout.EnumPopup("  Внешность элемента", elementShape);        
            
            //создаем элемент если нужно
            if (elementType != ElementsTypeEnum.Empty && block.Element == null) {
                //подгрузить префаб из настроек сетки
                block.CreatElement(grid.prefabElement, elementShape, elementType);
            }
            else if (elementType == ElementsTypeEnum.Empty && block.Element != null)
            {
                //удаляем элемент
                DestroyImmediate(block.Element.gameObject);
            }
            //пересоздаем новый элемент
            else if (elementType != ElementsTypeEnum.Empty && block.Element != null && block.Element.Type != elementType) {
                //удаляем элемент
                DestroyImmediate(block.Element.gameObject);
                //подгрузить префаб из настроек сетки
                block.CreatElement(grid.prefabElement, elementShape, elementType);
            }

            if (block.Element != null && block.Element.Shape != elementShape) {
                block.Element.Shape = elementShape;
            }
                       
            //условия для указания блокирующего элемента, иначе элемент удаляем
            if (elementType == ElementsTypeEnum.Standard && block.Element != null) {
                EditorGUILayout.LabelField("Настройка блокирующего элемента:", EditorStyles.boldLabel);

                Element element = block.Element;                
                BlockingElementsTypeEnum blockingElementType;//тип блокирующего элемента
                BlockingElementsShapeEnum blockingElementShape;//внешний вид элемента
                if (element.BlockingElement == null) {
                    blockingElementType = BlockingElementsTypeEnum.Empty;
                    blockingElementShape = BlockingElementsShapeEnum.Empty;
                }                    
                else {
                    blockingElementType = element.BlockingElement.Type;
                    blockingElementShape = element.BlockingElement.Shape;
                }
                blockingElementType = (BlockingElementsTypeEnum)EditorGUILayout.EnumPopup("  Тип блокирующего элемента", blockingElementType);
                blockingElementShape = (BlockingElementsShapeEnum)EditorGUILayout.EnumPopup("  Внешность блокирующего элемента", blockingElementShape);

                //создаем элемент если нужно
                if (blockingElementType != BlockingElementsTypeEnum.Empty && element.BlockingElement == null) {
                    //подгрузить префаб из настроек сетки
                    element.CreatBlockingElement(grid.prefabBlockingWall, blockingElementShape, blockingElementType);
                }
                else if (blockingElementType == BlockingElementsTypeEnum.Empty && element.BlockingElement != null) {
                    //удаляем элемент
                    DestroyImmediate(element.BlockingElement.gameObject);
                }
                //пересоздаем новый элемент
                else if (blockingElementType != BlockingElementsTypeEnum.Empty && element.BlockingElement != null && element.BlockingElement.Type != blockingElementType) {
                    //удаляем элемент
                    DestroyImmediate(element.BlockingElement.gameObject);
                    //подгрузить префаб из настроек сетки
                    element.CreatBlockingElement(grid.prefabBlockingWall, blockingElementShape, blockingElementType);
                }

                if (element.BlockingElement != null && element.BlockingElement.Shape != blockingElementShape) {
                    element.BlockingElement.Shape = blockingElementShape;
                }
            }
            else if (block.Element != null && block.Element.Type == ElementsTypeEnum.Empty && block.Element.BlockingElement != null) {
                //удаляем элемент
                DestroyImmediate(block.Element.BlockingElement.gameObject);
            }

        }
        else if (block.Type == BlockTypeEnum.Empty && block.Element != null) {
            //удаляем элемент
            DestroyImmediate(block.Element.gameObject);
            //удаляем элемент на заднем фоне
            DestroyImmediate(block.BehindElement.gameObject);
        }

        EditorUtility.SetDirty(block);
        if (block.BehindElement != null)
        {
            BehindElement behindElement = block.BehindElement.GetComponent<BehindElement>();
            EditorUtility.SetDirty(behindElement);
        }
        if (block.Element != null) {
            Element element = block.Element.GetComponent<Element>();
            EditorUtility.SetDirty(element);
            if (block.Element.BlockingElement != null) {
                BlockingElement blockingElement = block.Element.GetComponent<BlockingElement>();
                EditorUtility.SetDirty(blockingElement);
            }
        }
        //Применение изменений
        //serializedObject.ApplyModifiedProperties();
    }


        void Update()
    {
    //    //block = (BlockField)target;

    //    //BlocksEnum blocksType = block.Type;
    //    //block.Type = blocksType;
    }
}

#endif
