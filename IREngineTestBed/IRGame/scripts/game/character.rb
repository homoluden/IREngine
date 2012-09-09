#!ruby19
# encoding: utf-8
include IRGame::Common
include IRGame::Common::Enums
include IRGame::Characters

class Character
    def Character.build_a_b(a_coeffs, b_coeffs, ts)
        a0, a1, a2 = a_coeffs
        b0, b1 = b_coeffs
        
        t1 = 6*a0**2
        t2 = a2*ts^2
        t3 = a1*ts
        t4 = a0*t2
        t5 = a2*ts
        
        a = Vector4.new
        a.v1, a.v2, a.v3, a.v4 = [-(3*t4-2*t2*t3-t1)/t1,
                                  -((2*t4-2*t3^2+3*a0*t3-t1)*ts)/t1, 
                                  (t4*(2*t5+3*a1)-t1*t5-2*a1*t2*t3)/(a0*t1),
                                  ((4*t3-3*a0)*t4-2*t3^3+3*a0*t3^2-t1*t3+a0*t1)/(a0*t1)]
        
        t1 = a1*ts
        t2 = a0*ts
        t3 = a2*ts
        t4 = 6*a0^2
        t5 = b0*t1
        t6 = b1*ts
        t7 = t2*t3
        b = Vector.new
        b.v1, b.v2, a.v3, b.v4, b.v5, b.v6, b.v7, b.v8 = [-(2*t1*t6*ts-3*t2*t6)/t4,
                                                          -(2*t5*ts^2-3*b0*t2*ts)/t4,
                                                          -(2*t1^2*ts-3*t1*t2)/t4,
                                                          -(2*t1*t3*ts-3*t7)/t4,
                                                          (t6*(2*t1^2-2*t7)+(6*a0*b1-3*b1*t1)*t2)/(a0*t4),
                                                          ((2*t1*t5-2*b0*t7)*ts-3*t2*t5+6*a0*b0*t2)/(a0*t4),
                                                          (-2*t1*t7+t1*t4+2*t1^3-3*a0*t1^2)/(a0*t4),
                                                          (t3*(2*t1^2-2*t7)+(6*a0*a2-3*a2*t1)*t2)/(a0*t4)]
        
        [a, b]
    end
    def Character.build_discrete_model(ax, ay, bx, by, ts)
        # TODO: add parameters check
                
        character = Character.new
        
        character.state[StateVar.TimeSample]
        ax_trans, bx_trans = Character.build_a_b(ax, bx, ts)
        
        ay_trans, by_trans = Character.build_a_b(ay, by, ts)
        character.set_transition_matrix(ax_trans, ay_trans)
        character.set_control_matrix(bx_trans, by_trans)
        
    end

    def Character.update(state, time_delta)
        ts = state[StateVar.TimeSample]
        if time_delta < ts
            return
        end
        
        t = state[StateVar.TimeRemainder] + time_delta
        n, rem = (t / ts).to_i
        state[StateVar.TimeRemainder] = rem
        
        n.times{|i|
            Character.step(state)
        }
    end

    def Character.step(state)
    
        x, y, tx, ty = state[StateVar.XPosition], state[StateVar.YPosition], state[StateVar.XPosTarget], state[StateVar.YPosTarget]
                                       
        vx, vy, tvx, tvy = state[StateVar.XVelocity], state[StateVar.YVelocity], state[StateVar.XVelTarget], state[StateVar.YVelTarget]
        
        fx, dfx, fy, dfy, fxt, fyt = state[StateVar.XForce],
                                     State[StateVar.XForceGrad],
                                     state[StateVar.YForce],
                                     State[StateVar.YForceGrad],
                                     state[StateVar.XForceThresh], 
                                     state[StateVar.YForceThresh]
        
        ax, ay, bx, by = state[StateVar.XTransitionMtx], state[StateVar.YTransitionMtx], 
                         state[StateVar.XControlMtx], state[StateVar.YControlMtx]
        
        a0, a1, a2, a3 = ax.v1, ax.v2, ax.v3, ax.v4        
        x_, vx_ = a0*x + a1*vx,    a2*x + a3*vx
        
        fx = 0.0 if fx < fxt
                
        x_ += (bx.v3)*tx+(bx.v4)*tvx+(bx.v1)*fx+(bx.v2)*dfx
        vx_ += (bx.v7)*tx+(bx.v8)*tvx+(bx.v5)*fx+(bx.v6)*dfx
                
        a0, a1, a2, a3 = ay.v1, ay.v2, ay.v3, ay.v4
        y_, vy_ = a0*y+a1*vy, a2*y+a3*vy
        
        fy = 0.0 if fy < fyt
            
        y_ += (by.v3)*ty+(by.v4)*tvy+(by.v1)*fy+(by.v2)*dfy
        vy_ += (by.v7)*ty+(by.v8)*tvy+(by.v5)*fy+(by.v6)*dfy
        
        state[StateVar.XPosition], state[StateVar.YPosition] = x_, y_
        state[StateVar.XVelocity], state[StateVar.YVelocity] = vx_, vy_
        
        vel = Vector2.new
        vel.x, vel.y = vx_, vy_
        state[StateVar.Orientation] = vel.angle
    end
end