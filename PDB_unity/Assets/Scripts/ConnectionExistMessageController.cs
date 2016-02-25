using UnityEngine;
using System.Collections;

public class ConnectionExistMessageController : MonoBehaviour {

	public void ChangeStatus()
	{
		GetComponent<Animator> ().SetBool ("Play", false);
	}
}
