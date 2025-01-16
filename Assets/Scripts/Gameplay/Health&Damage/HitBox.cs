using UnityEngine;
using Unity.Netcode;
// This Monobehavier is made to be in a HitBox or a Projectile object inside a character
// The HitBox is espected to has a Rigbody to the hit detection and OnTriggerEnter work

namespace Blessing.Gameplay.HealthAndDamage
{
    public class HitBox : NetworkBehaviour
    {
        [field: SerializeField] public bool ShowDebug { get; private set; }
        
        [Header("Hit Box Settings")]
        [Tooltip("Owner of this HitBox")][field: SerializeField] public GameObject OwnerGameObject { get; private set; }
        public IHitter Owner { get; private set; }
        // [Tooltip("Character that owns this HitBox")][SerializeField] private Character character;
        [Tooltip("Will only hit this Layer Mask")][SerializeField] private LayerMask layerMask;
        [Tooltip("CapsuleCollider of the hitbox")][SerializeField] private CapsuleCollider capsuleCollider;
        [Tooltip("Controls how much the hitbox will be cast in the frame")][SerializeField] private float thickness = 0.1f;

        public Vector3 ColliderCenter { get; private set; }
        private Vector3 gizmoP1;
        private Vector3 gizmoP2;
        private float gizmoRadius;

        void Awake()
        {
            // Check if the fields are filled
            if (OwnerGameObject == null)
                Debug.LogWarning("Owner Field is required: " + gameObject.transform.parent.name);

            if (capsuleCollider == null)
                Debug.LogWarning("capsuleCollider Field is required: " + gameObject.transform.parent.name);

            if (thickness <= 0)
                Debug.LogWarning("thickness Field needs to be greater than zero: " + gameObject.transform.parent.name);

            Owner = OwnerGameObject.GetComponent<IHitter>();
            if (Owner == null)
                Debug.LogError("Owner must has IHitter interface: " + gameObject.transform.parent.name);

            gameObject.SetActive(false);
            
        }

        void Update()
        {
            CheckHit();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawSphere(gizmoP1, gizmoRadius);
            Gizmos.DrawSphere(gizmoP2, gizmoRadius);
        }
        public void CheckHit()
        {
            Vector3 center = transform.TransformPoint(capsuleCollider.center) - transform.right * thickness;
            ColliderCenter = center;
            float height = capsuleCollider.height * transform.lossyScale.z;
            float radius = capsuleCollider.radius * transform.lossyScale.x;
            int intDirection = capsuleCollider.direction;
            Vector3 capsuleDirection;

            // Set the direction of the capsule
            switch (intDirection)
            {
                case 0:
                    capsuleDirection = Vector3.right;
                    break;
                case 1:
                    capsuleDirection = Vector3.up;
                    break;
                case 2:
                    capsuleDirection = Vector3.forward;
                    break;
                default:
                    capsuleDirection = Vector3.forward;
                    break;
            }

            // Set the gizmos variables
            Vector3 p1 = center + capsuleDirection * (height - radius) / 2;
            Vector3 p2 = center - capsuleDirection * (height - radius) / 2;

            gizmoP1 = p1;
            gizmoP2 = p2;
            gizmoRadius = radius;

            // CapsuleCast the hitbox
            RaycastHit[] hits = Physics.CapsuleCastAll
                (
                    p1,
                    p2,
                    radius,
                    transform.right,
                    thickness,
                    layerMask
                );

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject.TryGetComponent(out HurtBox hurtBox))
                {
                    if (Owner.Hit(hurtBox.Owner))
                    {
                        // Pegar informação do dano e mantar para o target
                        hurtBox.Owner.GotHit(Owner);
                    }
                }

                /** Temporariamente comentado
                HurtBox hurtBox = hit.collider.gameObject?.GetComponent<HurtBox>();
                if (hurtBox == null)
                    continue;

                
                foreach (string ememiesGroup in character.EmemiesGroups)
                {
                    if (
                        hurtBox.Character &&
                        hurtBox.Character.CompareTag(ememiesGroup) &&
                        !character.TargetsList.Contains(hurtBox.Character.gameObject))
                    {
                        // Trigger attack state
                        character.GetCurrentMeleeState().OnTrigger(hurtBox, hit);

                        // The Attack logic is happening in the Character.cs class
                    }
                }
                **/
            }
        }
    }

}
