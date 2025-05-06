using System.Collections.Generic;
using UnityEngine;

namespace Blessing.Gameplay.Characters.Traits
{
    [CreateAssetMenu(fileName = "TraitList", menuName = "Scriptable Objects/Traits/TraitList")]
    public class TraitList : ScriptableObject
    {
        [SerializeField] public string Name;
        [SerializeField][TextArea] public string Description;
        public Trait[] Traits;
    }
}

