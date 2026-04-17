using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._00_Scripts.locations.industries
{
    public class Well : Industry
    {
        public override StopType Type => StopType.Water;
        [SerializeField] private int producedAmount = 1;
        [SerializeField] private float cycleTime = 6f;

        protected override void InitializeRecipe()
        {
            recipe = new Recipe(
                new Dictionary<ResourceEnum, int>(),
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.Water, producedAmount }
                },
                cycleTime
            );
        }
    }
}
