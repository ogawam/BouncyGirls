using System;
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
	[UnityEngine.Serialization.FormerlySerializedAs("_requiredCondition")]
	[EnumFlags][SerializeField] Define.Condition _requiredConditionOn;
	[EnumFlags][SerializeField] Define.Condition _requiredConditionOff;

	[UnityEngine.Serialization.FormerlySerializedAs("_command")]
	[EnumFlags][SerializeField] List<Define.Button> _buttons;
	[EnumFlags][SerializeField] Define.Button _input;

	Define.Button ConvertButton(Define.Button src, Define.Condition condition) {
		var dst = src;
		if((condition & Define.Condition.Reverce) != 0) {
			dst &= ~(Define.Button.L | Define.Button.R);
			if((src & Define.Button.L) != 0) {
				dst |= Define.Button.R;
			}
			if((src & Define.Button.R) != 0) {
				dst |= Define.Button.L;
			}
		}
		return dst;
	}

	public bool IsSuccess(Define.Condition condition, InputHistory[] input, Define.Button button) {
		if(!IsKeep(ConvertButton(button, condition))) {
			return false;
		}

		if(_requiredConditionOn != (_requiredConditionOn & condition) ||
			_requiredConditionOff != 0 && (_requiredConditionOff & condition) != 0) 
		{
			return false;
		}
		
		if(_buttons.Count <= input.Length) {
			for(int i = 0; i < _buttons.Count; ++i) {
				if(_buttons[i] != ConvertButton(input[i].button, condition) || input[i].time < 0) {
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
	[UnityEngine.Serialization.FormerlySerializedAs("_condition")]
	[EnumFlags][SerializeField] Define.Condition _conditionOn;
	public Define.Condition conditionOn { get { return _conditionOn; } }

	[EnumFlags][SerializeField] Define.Condition _conditionOff;
	public Define.Condition conditionOff { get { return _conditionOff; } }

	[SerializeField] float _damageValue = 0;
	public float damage { get { return _damageValue; } }

	[EnumFlags][SerializeField] Define.BodyPart _damagePoint;
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

	[SerializeField] Vector2 _blowForce;
	public Vector2 blowForce { get { return _blowForce; } }

	[SerializeField] float _blowTime;
	public float blowTime { get { return _blowTime; } }

	// アニメーション
	[SerializeField] string _animKey;
	public string animKey { get { return _animKey; } }

	[SerializeField] Define.AnimKeyType _animKeyType;
	public Define.AnimKeyType animKeyType { get { return _animKeyType; } }

	[SerializeField] float _animKeyValue;
	public float animKeyValue { get { return _animKeyValue; } }
}
