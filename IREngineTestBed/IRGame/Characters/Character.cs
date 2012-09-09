using IRGame.Common;
using IRGame.Common.Enums;
using System.Text;

namespace IRGame.Characters
{
    public class Character : GameObject
    {
        #region Fields
        private object _statePrintLock = new object();
        #endregion
        #region Ctors

        public Character()
        {
            ApplyForce(0.0, 0.0);
            ApplyMoment(0.0, 0.0);
        }

        #endregion

        #region Public Methods

        public void SetPosition(double x, double y)
        {
            State[StateVar.XPosition] = x;
            State[StateVar.YPosition] = y;
        }
        public void SetVelocity(double x, double y)
        {
            State[StateVar.XVelocity] = x;
            State[StateVar.YVelocity] = y;
        }

        public void SetPosition(Vector2 position)
        {
            SetPosition(position.X, position.Y);
        }
        public void SetVelocity(Vector2 velocity)
        {
            SetVelocity(velocity.X, velocity.Y);
        }
        public void SetOrientation(double angle)
        {
            State[StateVar.Orientation] = angle;
        }

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
            double fx0 = (double)State.GetOrAdd(StateVar.XForce, 0.0);
            double fy0 = (double)State.GetOrAdd(StateVar.YForce, 0.0);
            
            State.AddOrUpdate(StateVar.XForce, fx, (k, v) => fx);
            State.AddOrUpdate(StateVar.XForceGrad, fx - fx0, (k, v) => fx - fx0);
            State.AddOrUpdate(StateVar.XForce, fy, (k, v) => fy);
            State.AddOrUpdate(StateVar.YForceGrad, fy - fy0, (k, v) => fy - fy0);
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

        public void SetTransitionMatrix(Vector4 xMtxElements, Vector4 yMtxElements)
        {
            State[StateVar.XTransitionMtx] = xMtxElements;
            State[StateVar.YTransitionMtx] = yMtxElements;
        }

        public void SetControlMatrix(Vector8 xMtxElements, Vector8 yMtxElements)
        {
            State[StateVar.XControlMtx] = xMtxElements;
            State[StateVar.YControlMtx] = yMtxElements;
        }

        public string StateToString()
        {
            // TODO: wrap all operations with State dictionary to lock
            var builder = new StringBuilder();
            lock (_statePrintLock)
            {
                foreach (var key in State.Keys)
                {
                    builder.AppendFormat("{0} => {1}\n", key, State[key]);
                }
            }
            return builder.ToString();
        }
        #endregion
    }
}
