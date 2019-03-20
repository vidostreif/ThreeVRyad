using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//структура в которой сохраняются позиция элемента
public class Position
{
    public int posX;
    public int posY;

    //public Position()
    //{
    //    this.posX = -1;
    //    this.posY = -1;
    //}

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

    //выдает противоположный блок
    public Block GetOppositeBlock(DirectionEnum direction)
    {
        Block block = null;
        if (direction == DirectionEnum.Up)
            block = this.Down;
        else if (direction == DirectionEnum.Down)
            block = this.Up;
        else if (direction == DirectionEnum.Left)
            block = this.Right;
        else if (direction == DirectionEnum.Right)
            block = this.Left;

        return block;
    }

    //выдает противоположный блок
    public Block GetOppositeBlock(Block inBlock)
    {
        Block block = null;
        if (inBlock == this.Up)
            block = this.Down;
        else if (inBlock == this.Down)
            block = this.Up;
        else if (inBlock == this.Left)
            block = this.Right;
        else if (inBlock == this.Right)
            block = this.Left;

        return block;
    }

    //возвращает направление для указанного блока
    public DirectionEnum GetDirection(Block inBlock)
    {
        DirectionEnum directionEnum = DirectionEnum.Empty;
        if (inBlock == this.Up)
            directionEnum = DirectionEnum.Up;
        else if (inBlock == this.Down)
            directionEnum = DirectionEnum.Down;
        else if (inBlock == this.Left)
            directionEnum = DirectionEnum.Left;
        else if (inBlock == this.Right)
            directionEnum = DirectionEnum.Right;

        return directionEnum;
    }

    //возвращает противоположное направление блоку
    public DirectionEnum GetOppositeDirection(Block inBlock)
    {
        DirectionEnum directionEnum = DirectionEnum.Empty;
        if (inBlock == this.Up)
            directionEnum = DirectionEnum.Down;
        else if (inBlock == this.Down)
            directionEnum = DirectionEnum.Up;
        else if (inBlock == this.Left)
            directionEnum = DirectionEnum.Right;
        else if (inBlock == this.Right)
            directionEnum = DirectionEnum.Left;

        return directionEnum;
    }

    public void Info()
    {
        Debug.LogError("Up: " + Up + " Down: " + Down + " Left: " + Left + " Right: " + Right);
    }

    
}



