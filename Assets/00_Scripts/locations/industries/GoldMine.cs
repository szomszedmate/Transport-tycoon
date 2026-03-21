using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._00_Scripts.locations.industries
{
    public class GoldMine : Mine
    {
        protected override void Start()
        {
            producedResource = ResourceEnum.Gold;
            producedAmount = 1;
            cycleTime = 10f;
            base.Start();
        }
    }
}
