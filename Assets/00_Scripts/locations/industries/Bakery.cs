using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._00_Scripts.locations.industries
{
    using System.Collections.Generic;
    using UnityEngine;

    public class Bakery : Industry
    {
        [SerializeField] private int flourRequired = 2;
        [SerializeField] private int waterRequired = 1;
        [SerializeField] private int breadProduced = 1;
        [SerializeField] private float cycleTime = 10f;

        protected override void InitializeRecipe()
        {
            recipe = new Recipe(
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.Flour, flourRequired },
                    { ResourceEnum.Water, waterRequired }
                },
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.Bread, breadProduced }
                },
                cycleTime
            );
        }
    }
}
