using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] PlayerCursor cursor;
    Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void FixedUpdate()
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward, cursor.transform.localPosition.normalized);
    }

    public void OnStateChanged(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                animator.SetBool("moving", false);
                break;
            case PlayerState.MOVING:
                animator.SetBool("moving", true);
                break;
        }
    }
}
