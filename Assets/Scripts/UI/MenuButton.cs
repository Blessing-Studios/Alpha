using Blessing.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using Blessing.Audio;
using System.Collections;
using UnityEngine.UI;

namespace Blessing.UI
{
    [RequireComponent(typeof(Image))]
    public class MenuButton : ElementUi
    {
        private Image buttonImage;
        [SerializeField][Range(0, 1f)] private float fadedAlpha = 0.0f;
        [SerializeField] private float fadeSpeed = 1f;
        [SerializeField] private float initialAlpha;

        void Awake()
        {
            buttonImage = GetComponent<Image>();
            initialAlpha = buttonImage.color.a;

            buttonImage.color = new Color(
                    buttonImage.color.r,
                    buttonImage.color.g,
                    buttonImage.color.b,
                    fadedAlpha
            );
        }

        void OnDestroy()
        {
            StopAllCoroutines();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            StopAllCoroutines();
            StartCoroutine(FadeOut());   
        }

        private IEnumerator FadeOut()
        {
            float time = 0;

            while (buttonImage.color.a > fadedAlpha)
            {
                buttonImage.color = new Color(
                    buttonImage.color.r,
                    buttonImage.color.g,
                    buttonImage.color.b,
                    Mathf.Lerp(initialAlpha, fadedAlpha, time * fadeSpeed)
                );

                time += Time.deltaTime;
                yield return null;
            }

            StopAllCoroutines();
        }

        private IEnumerator FadeIn()
        {
            float time = 0;

            while (buttonImage.color.a < initialAlpha)
            {
                buttonImage.color = new Color(
                    buttonImage.color.r,
                    buttonImage.color.g,
                    buttonImage.color.b,
                    Mathf.Lerp(fadedAlpha, initialAlpha, time * fadeSpeed)
                );

                time += Time.deltaTime;
                yield return null;
            }

            StopAllCoroutines();
        }
    }
}