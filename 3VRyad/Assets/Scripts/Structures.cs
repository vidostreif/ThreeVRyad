using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//структура в которой сохраняются позиция элемента
public struct Position
{
    public int posX;
    public int posY;

    public Position(int posX, int posY)
    {
        this.posX = posX;
        this.posY = posY;
    }

    public void Info()
    {
        Debug.LogError("posX:" + posX + " posY: " + posY);
    }
}

//структура в которой сохраняются соседние элементы
public struct NeighboringBlocks
{
    public Block Up;
    public Block Down;
    public Block Left;
    public Block Right;
    public Block[] allBlockField;

    public NeighboringBlocks(Block up, Block down, Block left, Block right)
    {
        ////если значения не определены то создаем пустые блоки
        //if (up == null)
        //    up = new BlockField();
        //if (down == null)
        //    down = new BlockField();
        //if (left == null)
        //    left = new BlockField();
        //if (right == null)
        //    right = new BlockField();

        this.Up = up;
        this.Down = down;
        this.Left = left;
        this.Right = right;

        allBlockField = new Block[4] { this.Up, this.Down, this.Left, this.Right };
    }

    //выдает блок в указанном направлении
    public Block GetBlock(DirectionEnum direction)
    {
        Block block = null;
        if (direction == DirectionEnum.Up)
            block = this.Up;
        else if (direction == DirectionEnum.Down)
            block = this.Down;
        else if (direction == DirectionEnum.Left)
            block = this.Left;
        else if (direction == DirectionEnum.Right)
            block = this.Right;

        return block;
    }

    public void Info()
    {
        Debug.LogError("Up: " + Up + " Down: " + Down + " Left: " + Left + " Right: " + Right);
    }

    
}



