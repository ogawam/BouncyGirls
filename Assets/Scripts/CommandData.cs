using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHistory {
	public float time;
	public Define.Button button;
}

[System.Serializable]
public class CommandData {

	[SerializeField] string _name;
	public string name { get { return _name; } }

	// 成立条件
	[SerializeField] Define.Condition _requiredCondition;

	[UnityEngine.Serialization.FormerlySerializedAs("_command")]
	[SerializeField] List<Define.Button> _buttons;
	[SerializeField] Define.Button _input;
	public bool IsSuccess(Define.Condition condition, InputHistory[] input, Define.Button current) {
		if(!IsKeep(current)) {
			return false;
		}

		if(_requiredCondition != (_requiredCondition & condition)) {
			return false;
		}
		
		if(_buttons.Count <= input.Length) {
			for(int i = 0; i < _buttons.Count; ++i) {
				var button = input[i].button;
				if((condition & Define.Condition.Reverce) != 0) {
					button &= ~(Define.Button.L | Define.Button.R);
					if((input[i].button & Define.Button.L) != 0)
						button |= Define.Button.R;
					if((input[i].button & Define.Button.R) != 0)
						button |= Define.Button.L;
				}
				if(_buttons[i] != button || input[i].time < 0) {
					return false;
				}
			}
			// 入力に使った場合は無効化する
			for(int i = 0; i < _buttons.Count; ++i) {
				input[i].time = -1;
			}
			return true;
		}
		return false;
	}

	public bool IsKeep(Define.Button current) {
		return _input == (_input & current);
	}

	// 性能
	[SerializeField] Define.Condition _condition;
	public Define.Condition condition { get { return _condition; } }

	[SerializeField] float _damageValue = 0;
	public float damage { get { return _damageValue; } }

	[SerializeField] Define.BodyPart _damagePoint;
	public Define.BodyPart damagePoint { get { return _damagePoint; } }

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
