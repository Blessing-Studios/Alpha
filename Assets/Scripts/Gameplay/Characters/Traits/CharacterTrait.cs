
using System;
using Unity.Collections;

namespace Blessing.Gameplay.Characters.Traits
{
    [Serializable] public class CharacterTrait
    {
        public string name;
        public Trait Trait;
        public TraitData Data;
        public CharacterTrait(Trait trait)
        {
            name = trait.Name;
            Trait = trait;
            Data = new TraitData(){Id = new FixedString64Bytes(Guid.NewGuid().ToString()), TraitId = Trait.Id};
        }
    }
    [Serializable] public class CharacterBuff : CharacterTrait
    {
        public Buff Buff;
        public CharacterBuff(Buff buff) : base(buff)
        {
            Buff = buff;
            Data.Duration = buff.Duration;
        }
    }
}

