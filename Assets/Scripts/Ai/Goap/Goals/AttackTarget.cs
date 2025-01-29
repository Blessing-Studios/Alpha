using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Blessing.Ai.Goap.Goals
{
    [Serializable]
    public class AttackTarget : BaseGoal
    {
        private readonly string name = "AttackTarget";
        [SerializeField] public override string Name => name;
        private readonly bool value = true;
        [SerializeField] public override bool Value => value;

        private readonly int priority = 10;
        [SerializeField] public override int Priority => priority;

        public override bool ValidateGoal(AiAgent aiAgent)
        {
            return true;
        }
    }
}
