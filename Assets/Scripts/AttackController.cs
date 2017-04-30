using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour {

	bool _isHit = false;
	public bool isHit { get { 
		bool result = _isHit;
		_isHit = false;
		return result; 
	} }

	void OnTriggerEnter(Collider collider) {
		if(gameObject.activeSelf) {
			_isHit = ((1 << collider.gameObject.layer) & LayerMask.GetMask("1PDamage", "2PDamage")) != 0;
		}
	}
}
