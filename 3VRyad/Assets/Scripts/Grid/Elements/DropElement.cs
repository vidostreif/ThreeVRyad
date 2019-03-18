using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//сбрасываемый элемент
public class DropElement : Element
{
    protected override void DopSettings()
    {
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.Drop };
    }
}
