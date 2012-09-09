using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using IRGame.Common.Enums;

namespace IRGame.Common
{
    public class GameObject
    {
        public Guid Id { get; private set; }
        public ConcurrentDictionary<StateVar, object> State { get; private set; }

        public GameObject()
        {
            Id = Guid.NewGuid();
            State = new ConcurrentDictionary<StateVar, object>();
        }
    }
}
