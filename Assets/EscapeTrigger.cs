using UnityEngine;

public class EscapeTrigger : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            GameManager.instance.PlayerEscaped();
        }
    }
}
