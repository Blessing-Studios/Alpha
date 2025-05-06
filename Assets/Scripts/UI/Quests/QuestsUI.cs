using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.Guild;
using Blessing.Gameplay.Guild.Quests;
using Blessing.Gameplay.TradeAndInventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI.Quests
{
    public class QuestsUI : MonoBehaviour
    {
        private QuestUIElement questUIElementPrefab { get { return GameManager.Singleton.QuestUIElementPrefab; } }
        private QuestItem questItemPrefab { get { return GameManager.Singleton.QuestItemPrefab; } }
        public GameObject QuestsContainer;
        public List<QuestUIElement> QuestsUIList;
        [SerializeField] private TextMeshProUGUI questGiverNameText;
        [SerializeField] private TextMeshProUGUI locationNameText;

        [Header("Quest Info")]
        public GameObject QuestInfoGameObject;
        public Adventurer Adventurer;
        public List<Quest> Quests;
        public TextMeshProUGUI RankText;
        public TextMeshProUGUI NameText;
        public ContentSizeFitter DescriptionSizeFitter;
        public TextMeshProUGUI DescriptionText;
        public ContentSizeFitter ObjectivesSizeFitter;
        public GameObject ObjectivesContainer;
        public ContentSizeFitter RewardsSizeFitter;
        public GameObject RewardsContainer;
        public Image Banner;
        public Quest SelectedQuest;
        public Button AcceptQuest;
        public Button CompleteQuest;

        void Start()
        {
            AcceptQuest.onClick.AddListener(() =>
            {
                GuildManager.Singleton.TakeQuest(Adventurer, SelectedQuest.Id);
                Sync();
            });

            CompleteQuest.onClick.AddListener(() =>
            {
                GuildManager.Singleton.CompleteQuest(Adventurer, SelectedQuest.Id);
                Sync();
            });

            // Add Button to Give Up quest
        }

        public void OpenQuestsUI(List<Quest> quests, Adventurer adventurer)
        {
            Quests = quests;
            Adventurer = adventurer;
            gameObject.SetActive(true);
            UpdateQuests(quests);
        }

        public void CloseQuestsUI()
        {
            gameObject.SetActive(false);
        }

        public void SelectQuest(Quest quest)
        {
            SelectedQuest = quest;

            RankText.text = $"Rank {quest.Rank.Label}";
            NameText.text = quest.Label;
            DescriptionText.text = quest.Description;
            Banner.sprite = quest.Banner;

            RebuildLayout(DescriptionSizeFitter.gameObject);

            // Objectives
            for(int i = ObjectivesContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(ObjectivesContainer.transform.GetChild(i).gameObject);
            }

            foreach (Objective objective in quest.Objectives)
            {
                TextMeshProUGUI objectiveText = Instantiate(GameManager.Singleton.SimpleTextPrefab);

                objectiveText.text = objective.Description;

                objectiveText.transform.SetParent(ObjectivesContainer.transform);
            }

            // Rewards
            for(int i = RewardsContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(RewardsContainer.transform.GetChild(i).gameObject);
            }

            foreach (Reward reward in quest.Rewards)
            {
                QuestItem questItem = Instantiate(GameManager.Singleton.QuestItemPrefab);

                questItem.Initialize(reward.Item, reward.Quantity);
                questItem.transform.SetParent(RewardsContainer.transform, false);
            }

            // Checa se adventurer já tem quest
            if (Adventurer.Quests.Contains(quest))
            {
                AcceptQuest.gameObject.SetActive(false);
                CompleteQuest.gameObject.SetActive(true);

                // Validate if it already be completed
                if (quest.Validate(Adventurer))
                    CompleteQuest.interactable = true;
                else
                    CompleteQuest.interactable = false;
            }
            else
            {
                AcceptQuest.gameObject.SetActive(true);
                CompleteQuest.gameObject.SetActive(false);
            }
        }

        public void RebuildLayout(GameObject gameObject)
        {
            StartCoroutine(WaitOneFrameThenRebuild(gameObject));
        }

        private IEnumerator WaitOneFrameThenRebuild(GameObject gameObject)
        {
            gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            gameObject.SetActive(true);
        }

        public void UpdateQuests(List<Quest> quests)
        {
            if (SelectedQuest.QuestSO == null && quests.Count > 0)
            {
                SelectQuest(quests[0]);
                QuestInfoGameObject.SetActive(true);
            }

            if (SelectedQuest.QuestSO == null) QuestInfoGameObject.SetActive(false);

            if (QuestsUIList.Count > 0)
            foreach(QuestUIElement questUIElement in QuestsUIList)
            {
                questUIElement.Release();
            }

            QuestsUIList.Clear();

            foreach (Quest quest in quests)
            {
                QuestUIElement questUI = PoolManager.Singleton.Get(questUIElementPrefab) as QuestUIElement;

                questUI.Initialize(quest, this);

                QuestsUIList.Add(questUI);
            }
        }

        public void Sync()
        {
            UpdateQuests(Quests);
            SelectQuest(SelectedQuest);
        }
    }
}
