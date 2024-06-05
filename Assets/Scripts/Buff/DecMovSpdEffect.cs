using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Codice.Client.BaseCommands.Import.Commit;

namespace Buff
{ 
    public class DecMovSpdEffect : Effect
    {
        public DecMovSpdEffect(Unit _unit) : base(_unit)
        {
            Modifier = 1.5f;
            Duration = 10;
        }
    }
}