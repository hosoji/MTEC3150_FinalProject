using UnityEngine;

public class OverheadCamera : MonoBehaviour
{

    public float yOffset = 4.5f;

    public Transform player;


    void Update()
    {
        transform.position = new Vector3(player.position.x, player.position.y + yOffset, player.position.z);
        
    }
}
