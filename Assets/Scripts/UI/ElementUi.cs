using Blessing.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using Blessing.Audio;

namespace Blessing.UI
{
    public class ElementUi : MonoBehaviour, IPointerEnterHandler, ISelectHandler
    {
        [SerializeField] AudioClip selectButtonAudioClip;
        [Range(0.0f, 1.0f)] public float Volume = 1.0f;

        public void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Singleton.SetSelectedGameObject(gameObject); 
        }

        public void OnSelect(BaseEventData eventData)
        {
            AudioManager.Singleton.PlayUiSound(selectButtonAudioClip, Volume);
        }
    }
}