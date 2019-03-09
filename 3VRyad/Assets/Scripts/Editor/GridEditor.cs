#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridBlocks))]
public class GridEditor : Editor
{
    GridBlocks grid;

    void OnEnable() {
        //EditorApplication.update += Update;
        grid = (GridBlocks)target;
        grid.thisTransform = grid.transform;
    }

    //[MenuItem("Kilosoft/Grid")]
    //public static void Init()
    //{
    //    new GameObject("Grid By Kilosoft", typeof(Grid));
    //}


    //void Awake()
    //{
    //    //grid = GameObject.Find("Grid").GetComponent<Grid>();
    //}

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        grid = (GridBlocks)target;
        if (grid != null)
        {
            //работаем только с объектом у которого есть BlockField
            foreach (var obj in Selection.GetFiltered(typeof(Block), SelectionMode.Assets))
            {
                //записать данные о позиции объекта
                Vector3 pos = (obj as Block).transform.position;
                Vector3 posGrid = grid.thisTransform.position;
                pos.x = Mathf.CeilToInt(pos.x / grid.blockSize) * grid.blockSize + (float)(posGrid.x - Math.Truncate(posGrid.x));
                pos.y = Mathf.CeilToInt(pos.y / grid.blockSize) * grid.blockSize + (float)(posGrid.y - Math.Truncate(posGrid.y));
                pos.z = posGrid.z;


                //выполняем если изменилась позиция
                if (pos != (obj as Block).transform.position)
                {
                    //Определяем на какой позиции он должен быть 
                    Position newPosition;
                    //Vector3 posAfterAdaptation = (obj as BlockField).transform.position;
                    newPosition.posX = (int)(pos.x - posGrid.x);
                    newPosition.posY = (int)(pos.y - posGrid.y);

                    //если позиция не занята в сетке другим блоком
                    if (grid.GetBlock(newPosition) == null || grid.GetBlock(newPosition) == (obj as Block))
                    {
                        //добавляем блок в сетку
                        grid.AddBlockToPosition(obj as Block, newPosition);
                        //если блок есть в сетке, то двигаем его по ней
                        if (grid.SearchBlock(obj as Block))
                        {
                            (obj as Block).transform.position = pos;
                        }

                    }


                }
            }            
        }
        
    }
}
#endif
