using System;
using System.Collections.Generic;
using Blessing.Gameplay.SkillsAndMagic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Blessing.Gameplay.Characters.Traits
{
    [Serializable] public struct TraitData : IEquatable<TraitData>, INetworkSerializeByMemcpy
    {
        public FixedString64Bytes Id;
        public int TraitId;
        public int Duration;
        public bool Equals(TraitData other)
        {
            return Id == other.Id && TraitId == other.TraitId;
        }
    }
    [CreateAssetMenu(fileName = "Trait", menuName = "Scriptable Objects/Traits/Trait")]
    public class Trait : ScriptableObject
    {
        public int Id;
        public string Name;
        public string Label;
        [TextArea] public string Description;
        public Effect[] Effects;
        public VisualEffect VisualEffect;
        public bool SpawnVFXOnGround = false;
        public bool VFXFollowChar = false;
        public bool CanStack;

        public int GetStatChange(Stat stat)
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (StatChange change in effect.StatChanges)
                if (change.Stat == stat)
                {
                    changeValue += change.Value;
                }
            }

            return changeValue;
        }

        public int GetHealthRegenChange()
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (HealthChange change in effect.HealthChanges)
                {
                    changeValue += change.Regen;
                }
            }

            return changeValue;
        }
        public int GetHealthDecayChange()
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (HealthChange change in effect.HealthChanges)
                {
                    changeValue += change.Decay;
                }
            }

            return changeValue;
        }

        public int GetAttackChange()
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (AttackChange change in effect.AttackChanges)
                {
                    changeValue += change.Attack;
                }
            }

            return changeValue;
        }

        public int GetDefenseChange()
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (DefenseChange change in effect.DefenseChanges)
                {
                    changeValue += change.Defense;
                }
            }

            return changeValue;
        }

        public int GetManaRegen(ManaColor color)
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (ManaChange change in effect.ManaChanges)
                if (change.ManaColor == color)
                {
                    changeValue += change.Regen;
                }
            }

            return changeValue;
        }

        public int GetManaDecay(ManaColor color)
        {
            int changeValue = 0;
            foreach (Effect effect in Effects)
            {
                foreach (ManaChange change in effect.ManaChanges)
                if (change.ManaColor == color)
                {
                    changeValue += change.Decay;
                }
            }

            return changeValue;
        }
#if UNITY_EDITOR
        public void Awake()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Trait", new[] { "Assets/Traits" });

            if (Id == 0)
                Id = guids.Length;
        }
        [UnityEditor.MenuItem("Blessing/GameData/Traits/Generate Traits Ids")]
        public static void GenerateTraitsIds()
        {
            string[] allTraitsGuids = UnityEditor.AssetDatabase.FindAssets("t:TraitList AllTraits", new[] { "Assets/Traits" });

            TraitList allTraits;
            
            if (allTraitsGuids.Length == 0)
            {
                // Create new weapon
                Debug.Log("AllTraits Created");
                
                string path = $"Assets/Traits/AllTraits.asset";
                allTraits = CreateInstance<TraitList>();
                UnityEditor.AssetDatabase.CreateAsset(allTraits, path);
            }
            else
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(allTraitsGuids[0]);
                allTraits = (TraitList) UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TraitList));
            }

            // Find all Texture2Ds that have 'co' in their filename, that are labelled with 'architecture' and are placed in 'MyAwesomeProps' folder
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Trait", new[] { "Assets/Traits" });

            int incrementId = 1;

            List<Trait> allTraitsList = new();

            foreach (string guid in guids)
            {
                Debug.Log(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));

                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                Trait trait = (Trait) UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(Trait));

                allTraitsList.Add(trait);

                trait.Id = incrementId;

                Debug.Log(trait.name + " New Id: " + incrementId);

                incrementId += 1;

                UnityEditor.EditorUtility.SetDirty(trait);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }

            allTraits.Traits = allTraitsList.ToArray();

            UnityEditor.EditorUtility.SetDirty(allTraits);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}

