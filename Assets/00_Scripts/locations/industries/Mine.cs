using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._00_Scripts.locations.industries
{
    public abstract class Mine : Industry
    {
        [Header("Mine Settings")]
        [SerializeField] protected ResourceEnum producedResource;
        [SerializeField] protected int producedAmount = 1;
        [SerializeField] protected float cycleTime = 8f;

        protected override void InitializeRecipe()
        {
            recipe = new Recipe(
                new Dictionary<ResourceEnum, int>(),
                new Dictionary<ResourceEnum, int>
                {
                    { producedResource, producedAmount }
                },
                cycleTime
            );
        }
    }
}
