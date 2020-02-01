﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPassedCondition : InversableCondition
{
    public GameplayEvent Event;
    public override bool Check()
    {
        bool result = PlayerData.eventsDone.Contains(Event);
        return Invert ? !result : result;
    }
}
