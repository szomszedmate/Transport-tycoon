using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._00_Scripts.locations.industries
{
    public class GoldSmelter : Industry
    {
        [SerializeField] private int goldRequired = 2;
        [SerializeField] private int coalRequired = 1;
        [SerializeField] private int goldBarProduced = 1;
        [SerializeField] private float cycleTime = 12f;

        protected override void InitializeRecipe()
        {
            recipe = new Recipe(
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.Gold, goldRequired },
                    { ResourceEnum.Coal, coalRequired }
                },
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.GoldBar, goldBarProduced }
                },
                cycleTime
            );
        }
    }
}
