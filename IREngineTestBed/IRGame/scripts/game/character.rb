#!ruby19
# encoding: utf-8
require "helpers"
require "logging"
require "IRGame.dll"

include IRGame::Common
include IRGame::Common::Enums
include IRGame::Characters

class Character
    def Character.build_abc(a_coeffs, b_coeffs, ts)
        a1, a2, a3 = a_coeffs
        b1, b2 = b_coeffs
        
        t1 = 2*a1
        t2 = a3*ts**2
        t3 = a2*ts**2
        
        a = Vector4.new
        a.v1, a.v2, a.v3, a.v4 = [-(t2-t1)/t1,
                                   (t1*ts-t3)/t1,
                                  -(a3*t1*ts-a2*t2)/(a1*t1),
                                  -(a2*t1*ts-a2*t3+a1*t2-a1*t1)/(a1*t1)]
        
        b = Vector2.new
        b.x, b.y = [ ts**2/2,
                     (t1*ts-t3)/t1]
        
        c = Vector2.new
        c.x, c.y = [b1, b2] #[b1/ts, b2/ts]
        
        [a, b, c]
    end
    def Character.build_discrete_model(ax, ay, bx, by, ts)
        # TODO: add parameters check
                
		puts "\n***   Starting build of discrete model...\n" if $verbose
        character = Character.new
        
        character.state[StateVar.XPosStiff] = 1.0
        character.state[StateVar.YPosStiff] = 1.0
        character.state[StateVar.TimeSample] = ts
        
        x_trans, x_ctrl, x_meas = Character.build_abc(ax, bx, ts)        
        y_trans, y_ctrl, y_meas = Character.build_abc(ay, by, ts)
        IRE.log "Created ABC:\ncx: #{x_meas.inspect}\ncy: #{y_meas.inspect}\n" if $verbose
        
        character.set_transition_matrix(x_trans, y_trans)
        character.set_control_matrix(x_ctrl, y_ctrl)
        character.set_measure_matrix(x_meas, y_meas)
        
        character.set_position 0, 0
        character.set_velocity 0, 0
        character.set_orientation 0
        character.set_target_position 0, 0
        character.apply_force 0, 0
        character.state[StateVar.TimeRemainder] = 0
        character.state[StateVar.XForceThresh] = 0
        character.state[StateVar.YForceThresh] = 5
        character.state[StateVar.LastUx] = 0.0
        character.state[StateVar.LastUy] = 0.0
        character.state[StateVar.XPhaseVector] = Vector2.new
        character.state[StateVar.YPhaseVector] = Vector2.new
        
        IRE.log "Character created successfully!\nInitial state:\n#{character.state_to_string}\n" if $verbose
        
        character
    end

    def Character.update(state, time_delta)
        ts = state[StateVar.TimeSample]
        rem = state[StateVar.TimeRemainder]
        state[StateVar.TimeRemainder] = t = rem + time_delta
        if t < ts
            return
        end
                
        n, rem = t.divmod ts
        state[StateVar.TimeRemainder] = rem
        
        n.times{|i|
            Character.step(state)
        }
    end

    def Character.step(state)
        IRE.log "Step started" if $verbose
        
        ts = state[StateVar.TimeSample]
        x_stiff, y_stiff = state[StateVar.XPosStiff], state[StateVar.YPosStiff]
        x, y = state[StateVar.XPhaseVector], state[StateVar.YPhaseVector]
        IRE.log "Recieved phase vectors:\nx: #{x.inspect}\ny: #{y.inspect}\n" if $verbose
        
        px, py, tx, ty = state[StateVar.XPosition], state[StateVar.YPosition], state[StateVar.XPosTarget], state[StateVar.YPosTarget]
        IRE.log "Position vars recieved" if $verbose
        
        fx, fy, fxt, fyt =  state[StateVar.XForce],
                            state[StateVar.YForce],
                            state[StateVar.XForceThresh], 
                            state[StateVar.YForceThresh]
        IRE.log "Force vars recieved" if $verbose
        
        ax, bx, cx = state[StateVar.XTransitionMtx], state[StateVar.XControlMtx], state[StateVar.XMeasureMtx]
        ay, by, cy = state[StateVar.YTransitionMtx], state[StateVar.YControlMtx], state[StateVar.YMeasureMtx]
        
        IRE.log "Model vars recieved" if $verbose
        
        a1, a2, a3, a4 = ax.v1, ax.v2, ax.v3, ax.v4        
        x0 = a1*x.x + a2*x.y
        x1 = a3*x.x + a4*x.y
        
        fx = 0.0 if fx < fxt
        
        ux = fx + x_stiff * (tx - px)
        state[StateVar.LastUx] = ux
        
        x0 += bx.x * ux
        x1 += bx.y * ux
        new_phase = Vector2.new
        new_phase.x, new_phase.y = x0, x1
        
        state[StateVar.XPhaseVector] = new_phase
        
        dpx = cx.x * x0 + cx.y * x1
        pos = state[StateVar.XPosition] = state[StateVar.XPosition] + dpx
        vx = state[StateVar.XVelocity] = dpx/ts
        
        a1, a2, a3, a4 = ay.v1, ay.v2, ay.v3, ay.v4        
        y0 = a1*y.x + a2*y.y
        y1 = a3*y.x + a4*y.y
        
        fy = 0.0 if fy < fyt
        
        uy = fy + y_stiff * (ty - py)
        state[StateVar.LastUy] = uy
        
        y0 += by.x * uy
        y1 += by.y * uy
        new_phase = Vector2.new
        new_phase.x, new_phase.y = y0, y1
        
        state[StateVar.YPhaseVector] = new_phase
        
        dpy = cy.x * y0 + cy.y * y1
        pos = state[StateVar.YPosition] = state[StateVar.YPosition] + dpy
        vy = state[StateVar.YVelocity] = dpy/ts
        
        vel = Vector2.new
        vel.x, vel.y = vx, vy
        state[StateVar.Orientation] = vel.angle.to_deg
        IRE.log "Step end!" if $verbose
    end
    
    def print
        x, y, tx, ty = state[StateVar.XPosition], state[StateVar.YPosition], state[StateVar.XPosTarget], state[StateVar.YPosTarget]
        vx, vy = state[StateVar.XVelocity], state[StateVar.YVelocity]
        
        IRE.log "Position: {#{x}; #{y}}. Target: {#{tx};#{ty}}."
        IRE.log "Velocity: {#{vx}; #{vy}}."
    end
end