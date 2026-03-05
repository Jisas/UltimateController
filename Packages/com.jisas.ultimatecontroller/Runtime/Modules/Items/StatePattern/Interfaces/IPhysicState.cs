using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using System;

namespace UltimateController.ItemSystem.StatePattern
{
    internal interface IPhysicState
    {
        public void StateFixedUpdate();
    }
}
