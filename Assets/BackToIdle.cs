using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackToIdle : StateMachineBehaviour
{
    [Header("Mascot Controller Reference")]
    public MascotStatus mascotStatus;
    
    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (mascotStatus != null)
        {
            // Show the specific target object
            mascotStatus.ShowTargetObject();
            
            // Hide all other mascot objects
            mascotStatus.HideAllTargets();
            
            if (showDebugInfo)
            {
                Debug.Log("BackToIdle: Executed ShowTargetObject and HideAllTargets via MascotStatus");
            }
        }
        else
        {
            Debug.LogError("BackToIdle: MascotStatus reference is not assigned! Please assign it in the inspector.");
        }
    }
    
    /// <summary>
    /// Public method to be called by AnimationEvent 'HideAllProps'
    /// This method hides all mascot objects using the MascotStatus script
    /// </summary>
    public void HideAllProps()
    {
        if (mascotStatus != null)
        {
            mascotStatus.HideAllTargets();
            
            if (showDebugInfo)
            {
                Debug.Log("BackToIdle: Hidden all props via MascotStatus");
            }
        }
        else
        {
            Debug.LogError("BackToIdle: Cannot hide props - MascotStatus reference is not assigned!");
        }
    }


    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}

