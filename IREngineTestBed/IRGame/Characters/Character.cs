using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IRGame.Common;
using IRGame.Common.Helpers;

namespace IRGame.Characters
{
    public class Character : GameObject
    {       
        #region Ctors

        public Character()
        {
            this.ApplyForce(0.0, 0.0);
            this.ApplyMoment(0.0, 0.0);
        }

        #endregion
    }
}
