using System;
using System.Collections;
using System.Collections.Generic;
using Blessing;
using UnityEngine;

namespace Blessing.Gameplay
{
    public class FadeObjectsBlocking : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private Transform target;
        [SerializeField][Range(0, 1f)] private float fadedAlpha = 0.33f;
        [SerializeField] private bool retainShadows = true;
        [SerializeField] private Vector3 targetPositionOffset = Vector3.up;
        [SerializeField] private float fadeSpeed = 1f;

        [Header("Read Only Data")]
        [SerializeField] private List<FadingObject> objectsBlockingView = new();
        private Dictionary<FadingObject, Coroutine> runningCoroutines = new();

        private RaycastHit[] hits = new RaycastHit[5];

        private void Start()
        {
            StartCoroutine(CheckForObjects());
        }

        private IEnumerator CheckForObjects()
        {
            while (true)
            {
                Vector3 directionToCam = GameManager.Singleton.MainCamera.transform.position - transform.position;
                int numHits = Physics.RaycastNonAlloc(
                        transform.position,
                        directionToCam,
                        hits,
                        directionToCam.magnitude,
                        layerMask
                );

                if (numHits > 0)
                {
                    for (int i = 0; i < numHits; i++)
                    {
                        FadingObject fadingObject = GetFadingObjectFromHit(hits[i]);

                        if (fadingObject != null && !objectsBlockingView.Contains(fadingObject))
                        {
                            if (runningCoroutines.ContainsKey(fadingObject))
                            {
                                if (runningCoroutines[fadingObject] != null)
                                {
                                    StopCoroutine(runningCoroutines[fadingObject]);
                                }

                                runningCoroutines.Remove(fadingObject);
                            }

                            runningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectOut(fadingObject)));
                            objectsBlockingView.Add(fadingObject);
                        }
                    }
                }

                FadeObjectsNoLongerBeingHit();

                ClearHits();

                yield return null;
            }
        }

        private void FadeObjectsNoLongerBeingHit()
        {
            List<FadingObject> objectsToRemove = new(objectsBlockingView.Count);

            foreach (FadingObject fadingObject in objectsBlockingView)
            {
                bool objectIsBeingHit = false;
                for (int i = 0; i < hits.Length; i++)
                {
                    FadingObject hitFadingObject = GetFadingObjectFromHit(hits[i]);
                    if (hitFadingObject != null && fadingObject == hitFadingObject)
                    {
                        objectIsBeingHit = true;
                        break;
                    }
                }

                if (!objectIsBeingHit)
                {
                    if (runningCoroutines.ContainsKey(fadingObject))
                        if (runningCoroutines[fadingObject] != null)
                        {
                            {
                                StopCoroutine(runningCoroutines[fadingObject]);
                            }
                            runningCoroutines.Remove(fadingObject);
                        }

                    runningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectIn(fadingObject)));
                    objectsToRemove.Add(fadingObject);
                }
            }

            foreach (FadingObject removeObject in objectsToRemove)
            {
                objectsBlockingView.Remove(removeObject);
            }
        }

        private IEnumerator FadeObjectOut(FadingObject fadingObject)
        {
            foreach (Material material in fadingObject.Materials)
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_Surface", 1);

                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                material.SetShaderPassEnabled("DepthOnly", false);
                material.SetShaderPassEnabled("SHADOWCASTER", retainShadows);

                material.SetOverrideTag("TenderType", "Transparent");

                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            float time = 0;

            while (fadingObject.Materials[0].color.a > fadedAlpha)
            {
                foreach (Material material in fadingObject.Materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        material.color = new Color(
                            material.color.r,
                            material.color.g,
                            material.color.b,
                            Mathf.Lerp(fadingObject.InitialAlpha, fadedAlpha, time * fadeSpeed)
                        );
                    }
                }

                time += Time.deltaTime;
                yield return null;
            }

            if (runningCoroutines.ContainsKey(fadingObject))
            {
                StopCoroutine(runningCoroutines[fadingObject]);
                runningCoroutines.Remove(fadingObject);
            }
        }

        private IEnumerator FadeObjectIn(FadingObject fadingObject)
        {
            Debug.Log(gameObject.name + ": Teste FadeObjectIn");
            float time = 0;

            while (fadingObject.Materials[0].color.a < fadingObject.InitialAlpha)
            {
                foreach (Material material in fadingObject.Materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        material.color = new Color(
                            material.color.r,
                            material.color.g,
                            material.color.b,
                            Mathf.Lerp(fadedAlpha, fadingObject.InitialAlpha, time * fadeSpeed)
                        );
                    }
                }

                time += Time.deltaTime;
                yield return null;
            }

            foreach (Material material in fadingObject.Materials)
            {
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.SetInt("_Surface", 0);

                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;

                material.SetShaderPassEnabled("DepthOnly", true);
                material.SetShaderPassEnabled("SHADOWCASTER", true);

                material.SetOverrideTag("TenderType", "Opaque");

                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            if (runningCoroutines.ContainsKey(fadingObject))
            {
                StopCoroutine(runningCoroutines[fadingObject]);
                runningCoroutines.Remove(fadingObject);
            }
        }

        private void ClearHits()
        {
            Array.Clear(hits, 0, hits.Length);
        }

        private FadingObject GetFadingObjectFromHit(RaycastHit hit)
        {
            return hit.collider != null ? hit.collider.GetComponent<FadingObject>() : null;
        }
    }
}
