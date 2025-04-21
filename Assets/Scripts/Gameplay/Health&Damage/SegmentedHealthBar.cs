using System.Collections;
using System.Collections.Generic;
using Blessing.Gameplay.Characters;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.HealthAndDamage
{
    public class SegmentedHealthBar : MonoBehaviour
    {
        protected CharacterHealth characterHealth;
        [SerializeField] protected Slider[] healthBars;
        [SerializeField] protected GameObject woundPrefab;
        [SerializeField] private float healthPercent = 100;

        private void HandleOnHealthChanged(object sender, System.EventArgs eventArgs)
        {
            // Update healthPercent
            // UpdateHealthBars();
        }

        // Player Character Va
        public void Initialize(CharacterHealth characterHealthComponent)
        {
            if (characterHealthComponent == null)
            {
                Debug.LogError("Health Bar component needs a characterHealth.");
                return;
            }

            this.characterHealth = characterHealthComponent;

            // Create wounds
            for (int i = 1; i <= characterHealth.MaxWounds; i++)
            {
                GameObject wound = Instantiate(
                woundPrefab,
                new Vector3(0, 0, 0),
                Quaternion.identity);

                wound.transform.SetParent(transform, false);

                float width = wound.transform.Find("Frame").GetComponent<RectTransform>().rect.width;

                wound.GetComponent<RectTransform>().localPosition = new Vector3(width * (i - 1), 0, 0);
            }

            healthBars = GetComponentsInChildren<Slider>();

            healthPercent = characterHealth.GetHealthPercent();
        }

        public void UpdateHealthBars()
        {
            int woundHealth = characterHealth.GetWoundHealth();
            int wounds = characterHealth.GetWounds();
            int health = characterHealth.GetHealth();

            Debug.Log("woundHealth: " + woundHealth);
            Debug.Log("wounds: " + wounds);
            Debug.Log("health: " + health);

            for (int i = 0; i < healthBars.Length; i++)
            {
                if (i < wounds - 1)
                {
                    healthBars[i].value = 1;
                }
                else if (i == wounds - 1)
                {
                    float rawValue = (wounds - 1) * woundHealth;
                    float newValue = (health - rawValue) / woundHealth;
                    Debug.Log("rawValue: " + rawValue);
                    Debug.Log("newValue: " + newValue);
                    healthBars[i].value = newValue;
                }
                else
                {
                    healthBars[i].value = 0;
                }
            }
        }

        void OnDestroy()
        {
            // characterHealth.OnHealthChanged -= HandleOnHealthChanged;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                UpdateHealthBars();
            }
        }
    }
}