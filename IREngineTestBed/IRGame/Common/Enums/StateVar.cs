using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IRGame.Common.Enums
{
    public enum StateVar
    {
        TimeSample,
        TimeRemainder,
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
        XForceGrad,
        XForceThresh,
        YForce,
        YForceGrad,
        YForceThresh,
        XMoment,
        XMomentThresh,
        YMoment,
        YMomentThresh,
        XTransitionMtx,
        XControlMtx,
        YTransitionMtx,
        YControlMtx
    }
}
