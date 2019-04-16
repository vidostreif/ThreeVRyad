//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

public static class BlockCheck
{
    public static bool ThisBlockWithoutElement(Block block)
    {

        if (block != null && (block.Element == null || block.Element.Destroyed))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithElement(Block block)
    {

        if (block != null && block.Element != null && !block.Element.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithMortalElement(Block block)
    {

        if (block != null && block.Element != null && !block.Element.Destroyed && block.Element.Immortal == false)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithStandartElement(Block block)
    {

        if (block != null && block.Element != null && !block.Element.Destroyed && block.Element.Type == ElementsTypeEnum.Standard)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithCurElement(Block block, ElementsTypeEnum elementsTypeEnum)
    {

        if (block != null && block.Element != null && !block.Element.Destroyed && block.Element.Type == elementsTypeEnum)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithCollectorElementAndNoBlockingElement(Block block)
    {
        if (block != null && block.Element != null && !block.Element.Destroyed && block.Element.Collector
            && (block.Element.BlockingElement == null || (block.Element.BlockingElement != null && block.Element.BlockingElement.Destroyed)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithDestroyElement(Block block)
    {

        if (block != null && block.Element != null && block.Element.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisStandardBlockWithoutElement(Block block)
    {

        if (block != null && block.Type == BlockTypeEnum.Standard && (block.Element == null || block.Element.Destroyed))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisNoBlockedBlockWithElement(Block block)
    {

        if (block != null && !block.Blocked && block.Element != null && !block.Element.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisStandardBlockWithStandartElementCanMove(Block block)
    {

        if (block != null && block.Type == BlockTypeEnum.Standard && block.Element != null && !block.Element.Destroyed && block.Element.Type == ElementsTypeEnum.Standard && !block.Element.LockedForMove)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithElementCanMove(Block block)
    {

        if (block != null && block.Element != null && !block.Element.Destroyed && !block.Element.LockedForMove)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithElementCantMove(Block block)
    {
        if (block != null && block.Element != null && !block.Element.Destroyed && block.Element.LockedForMove)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithElementWithoutBlockingElement(Block block)
    {

        if (block != null && block.Element != null && !block.Element.Destroyed && (block.Element.BlockingElement == null || block.Element.BlockingElement.Destroyed))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithElementAndBlockingElement(Block block)
    {
        if (block != null && block.Element != null && !block.Element.Destroyed && block.Element.BlockingElement != null && !block.Element.BlockingElement.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool ThisBlockWithBehindElement(Block block)
    {

        if (block != null && block.BehindElement != null && !block.BehindElement.Destroyed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //сбрасывающий блок со сбрасываемым элементом
    public static bool ThisBlockDropingWithDropElement(Block block)
    {

        if (block != null && block.Dropping && block.Element != null && !block.Element.Destroyed && block.Element.Drop)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //сбрасывающий блок
    public static bool ThisBlockDroping(Block block)
    {
        if (block != null && block.Dropping)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
