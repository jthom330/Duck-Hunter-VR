using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPump : StateMachineBehaviour {

    public GameObject shellPrefab;
    private bool isShellEjected = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //play shotgun cock sound
        AudioSource audioSource = animator.GetComponentInChildren<AudioSource>();
        audioSource.Play();

        //reset var for this round of the animation 
        isShellEjected = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //eject a shot gun shell in the middle of the pump animation
        if (stateInfo.normalizedTime >= 0.5f && !isShellEjected)
        {
            //create a shotgun shell at a specified point in space with te correct orientation
            Transform ejectPoint = animator.transform.GetChild(3);
            GameObject prefab = Instantiate(shellPrefab, new Vector3(ejectPoint.position.x, ejectPoint.position.y, ejectPoint.position.z), Quaternion.Euler(ejectPoint.rotation.x-90f, ejectPoint.rotation.y, ejectPoint.rotation.z));

            //add force to the created shell 
            prefab.GetComponent<Rigidbody>().AddForce((ejectPoint.right + ejectPoint.up - (ejectPoint.forward * .5f)) * 100);
            
            //prevents another shell from being ejected during this animation
            isShellEjected = true;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //allows gun to fire again
        Globals.canFireGun = true;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
