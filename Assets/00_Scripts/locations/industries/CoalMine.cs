using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._00_Scripts.locations.industries
{
    public class CoalMine : Mine
    {
        protected override void Start()
        {
            producedResource = ResourceEnum.Coal;
            producedAmount = 1;
            cycleTime = 8f;
            base.Start();
        }
    }
}
