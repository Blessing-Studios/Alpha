using Blessing.Core.ScriptableObjectDropdown;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Blessing.UI
{
    public class BottonMenu : MonoBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        
        [SerializeField] private Button patchNotesButton;
        [SerializeField] private Button quitButton;
        
        void Awake() 
        {
            patchNotesButton.onClick.AddListener(() => {
                Debug.Log("patchNotesButton");
                Debug.Log("patchNotesButton: not done");
            });

            quitButton.onClick.AddListener(() => { 
                Debug.Log("QuitButton");
                Application.Quit(); 
            });
        }
    }
}

