using Blessing.Core.ObjectPooling;
using Blessing.Gameplay.Guild.Quests;
using Blessing.Gameplay.TradeAndInventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI.Quests
{
    public class QuestUIElement : PooledObject
    {
        public Image Icon;
        public TextMeshProUGUI RankText;
        public TextMeshProUGUI NameText;
        public Button DetailButton;

        public void Initialize(Quest quest, QuestsUI questsUI)
        {
            RankText.text = $"Rank {quest.Rank.Label}";
            NameText.text = quest.Label;
            Icon.sprite = quest.Icon;

            DetailButton.onClick.AddListener(() => {
                    questsUI.SelectQuest(quest);
            });
            transform.localScale = Vector3.one;
            transform.SetParent(questsUI.QuestsContainer.transform, false);
        }

        public override void GetFromPool()
        {
            gameObject.SetActive(true);
        }
        public override void ReleaseToPool()
        {
            DetailButton.onClick.RemoveAllListeners();
            gameObject.SetActive(false);
        }

        public override void DestroyPooledObject()
        {
            Destroy(gameObject);
        }
    }
}
