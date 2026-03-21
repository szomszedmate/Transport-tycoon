using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._00_Scripts.locations.industries
{
    public class Mill : Industry
    {
        [SerializeField] private int wheatRequired = 2;
        [SerializeField] private int flourProduced = 1;
        [SerializeField] private float cycleTime = 8f;

        protected override void InitializeRecipe()
        {
            recipe = new Recipe(
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.Wheat, wheatRequired }
                },
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.Flour, flourProduced }
                },
                cycleTime
            );
        }
    }
}
