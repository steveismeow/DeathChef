using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    public Vector2 RawMoveInput { get; private set; }
    public Vector2 RawAimInput { get; private set; }


    public bool MovementInput { get; private set; }
    public bool AimInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool InteractionInput { get; private set; }
    public bool SelectInput { get; private set; }
    public bool CancelInput { get; private set; }
    public bool PauseInput { get; private set; }





    #region Move
    public void Move(InputAction.CallbackContext context)
    {

        RawMoveInput = context.ReadValue<Vector2>();

        if (context.started)
        {
            MovementInput = true;
        }


        if (context.canceled)
        {
            MovementInput = false;
        }
    }
    #endregion

    #region Aim
    public void Aim(InputAction.CallbackContext context)
    {

        RawAimInput = context.ReadValue<Vector2>();

        if (context.started)
        {
            AimInput = true;
        }


        if (context.canceled)
        {
            AimInput = false;
        }
    }
    #endregion

    #region Attack
    public void Attack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            AttackInput = true;
        }

        if (context.canceled)
        {
            AttackInput = false;
        }
    }

    public void UseAttackInput() => AttackInput = false;
    #endregion

    #region Interact
    public void Interact(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InteractionInput = true;
        }

        if (context.canceled)
        {
            InteractionInput = false;
        }
    }

    public void UseInteractionInput() => InteractionInput = false;
    #endregion

    #region Select
    public void Select(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SelectInput = true;
        }

        if (context.canceled)
        {
            SelectInput = false;
        }
    }

    public void UseSelectInput() => SelectInput = false;
    #endregion

    #region Cancel
    public void Cancel(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            CancelInput = true;
        }

        if (context.canceled)
        {
            CancelInput = false;
        }
    }

    public void UseCancelInput() => CancelInput = false;
    #endregion

    #region Pause
    public void Pause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PauseInput = true;
        }

        if (context.canceled)
        {
            PauseInput = false;
        }
    }

    public void UsePauseInput() => PauseInput = false;
    #endregion


}
