using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IRGame
{
    public sealed class SimpleGame
    {
        
        #region Singleton

        private static volatile SimpleGame _instance;
        private static readonly object SyncRoot = new Object();

        private SimpleGame()
        {
            
        }

        public static SimpleGame Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = new SimpleGame();
                    }
                }

                return _instance;
            }
        }

        #endregion

    }
}
