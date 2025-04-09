using Blessing.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using Blessing.Audio;

namespace Blessing.UI
{
    public class ElementUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
    {
        [SerializeField] AudioClip selectButtonAudioClip;
        [Range(0.0f, 1.0f)] public float Volume = 1.0f;

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            GameManager.Singleton.SetSelectedGameObject(gameObject); 
        }

        public virtual void OnPointer(PointerEventData eventData)
        {
            GameManager.Singleton.SetSelectedGameObject(gameObject); 
        }
        public virtual void OnPointerExit(PointerEventData eventData)
        {
            // throw new System.NotImplementedException();
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (selectButtonAudioClip != null)
                AudioManager.Singleton.PlayUiSound(selectButtonAudioClip, Volume);
        }
    }
}