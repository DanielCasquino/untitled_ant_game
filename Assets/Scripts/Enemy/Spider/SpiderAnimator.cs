using UnityEngine;

public class SpiderAnimator : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnStateChanged(Spider.AnimationState state)
    {
        switch (state)
        {
            case Spider.AnimationState.IDLE:
                animator.SetBool("moving", false);
                break;
            case Spider.AnimationState.MOVING:
                animator.SetBool("moving", true);
                break;
        }
    }
}
