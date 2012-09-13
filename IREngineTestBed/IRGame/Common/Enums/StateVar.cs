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
        XPhaseVector,
        XPosition,
        XVelocity,
        XPosTarget,
        YPhaseVector,
        YPosition,
        YVelocity,
        YPosTarget,
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
        XMeasureMtx,
        YTransitionMtx,
        YControlMtx,
        YMeasureMtx,
        XPosStiff,
        YPosStiff,
        LastUx,
        LastUy
    }
}
