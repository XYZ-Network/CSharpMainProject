using Buff;
using Model.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Codice.Client.BaseCommands.Import.Commit;

namespace Buff
{
    public class IncMovSpdEffect : Effect
    {
        public IncMovSpdEffect(Unit _unit) : base(_unit)
        {
            Modifier = 0.4f;
            Duration = 10;
        }
    }
}