using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class PartInfo {
	[SerializeField] Rigidbody _rigid;
	public Rigidbody rigid { get {return _rigid; } }

	[SerializeField] AttackController _attack;
	public AttackController attack { get { return _attack; } }

	[SerializeField] Define.BodyPart _type;
	public Define.BodyPart type { get { return _type; } }

	[System.NonSerialized] public Vector3 pos;
}

public class PlayerController : MonoBehaviour {

	Rigidbody _rootRigid;
	CapsuleCollider _rootCollider;
	Animator _animator;

	[SerializeField] Transform _head;

	[SerializeField] List<CommandData> _commands = new List<CommandData>();

	[SerializeField] PartInfo[] _partInfos;

	[SerializeField] Vector2 _playerOffset;

	List<InputHistory> _inputHistories = new List<InputHistory>();
	Define.Button _inputButton = 0;

	float _standHeight;

	float _comboDispCount;

	public Vector2 position { get { return _rootRigid.transform.position; } }

	PlayerController _rival;
	Transform _rivalTrans;
	public PlayerController rival { set { _rival = value; _rivalTrans = _rival.transform; } }

	Dictionary<Define.BodyPart, PartInfo> _partMap = new Dictionary<Define.BodyPart, PartInfo>();

	int _playerNo = 0;
	Define.Condition _condition = 0;
	CommandData _playingCommand = null;

	string _inputUD;
	string _inputLR;
	string _inputG;
	string _inputP;
	string _inputK;

	float _commandCount = 0;
	float _blowCount = 0;

	int _comboCount = 0;

	int _hp = 1000;
	[SerializeField] Image _hpGauge;
	[SerializeField] Text _comboText;

	public void Setup(int no) {
		_playerNo = no;
		_inputUD = string.Format("{0}P_UD", _playerNo);
		_inputLR = string.Format("{0}P_LR", _playerNo);
		_inputG = string.Format("{0}P_G", _playerNo);
		_inputP = string.Format("{0}P_P", _playerNo);
		_inputK = string.Format("{0}P_K", _playerNo);

		foreach(var partInfo in _partInfos) {
			partInfo.pos = partInfo.rigid.transform.localPosition;
			if(no > 1) {
				partInfo.rigid.gameObject.layer = LayerMask.NameToLayer("2PDamage");
				if(partInfo.attack != null) {
					partInfo.attack.gameObject.layer = LayerMask.NameToLayer("2PAttack");
				}
			}
			if(partInfo.attack != null) {
				partInfo.attack.gameObject.SetActive(false);
			}
			_partMap[partInfo.type] = partInfo;
		}
	}

	void Awake () {
		_rootRigid = GetComponent<Rigidbody>();
		_rootCollider = GetComponent<CapsuleCollider>();
		_standHeight = _rootCollider.height;
		_animator = GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		var rivalVec = _rivalTrans.transform.position - transform.position;
		var rotation = Quaternion.Euler( (rivalVec.x < 0 ? Vector3.down : Vector3.up) * 90);

		if((_condition & Define.Condition.Air) == 0) {
			transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 0.2f);
			if(rivalVec.x < 0)
				_condition |= Define.Condition.Reverce;
			else _condition &= ~Define.Condition.Reverce;
		}

		var position = transform.position;
		position.z = 0;

		if((_condition & Define.Condition.Look) != 0) {
			rotation = Quaternion.LookRotation(rivalVec, Vector3.up);
			rotation.eulerAngles += Vector3.up * (rivalVec.x > 0 ? 45f: -45f);
			_head.rotation = Quaternion.Lerp(_head.rotation, rotation, 0.25f);
		}

		if((_condition & Define.Condition.Damage) != 0) {
			_blowCount -= Time.deltaTime;
			if(_blowCount < 0) {
				_animator.SetBool("damage", false);
				_condition &= ~Define.Condition.Damage;
				_comboCount = 0;
			}
		}

		if(_comboDispCount > 0) {
			_comboDispCount -= Time.deltaTime;
			_comboText.enabled = _comboDispCount > 0;
		}

