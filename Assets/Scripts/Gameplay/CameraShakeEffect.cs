using Unity.Cinemachine;
using UnityEngine;

namespace Blessing.Gameplay
{

    [CreateAssetMenu(fileName = "CameraShakeEffect", menuName = "Scriptable Objects/Camera/ShakeEffect")]
    public class CameraShakeEffect : ScriptableObject
    {
        [Header("Impulse Definition")]
        [SerializeField] private CinemachineImpulseDefinition impulseDefinition = new CinemachineImpulseDefinition
        {
            ImpulseChannel = 1,
            ImpulseShape = CinemachineImpulseDefinition.ImpulseShapes.Bump,
            CustomImpulseShape = new AnimationCurve(),
            ImpulseDuration = 0.2f,
            ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform,
            DissipationDistance = 100f,
            DissipationRate = 0.25f,
            PropagationSpeed = 343f
        };
        [Header("Impulse Source Settings")]
        public float ImpactForce = 1f;
        public Vector3 DefaultVelocity = new(0f, -1f, 0f);

        [Header("Impulse Listener Settings")]
        public float ListenerAmplitude = 1f;
        public float ListenerFrequency = 1f;
        public float ListenerDuration = 1f;

        public CinemachineImpulseDefinition ImpulseDefinition
        {
            get 
            {
                return impulseDefinition; 
            }
        }
    }
}