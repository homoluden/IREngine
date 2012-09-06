using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IRGame.Common.Enums
{
    public enum StateVar
    {
        Initialized,
        Orientation,
        XPosition,
        XVelocity,
        XPosTarget,
        XVelTarget,
        YPosition,
        YVelocity,
        YPosTarget,
        YVelTarget,
        XForce,
        XForceThresh,
        YForce,
        YForceThresh,
        XMoment,
        XMomentThresh,
        YMoment,
        YMomentThresh,
        TransitionMtx,
        ControlMtx
    }
}
