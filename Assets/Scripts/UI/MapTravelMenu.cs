using System;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.GameData;
using Blessing.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI
{
    public class MapTravelMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [SerializeField] private Button goToTavernButton;
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference TavernScene;
        private SceneReference tavernScene { get { return TavernScene.value as SceneReference; } }

        [SerializeField] private Button goToTowerButton;
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference TowerScene;
        private SceneReference towerScene { get { return TowerScene.value as SceneReference; } }

        void Awake()
        {
            goToTavernButton.onClick.AddListener(() =>
            {
                // goToTavernButton.interactable = false;

                // Dois casos, single player e multiplayer

                // Multiplayer
                //  * Desconectar da sessão

                // * Criar uma sessão nova

                //  * Remover Cena que está saindo
                //  * Carregar Cena nova

                // GameplayEventHandler.MapTravelTriggered("abcd", "Teste12345", tavernScene);
                

                GameplayEventHandler.MapTravelTriggered("abc", Guid.NewGuid().ToString()[..8], tavernScene);
            });

            goToTowerButton.onClick.AddListener(() =>
            {
                // goToTowerButton.interactable = false;
                GameplayEventHandler.MapTravelTriggered("abc", Guid.NewGuid().ToString()[..8], towerScene);

            });
        }
    }
}
