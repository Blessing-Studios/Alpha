using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blessing.Gameplay
{
    public class FadingObject : MonoBehaviour, IEquatable<FadingObject>
    {
        public List<Renderer> Renderers = new List<Renderer>();
        public Vector3 Position;
        public List<Material> Materials = new List<Material>();
        public float InitialAlpha;
        public void Awake()
        {
            Position = transform.position;

            if (Renderers.Count == 0)
            {
                Renderers.AddRange(GetComponentsInChildren<Renderer>());
            }

            foreach (Renderer renderer in Renderers)
            {
                Materials.AddRange(renderer.materials);
            }

            InitialAlpha = Materials[0].color.a;
        }
        public bool Equals(FadingObject other)
        {
            return Position.Equals(other.Position);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}
