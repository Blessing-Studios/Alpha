using UnityEngine;
using Unity.Netcode;
using Blessing.Gameplay.HealthAndDamage;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using Unity.Netcode.Components;
using Blessing.Gameplay.TradeAndInventory;
using Blessing.Gameplay.Interation;
using Blessing.Gameplay.Characters.InputActions;
using Blessing.Gameplay.Characters.InputDirections;
using Blessing.Gameplay.SkillsAndMagic;
using System.Collections;
using Blessing.Gameplay.Characters.States;
using Blessing.Audio;
using Blessing.Gameplay.Characters.Traits;
using Blessing.VFX;
using System.Linq;
using UnityEngine.VFX;

namespace Blessing.Gameplay.Characters
{
    [RequireComponent(typeof(CharacterStateMachine))]
    [RequireComponent(typeof(MovementController))]
    [RequireComponent(typeof(CharacterHealth))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(CharacterGear))]
    [RequireComponent(typeof(CharacterMana))]
    [RequireComponent(typeof(CharacterStats))]
    public abstract class Character : MonoBehaviour, IHitter, IHittable, ISkillTrigger
    {
        protected string characterName;
        public string CharacterName
        {
            get => characterName;
            protected set => characterName = value;
        }

        [field: SerializeField] public bool ShowDebug { get; private set; }
        public float AttackPressedTimerWindow = 0.2f;
        [field: SerializeField] protected List<CharacterHitAudio> characterHitAudios = new();
        public MovementController MovementController { get; protected set; }
        public CharacterStateMachine CharacterStateMachine { get; protected set; }
        public CharacterHealth Health { get; protected set; }
        public CharacterGear Gear { get; protected set; }
        public CharacterMana Mana { get; protected set; }
        public CharacterStats Stats { get; protected set; }
        public Dictionary<Stat, int> ValueByStat { get { return Stats.ValueByStat;} }
        public CharacterController CharacterController { get; protected set; }
        public CharacterNetwork CharacterNetwork { get; protected set; }
        [field: SerializeField] protected InputActionList actionList;
        [field: SerializeField] protected InputDirectionList directionList;
        public InputActionType TriggerAction { get; protected set; }
        public InputDirectionType TriggerDirection { get; protected set; }
        protected Dictionary<string, InputActionType> inputActionsDic = new();
        protected Dictionary<string, InputDirectionType> inputDirectionsDic = new();
        [field: SerializeField] public Vector2Int DamageAndPen { get; protected set; }
        [field: SerializeField] public Vector2Int DefenseAndPenRes { get; protected set; }

        // Unificar CharacterTraits e CharacterBuffs quando acabar de testar
        public List<CharacterTrait> CharacterTraits = new();
        
         // Teste, temporário
        public List<CharacterSkill> CharacterSkills;
        [field: SerializeField] public Skill ActiveSkill { get; set; }
        public HashSet<PassiveSkill> PassiveSkills = new();
        [field: SerializeField] public Transform SkillOrigin { get; protected set; }
        protected Vector3 skillDirection = Vector3.right;
        public Vector3 SkillDirection { get { return skillDirection; }}
        
        public List<IHittable> TargetList { get; private set; }

        public HitInfo HitInfo { get; protected set; }
        // [field: SerializeField] protected NetworkVariable<int> stateIndex = new NetworkVariable<int>(1,
        //     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Mover para CharacterNetwork

        // public int StateIndex { get { return stateIndex.Value; }}

        public int StateIndex
        {
            get
            {
                if (CharacterNetwork != null)
                    return CharacterNetwork.StateIndex;
                else
                    return stateIndex;
            }
        }
        public void SetStateIndex(int stateIndex) // Gambiarra para funcionar tanto online quanto offline
        {
            if (CharacterNetwork != null)
                CharacterNetwork.SetStateIndex(stateIndex);
            else
                this.stateIndex = stateIndex;
        }

        public void SetComboMoveIndex(Vector2Int comboMoveIndex)
        {
            if (CharacterNetwork != null)
                CharacterNetwork.SetComboMoveIndex(comboMoveIndex);
        }

        private int stateIndex; // Offline StateIndex
        public Vector3 SpawnLocation;

        public bool HasAuthority
        {
            get
            {
                if (CharacterNetwork != null)
                    return CharacterNetwork.HasAuthority;
                else
                    return true;
            }
        }

        protected virtual void Awake()
        {
            MovementController = GetComponent<MovementController>();
            CharacterStateMachine = GetComponent<CharacterStateMachine>();
            Health = GetComponent<CharacterHealth>();
            Gear = GetComponent<CharacterGear>();
            Mana = GetComponent<CharacterMana>();
            Stats = GetComponent<CharacterStats>();

            CharacterController = GetComponent<CharacterController>();

            CharacterNetwork = GetComponent<CharacterNetwork>();

            TargetList = new List<IHittable>();

            foreach (InputActionType InputAction in actionList.InputActions)
            {
                inputActionsDic.Add(InputAction.Name, InputAction);
            }

            foreach (InputDirectionType InputDirection in directionList.InputDirections)
            {
                inputDirectionsDic.Add(InputDirection.Name, InputDirection);
            }

            // stateIndex.Value = 0;
        }

        protected virtual void Start()
        {
            if (HasAuthority)
            {
                CharacterController.enabled = false;
                CharacterController.transform.position = SpawnLocation;
                CharacterController.enabled = true;
            }

            // Invoke(nameof(Initialize), 2.0f);
        }

        public virtual void Initialize()
        {

            if (ShowDebug) Debug.Log(gameObject.name + ": Initialize");

            StopAllCoroutines();
            StartCoroutine(ChangeByTime());

            SynchTraits();
        }

        private void SynchTraits()
        {
            CharacterTraits.Clear();
            GameManager.Singleton.UpdateLocalList(ref CharacterNetwork.TraitDataLocalList, CharacterNetwork.TraitDataNetworkList);

            foreach (TraitData traitData in CharacterNetwork.TraitDataLocalList)
            {
                foreach (Trait trait in GameManager.Singleton.AllTraits.Traits)
                {
                    if (trait.Id == traitData.TraitId)
                    {
                        Buff buff = trait as Buff;

                        if (buff != null)
                        {
                            CharacterBuff characterBuff = new(buff) { Data = traitData };
                            CharacterTraits.Add(characterBuff);
                        }
                        else
                        {
                            CharacterTrait characterTrait = new(trait) { Data = traitData };
                            CharacterTraits.Add(characterTrait);
                        }

                        break;
                    }
                }
            }

            UpdateParameters();
        }

        public Trait GetTrait(int id)
        {
            foreach (Trait trait in GameManager.Singleton.AllTraits.Traits)
            {
                if (trait.Id == id)
                {
                    return trait;
                }
            }

            return null;
        }

        public IEnumerator ChangeByTime()
        {
            while (Health.IsAlive)
            {
                if (HasAuthority)
                {
                    HandlePassiveSkills();
                    HandleBuffs();
                    Health.ChangeByTime();
                    Mana.ChangeByTime();
                }

                yield return new WaitForSeconds(Mana.ChangeTime);

                //Put code after waiting here

                //You can put more yield return new WaitForSeconds(1); in one coroutine
            }
        }
        public void ClearTargetList()
        {
            if (HasAuthority)
                CharacterNetwork.ClearTargetListRpc();
        }

        // [Rpc(SendTo.Everyone)]
        // public void ClearTargetListRpc() // mover para CharacterNetwork, não usar RPC
        // {
        //     TargetList.Clear();
        // }

        public virtual bool Hit(IHittable target)
        {
            if (target as Character == this)
            {
                // Can't hit itself
                return false;
            }

            if (TargetList.Contains(target))
            {
                // hit failed, target was already hit;
                return false;
            }

            // If CurrentMove is null, it will throw a error

            // ApplySkill
            if (ActiveSkill != null)
            {
                // Calculate damage from skill
                int skillDamage = ActiveSkill.GetSkillDamage(Stats.ValueByStat);

                // O dano soma com o do ataque normal e a Penetração é trocada pela pen da skill
                HitInfo = new HitInfo(DamageAndPen.x + skillDamage, ActiveSkill.DamageClass, ActiveSkill.Buffs, ActiveSkill.HitType);

                ActiveSkill = null;
            }
            else
            {
                HitInfo = new HitInfo(DamageAndPen.x, DamageAndPen.y, null, HitType.Slash);
            }

            TargetList.Add(target);

            return true;
        }

        public virtual void GotHit(IHitter hitter)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": Entrou GotHit HitInfo Damage - " + hitter.HitInfo.Damage);

            // TODO: criar sistema mais complexo para selecionar o som dependendo do tipo do hit usando o CharacterStateMachine
            HandleHitAudio(hitter.HitInfo.HitType);

            if (!HasAuthority) return;

            // Por enquanto, não pode bater em characters mortos
            if (Health.IsDead) return;

            // Apply Skill buffs
            if (hitter.HitInfo.Buffs != null)
                foreach (Buff buff in hitter.HitInfo.Buffs)
                {
                    ApplyBuff(buff);
                }

            // Subtrair Pen com PenRes
            int armorPen = hitter.HitInfo.DamageClass - DefenseAndPenRes.y;

            int damage = hitter.HitInfo.Damage;
            int defense = DefenseAndPenRes.x;

            if (armorPen < 0)
            {
                damage = (int)(damage * (1 - armorPen * 0.25f));
                damage = damage < 0 ? 0 : damage;
            }

            if (armorPen > 0)
            {
                defense = (int)(defense * (1 - armorPen * 0.25f));
                defense = defense < 0 ? 0 : defense;
            }

            int appliedDamage = damage - defense;

            if (ShowDebug) Debug.Log(gameObject.name + ": Entrou GotHit appliedDamage - " + appliedDamage);

            appliedDamage = appliedDamage < 0 ? 0 : appliedDamage;

            //Receive Damage
            Health.ReceiveDamage(appliedDamage);

            int health = Health.CurrentHealth;
            if (health > 0)
                CharacterStateMachine.SetNextState(CharacterStateMachine.TakeHitState);

            if (health <= 0)
                CharacterStateMachine.SetNextState(CharacterStateMachine.DeadState);
        }

        private void HandleHitAudio(HitType hitType)
        {
            foreach(CharacterHitAudio characterHitAudio in characterHitAudios)
            {
                if (characterHitAudio.HitType == hitType)
                    AudioManager.Singleton.PlaySoundFx(characterHitAudio.Clips, transform);
            } 
        }

        public virtual void OnDeath()
        {
            Health.SetCharacterAsDead();
            MovementController.DisableMovement();
            MovementController.DisableCollision();
        }
        public virtual void GetOwnership()
        {
            CharacterNetwork.GetOwnership();
        }
        public InputActionType GetAction(string name)
        {
            return inputActionsDic[name];
        }

        public InputDirectionType GetDirection(string name)
        {
            return inputDirectionsDic[name];
        }
        public abstract bool CheckIfActionTriggered(string actionName);
        public void AddTrait(Trait trait)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": AddTrait trait name - " + trait.name);
            // Check if Trait can be added
            if (!HasAuthority) return;

            // Check if this Trade Can Stack
            if (trait.CanStack == false)
            {
                foreach(CharacterTrait cTrait in CharacterTraits)
                {
                    if (cTrait.Trait == trait) return;
                }
            }

            TraitData data;

            Buff buff = trait as Buff;
            if (buff != null)
            {
                CharacterBuff characterBuff = new(buff);
                CharacterTraits.Add(characterBuff);
                data = characterBuff.Data;
            }
            else
            {
                CharacterTrait characterTrait = new(trait);
                CharacterTraits.Add(characterTrait);
                data = characterTrait.Data;
            }

            if (trait.VisualEffect != null)
            {
                CharacterNetwork.HandleTraitVisualEffectRpc(trait.Id);
            }

            CharacterNetwork.TraitDataNetworkList.Add(data);
            CharacterNetwork.TraitDataLocalList.Add(data);

            UpdateParameters();   
        }

        public void AddTrait(TraitData traitData)
        {
            foreach (Trait trait in GameManager.Singleton.AllTraits.Traits)
            {
                if (trait.Id == traitData.TraitId)
                {
                    Buff buff = trait as Buff;

                    if (buff != null)
                    {
                        CharacterBuff characterBuff = new(buff) { Data = traitData };
                        CharacterTraits.Add(characterBuff);
                    }
                    else
                    {
                        CharacterTrait characterTrait = new(trait) { Data = traitData };
                        CharacterTraits.Add(characterTrait);
                    }

                    break;
                }
            }

            UpdateParameters();

            if (!HasAuthority) return;

            CharacterNetwork.TraitDataNetworkList.Add(traitData);
            CharacterNetwork.TraitDataLocalList.Add(traitData);
        }

        public void RemoveTrait(Trait trait)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": RemoveTrait trait name - " + trait.name);

            // Check if Trait can be removed
            if (!HasAuthority) return;

            // Melhorar lógica
            Buff buff = trait as Buff;

            TraitData dataToRemove;

            CharacterTrait traitToRemove = CharacterTraits.Where(item => item.Trait == trait).OrderByDescending(item => item.Data.Duration).FirstOrDefault();

            if (traitToRemove == null) return;

            dataToRemove = traitToRemove.Data;
            CharacterTraits.Remove(traitToRemove);

            UpdateParameters();

            CharacterNetwork.TraitDataNetworkList.Remove(dataToRemove);
            CharacterNetwork.TraitDataLocalList.Remove(dataToRemove);
        }
        public void RemoveTrait(TraitData traitData)
        {
            int indexToRemove = -1;
            for (int i = 0; i < CharacterTraits.Count; i++)
            {
                if (CharacterTraits[i].Data.Equals(traitData))
                {
                    indexToRemove = i;
                }
            }

            CharacterTraits.RemoveAt(indexToRemove);

            UpdateParameters();

            if (!HasAuthority) return;

            CharacterNetwork.TraitDataNetworkList.Remove(traitData);
            CharacterNetwork.TraitDataLocalList.Remove(traitData);
        }

        public void ApplyBuff(Buff buff)
        {
            AddTrait(buff);          
        }

        private void HandleBuffs()
        {
            if (CharacterTraits.OfType<CharacterBuff>().ToList().Count == 0) return;

            // Traits will have duration 0 as by default, so only Buffs will be subtract
            CharacterTraits.ForEach(buff => { if (buff.Data.Duration > 0) buff.Data.Duration--; });

            // Updating TraitDataNetworkList so it can subtract Duration
            TraitData[] datas = CharacterNetwork.TraitDataLocalList.ToArray();
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].Duration > 0)
                {
                    CharacterNetwork.TraitDataNetworkList.Remove(datas[i]);
                    CharacterNetwork.TraitDataLocalList.Remove(datas[i]);

                    datas[i].Duration--;

                    CharacterNetwork.TraitDataNetworkList.Add(datas[i]);
                    CharacterNetwork.TraitDataLocalList.Add(datas[i]);
                }
            }

            List<CharacterBuff> buffsToRemove = CharacterTraits.OfType<CharacterBuff>().Where(buff => buff.Data.Duration <= 0).ToList();

            List<TraitData> datasToRemove = new();
            foreach (CharacterBuff characterBuff in buffsToRemove)
            {
                datasToRemove.Add(characterBuff.Data);
                if (characterBuff.Buff.SideBuff != null)
                    ApplyBuff(characterBuff.Buff.SideBuff);
            }

            foreach (TraitData data in datasToRemove)
            {
                RemoveTrait(data);
            }
        }

        public void UpdateParameters()
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": UpdateParameters");
            // Update Stats
            Stats.UpdateAllStats(CharacterTraits);

            // Update Health
            Health.SetHealthParameters(Stats.Constitution, CharacterTraits);

            // Update Damage and Defense
            DamageAndPen = Gear.GetWeaponDamageAndPen(CharacterTraits);
            DefenseAndPenRes = Gear.GetArmorDefenseAndPen(CharacterTraits);

            // Update Mana
            Mana.SetManaParameters(Stats, CharacterTraits);
        }

        // GameEventListeners
        public void OnAddEquipment(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": OnAddEquipment");

            CharacterEquipment characterEquipment = data as CharacterEquipment;

            InventoryItem inventoryItem = characterEquipment.InventoryItem;

            Backpack backpack = inventoryItem.Item as Backpack;

            if (backpack != null)
            {
                Gear.AddBackpack(inventoryItem);
            }

            UpdateParameters();

            GameManager.Singleton.InventoryController.SyncGrids();
        }

        public void OnRemoveEquipment(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": OnRemoveEquipment");

            CharacterEquipment characterEquipment = data as CharacterEquipment;

            if (characterEquipment.GearSlotType == Gear.BackpackSlot)
            {
                Gear.RemoveBackpack();
            }

            UpdateParameters();

            GameManager.Singleton.InventoryController.SyncGrids();
        }

        public void OnAnimationAttack()
        {
            // TODO: create logic to select Skill with combo
            // if (!HasAuthority) return; 
            
            if (CharacterStateMachine.CurrentMove.CanUseSkill == true)
            {
                foreach (CharacterSkill characterSkill in CharacterSkills)
                {
                    if (characterSkill.IsActive && characterSkill.ComboMoveIndex == CharacterStateMachine.ComboMoveIndex)
                    {
                        HandleSkill(characterSkill.Skill);
                    }
                }   
            }

            // Handle Attack Sound
            AudioClip[] attackAudios = CharacterStateMachine.CurrentMove.AudioClips;

            if (attackAudios.Length > 0)
                AudioManager.Singleton.PlaySoundFx(attackAudios, transform);
        }

        public void HandleSkill(Skill skill)
        {
            // Spent Mana to trigger skill
            if (Mana.SpendManaSpectrum(skill.ManaCost))
            {
                skill.Trigger(this);

                // Add/Remove Passive Skill
                PassiveSkill activePassiveSkill = skill as PassiveSkill;
                if (activePassiveSkill != null)
                {
                    if (!PassiveSkills.Add(activePassiveSkill))
                    {
                        PassiveSkills.Remove(activePassiveSkill);
                        foreach(Buff buff in activePassiveSkill.Buffs)
                        {
                            RemoveTrait(buff);
                        }
                    }
                }
            }
        }

        private void HandlePassiveSkills()
        {
            if (PassiveSkills.Count == 0) return;

            // Debug.Log(gameObject.name + ": entrou HandlePassiveSkills -  PassiveSkills.Count" + PassiveSkills.Count);

            foreach (PassiveSkill passive in PassiveSkills)
            {
                // Spend Mana

                if (Mana.SpendManaSpectrum(passive.PassiveManaCost))
                {
                    foreach(Buff buff in passive.Buffs)
                    {
                        ApplyBuff(buff);
                    }

                    if (passive.AfterSkill != null)
                        passive.AfterSkill.Trigger(this);
                }
                else
                {
                    PassiveSkills.Remove(passive);
                    foreach(Buff buff in passive.Buffs)
                    {
                        RemoveTrait(buff);
                    }
                }
            }

            // Gastar mana e checar duração das passivas
        }
    }
}