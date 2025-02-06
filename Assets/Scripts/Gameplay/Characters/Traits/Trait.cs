using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Characters.Traits
{
    [Serializable]
    public class StatChange
    {
        public Stat Stat;
        public int Value;
    }
    [CreateAssetMenu(fileName = "Trait", menuName = "Scriptable Objects/Characters/Trait")]
    public class Trait : ScriptableObject
    {
        public string Name;
        public List<StatChange> StatChanges;

        public int GetStatChange(Stat stat)
        {
            int changeValue = 0;
            foreach (StatChange change in StatChanges)
            {
                if (change.Stat == stat)
                {
                    changeValue += change.Value;
                }
            }

            return changeValue;
        }
    }
}

