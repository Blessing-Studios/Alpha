using System;
using Blessing.Core.ScriptableObjectDropdown;
using Blessing.GameData;
using Blessing.Player;
using Blessing.Services;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Blessing.Gameplay.Transition
{
    [Serializable]
    public struct MapTravelData : IEquatable<MapTravelData>, INetworkSerializeByMemcpy
    {
        public FixedString64Bytes Scene;
        public FixedString64Bytes Session;

        public MapTravelData(string scene, string session)
        {
            Scene = new FixedString32Bytes(scene);
            Session = new FixedString32Bytes(session);
        }

        public bool Equals(MapTravelData other)
        {
            return
                (this.Scene == other.Scene) &&
                (this.Session == other.Session);
        }
    }
    public class MapTravel : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [ScriptableObjectDropdown(typeof(SceneReference))] public ScriptableObjectReference MapScene;
        private SceneReference mapScene { get { return MapScene.value as SceneReference; } }
        void OnTriggerEnter(Collider collider)
        {
            if (ShowDebug) Debug.Log(gameObject.name + ": TriggerEnter funcionou");

            //Exit if it is not the PlayerCharacter owned
            if (collider.TryGetComponent<PlayerCharacter>(out PlayerCharacter character) && character.HasAuthority)
            {
                character.Network.IsTraveling = true;

                string playerName = GameDataManager.Singleton.PlayerName;
                string sessionName = GameDataManager.Singleton.GetSessionByScene(mapScene);

                GameplayEventHandler.MapTravelTriggered(playerName, sessionName, mapScene);
            }
        }
    }
}
