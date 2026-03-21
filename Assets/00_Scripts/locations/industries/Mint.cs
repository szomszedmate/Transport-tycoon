using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._00_Scripts.locations.industries
{
    public class Mint : Industry
    {
        [SerializeField] private int goldBarRequired = 1;
        [SerializeField] private int coinProduced = 3;
        [SerializeField] private float cycleTime = 9f;

        protected override void InitializeRecipe()
        {
            recipe = new Recipe(
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.GoldBar, goldBarRequired }
                },
                new Dictionary<ResourceEnum, int>
                {
                    { ResourceEnum.Coin, coinProduced }
                },
                cycleTime
            );
        }
    }
}
