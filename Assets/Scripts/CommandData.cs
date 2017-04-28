using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CommandData {

	// 成立条件
	[SerializeField] Define.Condition _requiredCondition;

	[SerializeField] List<Define.Button> _command;
	public bool IsSuccess(Define.Condition condition, Define.Button[] input) {
		if(_condition != (_condition & condition)) {
			return false;
		}
		
		if(_command.Count < input.Length) {
			for(int i = 0; i < _command.Count; ++i) {
				if(_command[i] != input[i]) {
					return false;
				}
			}
			return true;
		}
		return false;
	}

	// 性能
	[SerializeField] Define.Condition _condition;
	public Define.Condition condition { get { return _condition; } }

	[SerializeField] float _damageValue = 0;
	public float damage { get { return _damageValue; } }

	[SerializeField] Define.DamagePoint _damagePoint;
	public Define.DamagePoint damagePoint { get { return _damagePoint; } }

	[SerializeField] float _totalTime = 0;
	public float totalTime { get { return _totalTime; } }

	[SerializeField] float _cancelStartTime = 0;
	public float cancelStartTime { get { return _cancelStartTime; } }

	[SerializeField] float _cancelEndTime = 0;
	public float cancelEndTime { get { return _cancelEndTime; } }

	[SerializeField] Vector2 _force;
	public Vector2 force { get { return _force; } }

	[SerializeField] float _groundMove = 0;
	public float groundMove { get { return _groundMove; } }

	// アニメーション
	[SerializeField] string _animKey;
	public string animKey { get { return _animKey; } }

	[SerializeField] Define.AnimKeyType _animKeyType;
	public Define.AnimKeyType animKeyType { get { return _animKeyType; } }

	[SerializeField] float _animKeyValue;
	public float animKeyValue { get { return _animKeyValue; } }
}