		if(_playingCommand != null) {
			if(_playingCommand.damagePoint != 0) {
				var attack = _partMap[_playingCommand.damagePoint].attack;
				if(attack.isHit) {
					_rival.HitAttack(_playingCommand, attack.transform.position + Vector3.back, _rootRigid.velocity);
					attack.gameObject.SetActive(false);
				}
			}

			var prevCount = _commandCount; 
			_commandCount += Time.deltaTime;

			_animator.SetBool("cancel", false);

			if(_playingCommand.cancelStartTime > 0) {
				if(prevCount < _playingCommand.cancelStartTime && _commandCount >= _playingCommand.cancelStartTime) {
					switch(_playingCommand.animKeyType) {
					case Define.AnimKeyType.Bool:
						_animator.SetBool(_playingCommand.animKey, false);
						break;
					case Define.AnimKeyType.Int:
						_animator.SetInteger(_playingCommand.animKey, 0);
						break;
					case Define.AnimKeyType.Float:
						_animator.SetFloat(_playingCommand.animKey, 0);
						break;
					}
				}
			}

			if((_condition & Define.Condition.Reverce) != 0)
				position.x -= _playingCommand.groundMove * Time.deltaTime;
			else position.x += _playingCommand.groundMove * Time.deltaTime;
			if((_condition & Define.Condition.Air) == 0) {
				if(_commandCount > _playingCommand.totalTime || !_playingCommand.IsKeep(_inputButton)) {
					FinishCommand();
				}
			}
		}
		if(Mathf.Abs(rivalVec.x) < _playerOffset.x && Mathf.Abs(rivalVec.y) < _playerOffset.y) {
			if(rivalVec.x > 0)
				position.x += -_playerOffset.x + rivalVec.x;
			else position.x += _playerOffset.x + rivalVec.x;
		}
		transform.position = position;

		if(Mathf.Abs(_rootRigid.velocity.x) < 1) {
			_condition &= ~Define.Condition.Dash;
		}

		Define.Button input = 0;
		float lr = Input.GetAxis(_inputLR);
		float ud = Input.GetAxis(_inputUD);
		bool g = Input.GetButton(_inputG);
		bool p = Input.GetButton(_inputP);
		bool k = Input.GetButton(_inputK);

		if(lr < -0.5f) {
			input |= Define.Button.L;
		}
		else if(lr > 0.5f) {
			input |= Define.Button.R;
		}

		if(ud < -0.5f) {
			input |= Define.Button.U;
		}
		else if(ud > 0.5f) {
			input |= Define.Button.D;
		}

		if(g) {
			input |= Define.Button.G;
		}

		if(p) {
			input |= Define.Button.P;
		}

		if(k) {
			input |= Define.Button.K;
		}

		if(input != _inputButton && input != 0) {
			_inputHistories.Insert(0, new InputHistory(){ time = Time.time, button = input & (input ^ _inputButton) });
		}
		for(int i = 0; i < _inputHistories.Count; ++i) {
			if(Time.time - _inputHistories[i].time >= 0.5f) {
				_inputHistories.RemoveRange(i, _inputHistories.Count - i);
				break;
			}
		}
		_inputButton = input;

		if((_playingCommand == null || _commandCount >= _playingCommand.cancelStartTime ) && (_condition & Define.Condition.Damage) == 0) {
			foreach(var command in _commands) {
				if(command.IsSuccess(_condition, _inputHistories.ToArray(), _inputButton)) {
					if(_playingCommand != command) {
						FinishCommand();

						switch(command.animKeyType) {
						case Define.AnimKeyType.Bool:
							_animator.SetBool(command.animKey, command.animKeyValue != 0);
							break;
						case Define.AnimKeyType.Int:
							_animator.SetInteger(command.animKey, Mathf.FloorToInt(command.animKeyValue));
							break;
						case Define.AnimKeyType.Float:
							_animator.SetFloat(command.animKey, command.animKeyValue);
							break;
						}
						_animator.SetBool("cancel", true);

						if(command.damagePoint != 0) {
							foreach(Define.BodyPart value in System.Enum.GetValues(typeof(Define.BodyPart))) {
								if((command.damagePoint & value) != 0) {
									_partMap[value].attack.gameObject.SetActive(true);
								}
							}
						}

						if(command.force.sqrMagnitude > 0) {
							var force = command.force;
							if((_condition & Define.Condition.Reverce) != 0)
								force.x = -force.x;
							_rootRigid.AddForce(force, ForceMode.VelocityChange);
						}
						if((command.conditionOn & Define.Condition.Air) != 0) {
//							_rootCollider.height = 0;
							_rootRigid.constraints &= ~RigidbodyConstraints.FreezeRotationX;
							_animator.SetFloat("jump", 1);
						}
						_condition |= command.conditionOn;
						_condition &= ~command.conditionOff;
						_playingCommand = command;
						_commandCount = 0;
					}
					break;
				}
			}
		}

