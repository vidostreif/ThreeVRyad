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

    public static bool ThisBlockWithCollectorElement(Block block)
    {
        if (block != null && block.Element != null && !block.Element.Destroyed && block.Element.Collector)
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

    public static bool ThisStandardBlockWithElement(Block block)
    {

        if (block != null && block.Type == BlockTypeEnum.Standard && block.Element != null && !block.Element.Destroyed)
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
}
