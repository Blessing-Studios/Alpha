using System.Collections.Generic;
using Blessing.Gameplay.Guild;
using Blessing.Gameplay.Guild.Quests;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI.Quests
{
    public class QuestBoardMenu : MonoBehaviour
    {
        private List<Button> buttons = new();
        [SerializeField] private Button QuestPrefab;
        public void AddQuests(List<Quest> quests, Adventurer adventurer)
        {
            if (gameObject.activeSelf == true)
            {
                foreach (Button button in buttons)
                {
                    Destroy(button.gameObject);
                }
                buttons.Clear();

                gameObject.SetActive(false);
                return;
            }

            foreach (Quest quest in quests)
            {
                Button questButton = Instantiate(QuestPrefab, this.transform);

                questButton.GetComponentInChildren<TextMeshProUGUI>().text = quest.Label;

                // Checa se adventurer já tem quest
                if (adventurer.Quests.Contains(quest))
                {
                    // Validate if can be completed
                    if (quest.Validate(adventurer))
                    {
                        questButton.GetComponent<Image>().color = Color.green;
                        questButton.onClick.AddListener(() =>
                        {
                            GuildManager.Singleton.CompleteQuest(adventurer, quest.Id);

                            foreach (Button button in buttons)
                            {
                                Destroy(button.gameObject);
                            }
                            buttons.Clear();

                            gameObject.SetActive(false);
                        });
                    }
                    else
                    {
                        questButton.interactable = false;
                    }
                }
                else
                {
                    questButton.onClick.AddListener(() =>
                    {
                        GuildManager.Singleton.TakeQuest(adventurer, quest.Id);

                        foreach (Button button in buttons)
                        {
                            Destroy(button.gameObject);
                        }
                        buttons.Clear();

                        gameObject.SetActive(false);
                    });
                }

                if (adventurer.IsQuestDone(quest.Id))
                {
                    questButton.GetComponent<Image>().color = Color.blue;
                    questButton.interactable = false;
                }

                buttons.Add(questButton);
                gameObject.SetActive(true);
            }
        }
    }
}
