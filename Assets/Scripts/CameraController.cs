using UnityEngine;
public class CameraController : MonoBehaviour
{
	[SerializeField] CharacterManager cm;
	[SerializeField] Transform player;
	void Update()
	{
		if (cm.isGhost)
		{
			transform.position = new Vector3(cm.ghost.transform.position.x, cm.ghost.transform.position.y + 1, transform.position.z);
		}
		else
		{
			transform.position = new Vector3(cm.avatar.transform.position.x, cm.avatar.transform.position.y + 2, transform.position.z);
		}
	}
}
