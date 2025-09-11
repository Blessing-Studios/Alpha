using UnityEngine;
using Unity.Netcode;
using Blessing.HealthAndDamage;
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
    [RequireComponent(typeof(CharacterNetwork))]
    public abstract class Character : MonoBehaviour, IHitter, IHittable, ISkillTrigger
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        public float AttackPressedTimerWindow = 0.2f;
        public MovementController MovementController { get; protected set; }
        public CharacterStateMachine CharacterStateMachine { get; protected set; }
        public CharacterHealth Health { get; protected set; }
        public CharacterGear Gear { get; protected set; }
        public CharacterMana Mana { get; protected set; }
        public CharacterStats Stats { get; protected set; }
        public Dictionary<Stat, int> ValueByStat { get { return Stats.ValueByStat;} }
        public CharacterController CharacterController { get {return MovementController.GetCharacterController();} }
        public CharacterNetwork CharacterNetwork { get; protected set; }
        [field: SerializeField] protected InputActionList actionList;
        [field: SerializeField] protected InputDirectionList directionList;
        public InputActionType TriggerAction { get; protected set; }
        public InputDirectionType TriggerDirection { get; protected set; }

        protected Dictionary<string, InputActionType> inputActionsDic = new();
        protected Dictionary<string, InputDirectionType> inputDirectionsDic = new();
        [field: SerializeField] public Vector2Int DamageAndPen { get; protected set; }
        [field: SerializeField] public Vector2Int DefenseAndPenRes { get; protected set; }
        public float ViewRange = 15.0f;

        // Unificar CharacterTraits e CharacterBuffs quando acabar de testar
        public List<CharacterTrait> CharacterTraits = new();
        
        [Header("Abilities and Skills")]
        public List<CharacterComboSkill> CharacterComboSkills;
        protected CastActionType[] castActions { get { return CharacterStateMachine.CastActions; } }
        [field: SerializeField] protected List<Ability> abilitiesSO = new();
        public List<CharacterAbility> Abilities = new();
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
        public float RandomFloat { get { return CharacterNetwork.GenerateRandomFloat(); } }
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

        private int stateIndex; // Offline StateIndex
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
        public void SetAbilityIndex(int abilityIndex)
        {
            if (CharacterNetwork != null)
                CharacterNetwork.SetAbilityIndex(abilityIndex);
        }

        
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

        public bool HasStarted = false;
        protected virtual void Start()
        {
            if (HasAuthority)
            {
                CharacterController.enabled = false;
                CharacterController.transform.position = SpawnLocation;
                CharacterController.enabled = true;
            }

            // Invoke(nameof(Initialize), 2.0f);

            // if (GameManager.Singleton.PlayerConnected && isInitialized == false)
            //     Initialize();

            HasStarted = true;
        }

        private bool isInitialized = false;
        public bool IsInitialized { get { return isInitialized; } }
        public virtual void Initialize()
        {
            if (isInitialized) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": Initialize");

            StopAllCoroutines();
            StartCoroutine(ChangeByTime());

            SynchTraits();

            LoadAbilities();

            isInitialized = true;
        }

        protected virtual void LoadAbilities()
        {
            Abilities.Clear();

            for (int i = 0; i < abilitiesSO.Count; i++)
            {
                Abilities.Add(new CharacterAbility(abilitiesSO[i], castActions[i]));
            }
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

        public virtual bool Hit(IHittable target, Vector3 hitPosition)
        {
            if (target as Character == this)
            {
                // Can't hit itself
                return false;
            }

            // If CurrentMove is null, it will throw a error
            Move currentMove = CharacterStateMachine.CurrentMove;

            if (TargetList.Contains(target))
            {
                // Target was already hit;
                return false;
            }

            
            float impact = Stats.Strength * currentMove.ImpactMultiplier;
            int damage = (int) (DamageAndPen.x * currentMove.DamageMultiplier);

            // ApplySkill
            if (ActiveSkill != null)
            {
                // Calculate damage from skill
                int skillDamage = ActiveSkill.GetSkillDamage(Stats.ValueByStat);

                // O dano soma com o do ataque normal e a Penetração é trocada pela pen da skill
                HitInfo = new HitInfo(damage + skillDamage, ActiveSkill.DamageClass, impact, hitPosition, ActiveSkill.Buffs, ActiveSkill.HitType);

                ActiveSkill = null;
            }
            else
            {
                // Pegar Damage Multiplicador do CurrentMove
                // CharacterStateMachine.CurrentMove
                HitInfo = new HitInfo(damage, DamageAndPen.y, impact, hitPosition, null, HitType.Slash);
            }

            TargetList.Add(target);

            // Gain Mana on hit
            Mana.HandleHit();

            return true;
        }

        public virtual void GotHit(IHitter hitter, HurtBox hurtBox)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": Entrou GotHit HitInfo Damage - " + hitter.HitInfo.Damage);

            if (hurtBox.Owner != (this as IHittable)) return;

            // Por enquanto, não pode bater em characters mortos
            if (Health.IsDead) return;

            // TODO: criar sistema mais complexo para selecionar o som dependendo do tipo do hit usando o CharacterStateMachine
            HandleHitEffect(hitter, hurtBox);

            if (!HasAuthority) return;

            HitInfo hitInfo = hitter.HitInfo;

            // Apply Skill buffs
            if (hitInfo.Buffs != null)
                foreach (Buff buff in hitInfo.Buffs)
                {
                    ApplyBuff(buff);
                }

            // Subtrair Pen com PenRes
            int armorPen = hitInfo.DamageClass - DefenseAndPenRes.y;

            int damage = hitInfo.Damage;
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

            Vector3 pushDirection = (transform.position - hitInfo.HitPosition).normalized;

            // Debug.Log("Teste: impulseDirection - " + pushDirection);
            // Debug.Log("Teste: transform.position - " + transform.position);
            // Debug.Log("Teste: hitInfo.HitPosition - " + hitInfo.HitPosition);

            float impact = hitInfo.Impact;
            float impulseTime = impact - Stats.Constitution;

            MovementController.HandlePushBack(pushDirection, impact, impulseTime);

            // Gain Mana on hit
            Mana.HandleHit();

            if (ShowDebug) Debug.Log(gameObject.name + ": GotHit Time - " + Time.time);

            if (hitInfo.CanTriggerTakeHit())
                CharacterStateMachine.SetNextState(CharacterStateMachine.TakeHitState);
        }

        private void HandleHitEffect(IHitter hitter, HurtBox hurtBox)
        {
            // TODO: Aplicar lógica do tipo de hit para definir qual efeito disparar

            hurtBox.TriggerHitEffect(hitter.HitInfo);
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
        public abstract bool CheckIfActionTriggered(InputActionType actionType);
        public abstract bool CheckIfDirectionTriggered(InputDirectionType directionType);
        public abstract bool CheckIfComboMoveTriggered(Move move);
        public bool AddTrait(Trait trait)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": AddTrait trait name - " + trait.name);
            // Check if Trait can be added
            if (!HasAuthority) return false;

            // Check if this Trade Can Stack
            if (trait.CanStack == false)
            {
                foreach (CharacterTrait cTrait in CharacterTraits)
                {
                    if (cTrait.Trait == trait) return false;
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

            if (trait.VFX != null)
                CharacterNetwork.HandleTraitVisualEffectRpc(trait.Id);

            CharacterNetwork.TraitDataNetworkList.Add(data);
            CharacterNetwork.TraitDataLocalList.Add(data);

            UpdateParameters();
            return true;
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
            // TODO: checar se index existe antes de remover
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

        public bool ApplyBuff(Buff buff)
        {
            Debug.Log(gameObject.name + ": ApplyBuff - " + buff.name);
            return AddTrait(buff);          
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
        public float TestRandomFloat;
        public void HandleSkill(Skill skill)
        {
            // Spent Mana to trigger skill
            if (Mana.SpendManaSpectrum(skill.ManaCost))
            {
                TestRandomFloat = RandomFloat;
                Debug.Log("Skill TestRandomFloat: " + TestRandomFloat);
                skill.Trigger(this, TestRandomFloat);

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

        public void TriggerAbility()
        {
            foreach(Skill skill in Abilities[CharacterStateMachine.AbilityIndex].Ability.Skills)
            {
                HandleSkill(skill);
            }
        }

        public bool CanCastAbility(int abilityIndex)
        {
            bool canCastAbility = true;

            foreach (Skill skill in Abilities[abilityIndex].Skills)
            {
                if (!Mana.HasEnoughMana(skill.ManaCost))
                {
                    canCastAbility = false;
                }
            }

            return canCastAbility;
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
                    foreach (Buff buff in passive.Buffs)
                    {
                        ApplyBuff(buff);
                    }

                    if (passive.AfterSkill != null)
                        passive.AfterSkill.Trigger(this);
                }
                else
                {
                    PassiveSkills.Remove(passive);
                    foreach (Buff buff in passive.Buffs)
                    {
                        RemoveTrait(buff);
                    }
                }
            }

            // Gastar mana e checar duração das passivas
        }
        // GameEventListeners
        public void OnIsAliveChanged(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            bool isAlive = (bool) data;

            if (!isAlive)
            {
                OnDeath();
            }

            if (isAlive)
            {
                // Resurrect
                Debug.Log(gameObject.name + ": Character OnIsAliveChanged: Resurrect?" + isAlive);
            }
        }

        public virtual void OnDeath()
        {
            MovementController.DisableMovement();
            MovementController.DisableCollision();
            StopAllCoroutines();
        }
        
        public void OnAddEquipment(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": OnAddEquipment");

            CharacterEquipment characterEquipment = data as CharacterEquipment;

            InventoryItem inventoryItem = characterEquipment.InventoryItem;

            // Backpack backpack = inventoryItem.Item as Backpack;
            // if (backpack != null)
            // {
            //     Gear.AddBackpack(inventoryItem);
            // }

            // Utility utility = inventoryItem.Item as Utility;
            // if (utility != null)
            // {
            //     Gear.AddUtility(inventoryItem, characterEquipment.DuplicateIndex);
            // }

            UpdateParameters();

            UIController.Singleton.SyncInventoryGrids();
        }

        public void OnRemoveEquipment(Component component, object data)
        {
            if (component.gameObject != gameObject) return;

            if (ShowDebug) Debug.Log(gameObject.name + ": OnRemoveEquipment");

            CharacterEquipment characterEquipment = data as CharacterEquipment;

            // if (characterEquipment.GearSlotType == Gear.BackpackSlot)
            // {
            //     Gear.RemoveBackpack();
            // }

            // if (characterEquipment.GearSlotType == Gear.UtilitySlot)
            // {
            //     Gear.RemoveUtility(characterEquipment.DuplicateIndex);
            // }

            UpdateParameters();

            UIController.Singleton.SyncInventoryGrids();
        }

        public virtual void OnAnimationAttack()
        {
            // TODO: create logic to select Skill with combo
            // if (!HasAuthority) return; 
            
            if (CharacterStateMachine.CurrentMove.CanUseSkill == true)
            {
                foreach (CharacterComboSkill characterSkill in CharacterComboSkills)
                {
                    if (characterSkill.IsActive && characterSkill.ComboMoveIndex == CharacterStateMachine.ComboMoveIndex)
                    {
                        HandleSkill(characterSkill.Skill);
                    }
                }   
            }

            if (CharacterStateMachine.CurrentMove.CanMultiHit == true)
            {
                ClearTargetList();
            }

            // Handle Attack Sound
            AudioClip[] attackAudios = CharacterStateMachine.CurrentMove.AudioClips;

            if (attackAudios.Length > 0)
                AudioManager.Singleton.PlaySoundFx(attackAudios, transform);
        }

        public virtual void OnAnimationCast()
        {
            Debug.Log("OnAnimationCast");
            TriggerAbility();
        }
    }
}