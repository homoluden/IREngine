#!ruby19
# encoding: utf-8
include IRGame::Common::Enums
include IRGame::Characters

class Character
    def Character.update(state)
        x, y, tx, ty = state[StateVar.XPosition], state[StateVar.YPosition], state[StateVar.XPosTarget], state[StateVar.YPosTarget]
                                       
        vx, vy, tvx, tvy = state[StateVar.XVelocity], state[StateVar.YVelocity], state[StateVar.XVelTarget], state[StateVar.YVelTarget]
        
        fx, fy, fxt, fyt = state[StateVar.XForce], state[StateVar.YForce], state[StateVar.XForceThresh], state[StateVar.YForceThresh]
        
        a, b = state[StateVar.TransitionMtx], state[StateVar.ControlMtx]
        
        
    end
end