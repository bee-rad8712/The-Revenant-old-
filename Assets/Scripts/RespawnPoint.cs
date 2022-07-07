using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private BoxCollider2D respawn;
    public GameObject respawnPoint;

    private void Awake()
    {
        respawn = respawnPoint.GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        
    }
}
