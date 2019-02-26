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
        EditorApplication.update += Update;
        grid = (GridBlocks)target;
        grid.thisTransform = grid.transform;
    }

    //[MenuItem("Kilosoft/Grid")]
    //public static void Init()
    //{
    //    new GameObject("Grid By Kilosoft", typeof(Grid));
    //}

    //public override void OnInspectorGUI()
    //{
    //    grid = (GridBlocks)target;

    //    EditorGUILayout.LabelField("Настройка шага:", EditorStyles.boldLabel);
    //    grid.Width = EditorGUILayout.FloatField("По оси X", grid.Width);
    //    grid.Height = EditorGUILayout.FloatField("По оси Y", grid.Height);
    //    grid.Draw = EditorGUILayout.Toggle("Показывать", grid.Draw);

    //    EditorGUILayout.LabelField("Настройка сетки:", EditorStyles.boldLabel);
    //    grid.CountLines = EditorGUILayout.IntSlider("Кол-во линий", (int)grid.CountLines, 1, 1000);

    //    EditorGUILayout.LabelField("Выбранные объекты:", EditorStyles.boldLabel);
    //    foreach (var obj in Selection.objects)
    //    {
    //        EditorGUILayout.TextField("Имя объекта:", obj.name);
    //    }

    //    grid.name = "Grid By Kilosoft";
    //    EditorUtility.SetDirty(grid);

    //    EditorGUILayout.LabelField("О программе:", EditorStyles.boldLabel);
    //    EditorGUILayout.LabelField("Name: Grid By Kilosoft\nAuthor: Kilosoft\nVersion: 0.1", EditorStyles.helpBox);
    //}


    //void Awake()
    //{
    //    //grid = GameObject.Find("Grid").GetComponent<Grid>();
    //}

    void Update()
    {
        //base.OnInspectorGUI();
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

            EditorUtility.SetDirty(grid);
        }

        
    }
}
#endif
