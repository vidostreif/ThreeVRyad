using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BorderGrid
{
    ////обвести сетку
    public static void CircleGrid(Grid grid)
    {

        GameObject borderGrid = new GameObject();
        borderGrid.name = "BorderGrid";
        borderGrid.transform.parent = grid.transform;
        borderGrid.transform.localRotation = new Quaternion(0, 0, 0, 0);
        borderGrid.transform.localPosition = new Vector3(0 - grid.blockSize * 0.25f, 0 - grid.blockSize * 0.25f, 0);

        //MonoBehaviour.Instantiate(prefabElement, thisTransform.position, Quaternion.identity);

        for (int x = 0; x < grid.containers.GetLength(0) + 1; x++)
        {
            for (int y = 0; y < grid.containers[0].block.GetLength(0) + 1; y++)
            {
                

                Block LeftDown = grid.GetBlock(x - 1, y - 1);
                Block LeftUp = grid.GetBlock(x - 1, y);
                Block RightDown = grid.GetBlock(x, y - 1);
                Block RightUp = grid.GetBlock(x, y);

                //если не все блоки равны null
                if ((LeftDown == null && LeftUp == null && RightDown == null && RightUp == null) || (LeftDown != null && LeftUp != null && RightDown != null && RightUp != null))
                {
                    continue;
                }
                else
                {
                    //определяем какие блоки заняты и выбираем соответствующий рисунок и поворот
                    Vector3 localPosition = new Vector3(grid.blockSize * x, grid.blockSize * y, 0);
                    int turn = 0;
                    if (RightUp == null)
                        DefineAndCreateBorder(localPosition, turn, borderGrid, LeftUp, RightDown);

                    //смещаем вниз
                    localPosition = new Vector3(localPosition.x, localPosition.y - grid.blockSize * 0.5f, localPosition.z);
                    turn = turn - 90;
                    if (RightDown == null)
                        DefineAndCreateBorder(localPosition, turn, borderGrid, RightUp, LeftDown);

                    //смещаем влево
                    localPosition = new Vector3(localPosition.x - grid.blockSize * 0.5f, localPosition.y, localPosition.z);
                    turn = turn - 90;
                    if (LeftDown == null)
                        DefineAndCreateBorder(localPosition, turn, borderGrid, RightDown, LeftUp);

                    //смещаем вверх
                    localPosition = new Vector3(localPosition.x, localPosition.y + grid.blockSize * 0.5f, localPosition.z);
                    turn = turn - 90;
                    if (LeftUp == null)
                        DefineAndCreateBorder(localPosition, turn, borderGrid, LeftDown, RightUp);
                }


            }
        }
    }

    private static void CreateBorder(Vector3 localPosition, int turn, BorderEnum borderEnum, GameObject borderGrid) {
        GameObject border = new GameObject();
        border.name = "border" + borderEnum;
        border.transform.parent = borderGrid.transform;
        border.transform.localRotation = Quaternion.Euler(border.transform.localRotation.eulerAngles.x, border.transform.localRotation.eulerAngles.y, turn);
        border.transform.localPosition = localPosition;
        SpriteRenderer spriteRendererBorder = border.AddComponent<SpriteRenderer>();
        spriteRendererBorder.sprite = SpriteBank.SetShape(borderEnum);
        spriteRendererBorder.sortingLayerName = "Block";
    }

    private static void DefineAndCreateBorder(Vector3 localPosition, int turn, GameObject borderGrid, Block block1, Block block2)
    {
        if (block1 != null && block2 != null)
            CreateBorder(localPosition, turn + 0, BorderEnum.InAngle, borderGrid);
        else if (block1 == null && block2 != null)
            CreateBorder(localPosition, turn + 0, BorderEnum.Line, borderGrid);
        else if (block1 != null && block2 == null)
            CreateBorder(localPosition, turn - 90, BorderEnum.Line, borderGrid);
        else
            CreateBorder(localPosition, turn + 0, BorderEnum.OutAngle, borderGrid);
    }
}
