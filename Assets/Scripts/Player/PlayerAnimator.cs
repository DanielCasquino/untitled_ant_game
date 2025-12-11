using System.Collections;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] PlayerCursor cursor;
    Animator animator;
    [SerializeField] float rotationSpeed = 10f;
    SpriteRenderer spriteRenderer;
    Coroutine flashRoutine;


    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward, cursor.transform.localPosition.normalized), rotationSpeed * Time.deltaTime);

        if (Player.instance.isInvis)
        {
            if (flashRoutine == null)
                flashRoutine = StartCoroutine(InvisFlash());
        }
        else
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
                spriteRenderer.enabled = true; // Ensure sprite is visible when not invisible
            }
        }
    }

    public void OnStateChanged(Player.AnimationState state)
    {
        switch (state)
        {
            case Player.AnimationState.IDLE:
                animator.SetBool("moving", false);
                break;
            case Player.AnimationState.MOVING:
                animator.SetBool("moving", true);
                break;
        }
    }

    IEnumerator InvisFlash()
    {
        while (Player.instance.isInvis)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.1f); // Flash interval
        }
        spriteRenderer.enabled = true;
    }
}
