using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimulatorDriver : AvatarDriver
{
    public SimulatorDriver(GameObject obj) : base(obj)
    {
        type = AvatarDriver.AvatarDriverType.Simulation;

        // Ready to obtain measures
        ready = true;
    }
}
