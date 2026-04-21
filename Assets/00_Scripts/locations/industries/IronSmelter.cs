using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._00_Scripts.locations.industries
{
    public class IronSmelter : Industry
    {
        public override StopType Type => StopType.IronBar;
        [SerializeField] private int ironRequired = 2;
        [SerializeField] private int coalRequired = 1;
        [SerializeField] private int ironBarProduced = 1;
        [SerializeField] private float cycleTime = 10f;

        protected override void InitializeRecipe()
        {
            recipe = new Recipe(
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.Iron, ironRequired },
                    { ResourceEnum.Coal, coalRequired }
                },
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.IronBar, ironBarProduced }
                },
                cycleTime
            );
        }
    }
}
