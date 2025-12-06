using UnityEngine;

public class SpiderAnimator : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnStateChanged(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.IDLE:
                animator.SetBool("moving", false);
                break;
            case AnimationState.MOVING:
                animator.SetBool("moving", true);
                break;
        }
    }
}
