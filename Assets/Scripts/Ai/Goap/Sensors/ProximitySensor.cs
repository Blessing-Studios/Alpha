using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Blessing.Ai.Goap.Models;
using Blessing.Gameplay.Characters;

namespace Blessing.Ai.Goap.Sensors
{
    public class ProximitySensor : MonoBehaviour, ISensor
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        private string sensorType = "ProximitySensor";
        public float SensorUpdateTime = 2.0f;
        public object Value { get; private set; }
        private List<IModel> modelsList;
        public List<IModel> ModelsList => modelsList;
        public List<Character> CloseEnemyList;
        public List<Character> CloseAllyList;
        private float timeLastUpdate;
        private AiAgent aiAgent;
        private AiCharacter aiCharacter;
        public GameObject ClosestEnemy = null;

        void Awake()
        {
            // aiPilot = GetComponent<AiPilot>();
            aiAgent = GetComponent<AiAgent>();
            aiCharacter = GetComponent<AiCharacter>();

            timeLastUpdate = 0.0f;

            CloseAllyList = new List<Character>();
            CloseEnemyList = new List<Character>();
        }

        public void StartSensor()
        {

        }

        void Update()
        {
            // if ((Time.time - timeLastUpdate) > SensorUpdateTime)
            // {
            //     timeLastUpdate = Time.time;
            //     UpdateSensor();
            //     UpdateModels();
            // }
        }

        public void UpdateSensor()
        {
            if (ShowDebug) Debug.Log(gameObject.name + "Entrou UpdateSensor");

            CloseAllyList = new List<Character>();
            CloseEnemyList = new List<Character>();

            // Collider2D[] colliders = Physics2D.OverlapCircleAll((Vector2)transform.position, aiCharacter.ViewRange);
            Collider[] colliders = Physics.OverlapSphere(transform.position, aiCharacter.ViewRange);
            
            ClosestEnemy = null;
            float nearestDistance = float.MaxValue;
            float distance;

            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent<Character>(out var character) && character.Health.IsAlive)
                {
                    if (gameObject.tag == character.gameObject.tag)
                    {
                        if (!CloseAllyList.Contains(character))
                        {
                            CloseAllyList.Add(character);
                        }
                    }
                    else
                    {
                        if (!CloseEnemyList.Contains(character))
                        {
                            CloseEnemyList.Add(character);

                            distance = (transform.position - character.transform.position).sqrMagnitude;
                            if (distance < nearestDistance)
                            {
                                nearestDistance = distance;
                                ClosestEnemy = character.gameObject;
                            }
                        }
                    }
                }
            }
        }

        public void UpdateModels()
        {
            if (modelsList == null && aiAgent.AgentModelsList != null)
            {
                modelsList = new List<IModel>();
                foreach (IModel model in aiAgent.AgentModelsList)
                {
                    if (model.SensorType == sensorType)
                    {
                        modelsList.Add(model);
                    }
                }
            }

            aiAgent.UpdateModels(modelsList);
        }
    }
}

