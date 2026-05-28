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
        public override StopType Type => StopType.Flour;
        [SerializeField] private int wheatRequired = 2;
        [SerializeField] private int flourProduced = 1;
        [SerializeField] private float cycleTime = 8f;
        [SerializeField] private WindMillFan fan;

        public void RotateFan(float deltaTime)
        {
            fan.Rotate(deltaTime * productivityFactor);
        }

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
