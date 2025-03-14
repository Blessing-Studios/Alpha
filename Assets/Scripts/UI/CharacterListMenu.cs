using Blessing.Core.ScriptableObjectDropdown;
using Blessing.GameData;
using Blessing.Services;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI
{
    public class CharacterListMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        [SerializeField] private Button createNewButton;
        [SerializeField] private Button characterSlotPrefab;
        [SerializeField] private GameObject firstSelectedGameObject;

        void Awake()
        {
            createNewButton.onClick.AddListener(() =>
            {
                Debug.Log("createNewButton");

                Button newCharacter = Instantiate(characterSlotPrefab, this.transform);

                newCharacter.onClick.AddListener(() =>
                {
                    Debug.Log("newCharacter.onClick");
                });
            });
        }

        void Start()
        {
            if (firstSelectedGameObject == null)
            {
                Debug.LogError(gameObject.name + ": firstSelectedGameObject is missing");
            }

            // Call GameManager to set Selected GameObject
            GameManager.Singleton.SetSelectedGameObject(firstSelectedGameObject);
        }
    }
}
