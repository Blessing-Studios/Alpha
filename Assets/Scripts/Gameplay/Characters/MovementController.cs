using UnityEngine;
using Unity.Netcode;
using Blessing;
using System.Collections;
using NUnit.Framework.Internal;
using System;

[RequireComponent(typeof(CharacterController))]
public abstract class MovementController : NetworkBehaviour
{
    // protected ClientNetworkTransform clientNetworkTransform;
    public float CharacterSpeed = 2.0f;
    public float Gravity;
    public float GroundedGravity;
    public Vector3 AttackMovement = Vector3.zero;
    public Vector2 direction; // Talvez deletar essa variável
    [SerializeField] protected bool canMove;
    public bool CanMove { get { return canMove; } }
    [SerializeField] protected bool canFace;
    public bool CanFace { get { return canFace; } }
    protected Animator animator;

    // Delcare reference variables
    // private PlayerInput playerInput;
    // 

    // private InputAction moveAction;
    // protected InputActionMap characterControlsMap;
    protected CharacterController characterController;
    [SerializeField] protected bool isPushable = true;
    [field: SerializeField] protected NetworkVariable<bool> isFacingRight = new NetworkVariable<bool>(true);
    public bool IsFacingRight { get { return isFacingRight.Value; } set { isFacingRight.Value = value; } }
    [SerializeField] protected bool isMovementPressed;
    protected bool isJumpPressed;
    protected int isWalkingHash;
    protected int isRunningHash;
    protected int speedHash;
    protected float oldPositionX;
    protected float speedModifier = 1f;
    public float ScriptablepeedModifier { get { return speedModifier; } }
    protected const float minDeltaX = 0.001f;

    // Variables to store player input values
    [SerializeField] protected Vector2 currentMovementInput;

    // The child Classes give value to currentMovement
    [SerializeField] protected Vector3 currentMovement = Vector3.zero;

    // ###################### Teste
    public Vector3 CurrentMovement;
    public CharacterController GetCharacterController()
    {
        return this.characterController;
    }

    public Vector2 GetCurrentMovementInput()
    {
        return currentMovementInput;
    }

    // Awake is called earlier than Start
    protected virtual void Awake()
    {
        // clientNetworkTransform = GetComponent<ClientNetworkTransform>();

        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        speedHash = Animator.StringToHash("Speed");

        canMove = true;
        canFace = true;

        GroundedGravity = GameManager.Singleton.GroundGravity;
        Gravity = GameManager.Singleton.Gravity;

    }

    public override void OnNetworkSpawn()
    {
        isFacingRight.OnValueChanged += OnNetworkIsFacingRightChanged;
    }

    private void OnNetworkIsFacingRightChanged(bool previousValue, bool newValue)
    {
        if (HasAuthority) return;

        if (newValue)
            transform.rotation = Quaternion.LookRotation(-Vector3.forward);
        else
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!HasAuthority) return;

        HandleGravity();
        HandleMovement(canMove);
        // HandleColision();
        // HandlePushBack();
        HandleFacing();

