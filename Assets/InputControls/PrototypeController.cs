using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrototypeController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private Vector2 moveInput;
    [SerializeField]private Rigidbody rb;

    [Range(0.1f, 100f)]
    [SerializeField] private float speed = 5f;
 
#region Callbacks
    private Action<InputAction.CallbackContext> movePerformedCallback;
    private Action<InputAction.CallbackContext> moveCanceledCallback;
        
    private bool subscribed = false; //<- simpler than 10 lines of invasive code on a locked event system

    private void InitActionCallbacks()
    {
        //[Personal comment]: ctx reads like an acronym not a word, 7 letter word doesn't need a 3 letter shortform
        movePerformedCallback = context => moveInput = context.ReadValue<Vector2>();
        moveCanceledCallback = context => moveInput = Vector2.zero;
    }

    private void ActionSubscribe()
    {
        if (subscribed) return;
        
        //Since using project wide inputs, need to store the context added here so can be removed at disable
        playerInput.actions["Move"].performed += movePerformedCallback;
        playerInput.actions["Move"].canceled += moveCanceledCallback;
        subscribed = true;
    }
    
    private void ActionUnubscribe()
    {
        if (!subscribed) return; //<- kind of moot but this is a custom event so putting in to be safe and tidy
        
        playerInput.actions["Move"].performed -= movePerformedCallback;
        playerInput.actions["Move"].canceled -= moveCanceledCallback;
        subscribed = false;
    }
#endregion Callbacks
 
#region Runtime Functions
    void Awake()
    {
        if (!playerInput)
        {
            Debug.LogWarning("Component missing assignment: PrototypeController on Player GameObject: PlayerInput not assigned");
            playerInput = GetComponent<PlayerInput>();
            if (!playerInput)
            {
                Debug.LogError("FATAL: Component missing: PlayerInput on Player GameObject");
                Application.Quit();
            }
        }

        if (!rb)
        {
            Debug.LogWarning("Component missing assignment: PrototypeController on Player GameObject: Rigidbody not assigned");
            rb = GetComponent<Rigidbody>();
            if (!rb)
            {
                Debug.LogError("FATAL: Component missing: Rigidbody on Player GameObject");
                Application.Quit();
            }
        }

        InitActionCallbacks();
    }
    
    private void OnEnable()
    {
        
        //using project wide input action asset, left in case swap to not project wide asset
        //playerInput.actions.Enable();
        ActionSubscribe();
    }

    //TODO: should be in game manager to halt all input actions, and all action subscriptions should go through there
    //since this is a bare bones test prototype we do it here
    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            ActionUnubscribe();
        }
        else
        {
            ActionSubscribe();
        }
    }
    

    private void OnDisable()
    {
        //using project wide input action asset, left in case swap to not project wide asset
        //playerInput.actions.Disable();
        
       ActionUnubscribe();
    }

    void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
#endregion Runtime Functions
}
