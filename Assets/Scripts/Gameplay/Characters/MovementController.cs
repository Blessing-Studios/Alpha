using UnityEngine;
using Unity.Netcode;
using Blessing;

[RequireComponent(typeof(CharacterController))]
public abstract class MovementController : NetworkBehaviour
{
    // protected ClientNetworkTransform clientNetworkTransform;
    public float CharacterSpeed = 2.0f;
    public float Gravity;
    public float GroundedGravity;
    public Vector3 AttackMovement = Vector3.zero;
    public Vector2 direction; // Talvez deletar essa variável
    protected bool canMove;
    protected Animator animator;

    // Delcare reference variables
    // private PlayerInput playerInput;
    // 

    // private InputAction moveAction;
    // protected InputActionMap characterControlsMap;
    protected CharacterController characterController;
    [SerializeField] protected bool isPushable = true;
    protected bool isMovementPressed;
    protected bool isJumpPressed;
    protected int isWalkingHash;
    protected int isRunningHash;
    protected int speedHash;
    protected float characterSpeed;
    // Variables to store player input values
    [SerializeField] protected Vector2 currentMovementInput;

    // The child Classes give value to currentMovement
    protected Vector3 currentMovement = Vector3.zero;

    // ###################### Teste
    public Vector3 CurrentMovement;
    public bool CanMove;
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

        GroundedGravity = GameManager.Singleton.GroundGravity;
        Gravity = GameManager.Singleton.Gravity;

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        HandleGravity();
        HandleMovement(canMove);
        // HandleColision();
        // HandlePushBack();
        HandleFacing();

        // Para Testar
        CurrentMovement = currentMovement;
        CanMove = canMove;
    }

    public virtual void DisableMovement()
    {
        canMove = false;
    }

    public virtual void EnableMovement()
    {
        canMove = true;
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
            currentMovement.x = 0.0f;
            currentMovement.z = 0.0f;
        }

        // Direction é útil para debugar
        direction.x = currentMovement.x;
        direction.y = currentMovement.z;

        characterController.Move( new Vector3
        (
            currentMovement.x * Time.deltaTime * CharacterSpeed,
            currentMovement.y,
            currentMovement.z * Time.deltaTime * CharacterSpeed
        ));

        characterSpeed = characterController.velocity.magnitude;
        animator.SetFloat(speedHash , characterController.velocity.magnitude);
    }

    protected void HandleColision()
    {
        
    }
    protected virtual void HandleFacing()
    {
        if (currentMovement.x < 0.0f) 
        {
            transform.rotation = Quaternion.LookRotation(-Vector3.forward);
        }
        if (currentMovement.x > 0.0f)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }
    }

    public virtual void HandlePushBack( Vector3 pushMovement)
    {
        // characterController.Move(pushMovement);
    }
    public virtual void HandleAttackMovment()
    {
        Quaternion rotation = this.gameObject.transform.rotation;
        characterController.Move(rotation * AttackMovement * Time.deltaTime * CharacterSpeed);
    }

    public void DisableCollision()
    {
        // GetCharacterController().excludeLayers = LayerMask.GetMask("Player");
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