		foreach(var part in _partMap.Values) {
			part.rigid.transform.localPosition = part.pos;
		}
	}

	void ResetKinetic() {
		foreach(var partInfo in _partInfos) {
			if(partInfo.type != Define.BodyPart.Head && partInfo.type != Define.BodyPart.Body)
				partInfo.rigid.isKinematic = false;
		}
	}

	void FinishCommand() {
		if(_playingCommand != null) {
			switch(_playingCommand.animKeyType) {
			case Define.AnimKeyType.Bool:
				_animator.SetBool(_playingCommand.animKey, false);
				break;
			case Define.AnimKeyType.Int:
				_animator.SetInteger(_playingCommand.animKey, 0);
				break;
			case Define.AnimKeyType.Float:
				_animator.SetFloat(_playingCommand.animKey, 0);
				break;
			}
			_condition &= ~Define.Condition.Crouch;
		}
		foreach(var partInfo in _partInfos) {
			if(partInfo.attack != null) {
				partInfo.attack.gameObject.SetActive(false);
			}
		}
		_condition |= Define.Condition.Look;
		_playingCommand = null;		
	}

	public void HitAttack(CommandData command, Vector3 pos, Vector2 velocity) {
		// todo 吹き飛ばし
		_hp -= Mathf.FloorToInt(command.damage * (1 + (velocity.sqrMagnitude) / 10000));
		var force = command.blowForce + velocity;
		if((_condition & Define.Condition.Reverce) != 0)
			force.x = -force.x;
		_rootRigid.AddForce(force / 2);
		_rival._rootRigid.AddForce(-force / 2);
		var sizeDelta = _hpGauge.rectTransform.sizeDelta;
		sizeDelta.x = 128 * _hp / 1000f;
		_hpGauge.rectTransform.sizeDelta = sizeDelta;

		_animator.SetBool("damage", true);
		GameManager.instance.CreateImpactEffect(pos);

		if((_condition & Define.Condition.Damage) != 0) {
			_comboCount++;
			if(_comboCount > 1) {
				_comboText.text = string.Format("{0} Hits!!", _comboCount);
				_comboText.enabled = true;
				_comboText.rectTransform.localScale = Vector3.one * 1.25f;
				_comboText.rectTransform.DOScale(Vector3.one, 0.1f);
				_comboDispCount = 2;
			}
		}
		else {
			_comboCount = 0;
			_condition |= Define.Condition.Damage;
		}
		_blowCount = command.blowTime;
	}

	void OnCollisionEnter(Collision collision) {
		foreach(var contact in collision.contacts) {
			if(contact.normal.y > 0.5f) {
				_rootCollider.height = _standHeight;
				_condition &= ~Define.Condition.Air;

				_rootRigid.constraints |= RigidbodyConstraints.FreezeRotationX;
				_animator.SetFloat("jump",0);
				break;
			}
		}
	}

	void OnGUI() {
		GUILayout.BeginArea(new Rect(Screen.width / 2 * (_playerNo - 1), 0, Screen.width / 2, Screen.height / 2));
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical();
		GUILayout.Label(":condition:");
		GUILayout.Label(_condition.ToString());

		GUILayout.Label(":current:");
		GUILayout.Label(_inputButton.ToString());

		GUILayout.Label(":history:");
		foreach(var history in _inputHistories) {
			GUILayout.Label(history.button.ToString());
		}

		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		if(_playingCommand != null) {
				GUILayout.Label(_playingCommand.name);
		}
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
	}
}