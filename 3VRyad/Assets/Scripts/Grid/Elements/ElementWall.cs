using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementWall : Element {   

    protected override void DopSettings()
    {
        vulnerabilityTypeEnum = new HitTypeEnum[] { HitTypeEnum.HitFromNearbyElement, HitTypeEnum.Explosion, HitTypeEnum.Instrument };
    }
}
