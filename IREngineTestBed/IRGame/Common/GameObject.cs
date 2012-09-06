using System;
using System.Collections.Generic;

namespace IRGame.Common
{
    public class GameObject
    {
        public Guid Id { get; private set; }
        public Dictionary<int, object> State { get; private set; }

        public GameObject()
        {
            Id = Guid.NewGuid();
            State = new Dictionary<int, object>();
        }
    }
}