        // Para Testar
        CurrentMovement = currentMovement;
    }

    protected virtual void Start()
    {
        oldPositionX = transform.position.x;

        if (IsFacingRight)
            transform.rotation = Quaternion.LookRotation(-Vector3.forward);
        else
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
    }

    public virtual void DisableMovement()
    {
        canMove = false;
    }

    public virtual void EnableMovement(bool resetSpeed = true)
    {
        if (resetSpeed) ResetSpeedModifier();

        canMove = true;
    }

    public void SetSpeedModifier(float value, bool canFace = true)
    {
        speedModifier = value;
        this.canFace = canFace;
    }

    public void ResetSpeedModifier()
    {
        speedModifier = 1f;
        canFace = true;
    }

    protected virtual void HandleGravity()
    {
        if (characterController.isGrounded)
            currentMovement.y = GroundedGravity;
        else
            currentMovement.y = Gravity;
    }

    protected virtual void HandleMovement(bool canMove)
    {
        if (!canMove)
        {
            animator.SetBool(isWalkingHash, false);
            RestCurrentMovement();
        }

        // Direction é útil para debugar
        direction.x = currentMovement.x;
        direction.y = currentMovement.z;

        characterController.Move(new Vector3
        (
            currentMovement.x * Time.deltaTime * CharacterSpeed * speedModifier,
            currentMovement.y,
            currentMovement.z * Time.deltaTime * CharacterSpeed * speedModifier
        ));

        animator.SetFloat(speedHash, characterController.velocity.magnitude);
    }

    public void RestCurrentMovement()
    {
        currentMovement.x = 0.0f;
        currentMovement.z = 0.0f;
    }

    protected void HandleCollision()
    {

    }

    private Coroutine impulseMovementCoroutine;
    protected IEnumerator ImpulseMovement(Vector3 direction, float impact, float impulseTime)
    {
        float time = 0;
        impact = impact > 20 ? 20 : impact;
        impulseTime = impulseTime > 20 ? 20 : impulseTime;

        // float time2 = 0;

        while (time < impulseTime && impact > 0.000001f)
        {
            impact = Mathf.Lerp(impact, 0, time / impulseTime);
            characterController.Move(impact * direction * Time.deltaTime);
            time += Time.deltaTime;
            // time2 += Time.deltaTime;

            // if (time2 > 0.2f)
            // {
            //     Debug.Log("Teste: time - " + time);
            //     Debug.Log("Teste: impulseSpeed - " + impact);
            //     time2 = 0;
            // }


            yield return null;
        }

        StopAllCoroutines();
    }
    protected virtual void HandleFacing()
    {
        float currentPositionX = transform.position.x;
        float xDelta = currentPositionX - oldPositionX;

        if (canFace)
        {
            if (xDelta < -minDeltaX)
            {
                transform.rotation = Quaternion.LookRotation(-Vector3.forward);
                if (HasAuthority) isFacingRight.Value = true;
            }
            else if (xDelta > minDeltaX)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward);
                if (HasAuthority) isFacingRight.Value = false;
            }
        }

        oldPositionX = currentPositionX;
    }
    public virtual void HandlePushBack(Vector3 direction, float impact, float impulseTime)
    {
        if (impulseMovementCoroutine != null)
        {
            StopCoroutine(impulseMovementCoroutine);
        }

        if (impact <= 0 || impulseTime <= 0)
        {
            return;
        }

        Coroutine coroutine = StartCoroutine(ImpulseMovement(direction, impact, impulseTime));
    }
    public virtual void HandleAttackMovement()
    {
        Quaternion rotation = this.gameObject.transform.rotation;
        characterController.Move(rotation * AttackMovement * Time.deltaTime * CharacterSpeed);
    }

    public void DisableCollision()
    {
        Debug.Log("DisableCollision()");
        GetCharacterController().excludeLayers = LayerMask.GetMask("Characters");
    }



    // this script pushes all rigidbodies that the character touches
    /**
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        
        // Check if it is a character and if it is pushable, return if false
        if (!hit.collider.gameObject.TryGetComponent<Character>(out Character otherCharacter) ||
            !otherCharacter.GetMovementController().isPushable)
        {
            return;
        }

        // We dont want to push objects below us
        // if (hit.moveDirection.y < -0.3)
        // {
        //     return;
        // }

        float characterForce = character.Strength * character.Weight;
        float otherCharacterForce = otherCharacter.Strength * otherCharacter.Weight;
        if (characterForce > otherCharacterForce)
        {
            float restultForce = (1f - otherCharacterForce / characterForce);
            Vector3 otherCharacterMovement = -hit.normal + new Vector3(0f, 0f, GroundedGravity);
            otherCharacter.GetMovementController().HandlePushBack( otherCharacterMovement * Time.deltaTime * CharacterSpeed * restultForce);
        }
    }
    **/
}
