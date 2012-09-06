﻿using IRGame.Common;
using IRGame.Common.Enums;

namespace IRGame.Characters
{
    public class Character : GameObject
    {       
        #region Ctors

        public Character()
        {
            ApplyForce(0.0, 0.0);
            ApplyMoment(0.0, 0.0);
        }

        #endregion

        #region Public Methods

        public Vector2 GetPosition()
        {
            return new Vector2 { X = (double)State[StateVar.XPosition], Y = (double)State[StateVar.YPosition] };
        }
        public Vector2 GetVelocity()
        {
            return new Vector2 { X = (double)State[StateVar.XVelocity], Y = (double)State[StateVar.YVelocity] };
        }
        public double GetOrientation()
        {
            return (double)State[StateVar.Orientation];
        }

        public void ApplyForce(Vector2 force)
        {
            ApplyForce(force.X, force.Y);
        }
        public void ApplyForce(double fx, double fy)
        {
            State[StateVar.XForce] = fx;
            State[StateVar.YForce] = fy;
        }
        
        public void ApplyMoment(Vector2 moment)
        {
            ApplyMoment(moment.X, moment.Y);
        }
        public void ApplyMoment(double mx, double my)
        {
            State[StateVar.XMoment] = mx;
            State[StateVar.YMoment] = my;
        }

        public void SetTargetPosition(Vector2 target)
        {
            SetTargetPosition(target.X, target.Y);
        }
        public void SetTargetPosition(double xTarget, double yTarget)
        {
            State[StateVar.XPosTarget] = xTarget;
            State[StateVar.YPosTarget] = yTarget;
        }
        
        public void SetTargetVelocity(Vector2 target)
        {
            SetTargetVelocity(target.X, target.Y);
        }
        public void SetTargetVelocity(double xTarget, double yTarget)
        {
            State[StateVar.XVelTarget] = xTarget;
            State[StateVar.YVelTarget] = yTarget;
        }

        public void SetTransitionMatrix(Vector4 vector)
        {
            State[StateVar.TransitionMtx] = vector;
        }

        public void SetControlMatrix(Vector8 vector)
        {
            State[StateVar.ControlMtx] = vector;
        }

        #endregion
    }
}
