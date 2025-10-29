using System;
using System.Collections;
using System.Collections.Generic;
using Blessing.Ai;
using Blessing.Gameplay;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Blessing
{
    // Will Controll some gameplay mechaniques on the map
    // Temporary
    public class GameMaster : NetworkBehaviour, ISpawnable
    {
        [SerializeField] protected TextMeshPro waitTimerText;
        [SerializeField] protected TextMeshPro currentWaveText;
        [SerializeField] protected int waitTime = 10;
        private static WaitForSeconds waitForSeconds1 = new(1f);
        public SessionOwnerMultiSpawner MultiSpawner;
        [SerializeField] protected NetworkVariable<int> waveTimer = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        [SerializeField] protected NetworkVariable<int> waitTimer = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public int WaitTimer { get { return waitTimer.Value; } protected set { waitTimer.Value = value; }}

        [SerializeField] protected NetworkVariable<int> currentWave = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public int CurrentWave { get { return currentWave.Value; } protected set { currentWave.Value = value; }}

        [SerializeField] protected NetworkVariable<bool> isWaveInProgress = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public bool IsWaveInProgress { get { return isWaveInProgress.Value;} protected set { isWaveInProgress.Value = value; }}

        public int EnemyWaveQty = 1;
        public List<AiCharacter> WaveEnemies = new();

        public override void OnNetworkSpawn()
        {
            waitTimer.OnValueChanged += OnNetworkWaitTimerChanged;
            currentWave.OnValueChanged += OnNetworkCurrentWaveChanged;
            isWaveInProgress.OnValueChanged += OnNetworkIsWaveInProgressChanged;
        }

        public override void OnNetworkDespawn()
        {
            waitTimer.OnValueChanged -= OnNetworkWaitTimerChanged;
            currentWave.OnValueChanged -= OnNetworkCurrentWaveChanged;
            isWaveInProgress.OnValueChanged -= OnNetworkIsWaveInProgressChanged;

            StopAllCoroutines();
        }

        private void OnNetworkIsWaveInProgressChanged(bool previousValue, bool newValue)
        {
            waitTimerText.gameObject.SetActive(!newValue);
        }

        private void OnNetworkCurrentWaveChanged(int previousValue, int newValue)
        {
            currentWaveText.text = "Onda número: " + newValue;
        }

        private void OnNetworkWaitTimerChanged(int previousValue, int newValue)
        {
            waitTimerText.text = "Próxima onda em " + (waitTime - newValue);
        }
        protected override void OnOwnershipChanged(ulong previous, ulong current)
        {
            base.OnOwnershipChanged(previous, current);

            // Resincronizar o número de inimigos na onda
            
        }

        public void Init(SessionOwnerNetworkObjectSpawner spawner)
        {

        }
        void Start()
        {
            MultiSpawner = GameManager.Singleton.MultiSpawner as SessionOwnerMultiSpawner;

            if (HasAuthority)
                StartNewWave(1);

            waitTimerText.gameObject.SetActive(!IsWaveInProgress);
            currentWaveText.text = "Onda número: " + CurrentWave;
            waitTimerText.text = "Próxima onda em " + (waitTime - WaitTimer);
        }


        void Update()
        {

        }

        private float timer = 0f;
        void FixedUpdate()
        {
            if (HasAuthority && IsWaveInProgress)
            {
                if (timer >= 3f)
                {
                    timer = 0f;

                    // Check if all enemies in wave are dead
                    bool allEnemiesDead = true;

                    foreach (AiCharacter character in WaveEnemies)
                    {
                        if (character.Health.IsAlive)
                        {
                            allEnemiesDead = false;
                            break;
                        }
                    }

                    if (allEnemiesDead)
                    {
                        Debug.Log("All Enemies are Dead");
                        FinishCurrentWave();
                    }
                }

                timer += Time.fixedDeltaTime;
            }
        }
        public void StartNewWave(int waveNumber)
        {
            currentWave.Value = waveNumber;
            // Start Wave
            // 1 << waveNumber will calculate 2 ^ wavenumber
            MultiSpawner.Spawn(EnemyWaveQty + (1 << waveNumber));

            WaveEnemies = MultiSpawner.SpawnedAiCharacters;

            IsWaveInProgress = true;
        }

        public void FinishCurrentWave()
        {
            
            IsWaveInProgress = false;
            Debug.Log(gameObject.name + ": Entrou FinishCurrentWave");

            WaveEnemies.Clear();
            MultiSpawner.SpawnedAiCharacters.Clear();
            StartCoroutine(WaitToStartNewWave());
        }

        IEnumerator WaitToStartNewWave()
        {
            WaitTimer = 0;
            while (WaitTimer < waitTime)
            {
                WaitTimer++;
                yield return waitForSeconds1;
            }
            
            StartNewWave(currentWave.Value + 1);

            // StopCoroutine(WaitToStartNewWave());
        }

        public void DisposeDeadBodies()
        {

        }
    }
}
