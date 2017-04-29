using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	class PartInfo {
		public Rigidbody body;
		public Collider attack;
		public Vector3 pos;
	}

	Rigidbody _rootRigid;
	CapsuleCollider _rootCollider;
	Animator _animator;

	[SerializeField] Transform _head;

	[SerializeField] Rigidbody _rigidHead;
	[SerializeField] Rigidbody _rigidBody;
	[SerializeField] Rigidbody _rigidLShoulder;
	[SerializeField] Rigidbody _rigidRShoulder;
	[SerializeField] Rigidbody _rigidLArm;
	[SerializeField] Rigidbody _rigidRArm;
	[SerializeField] Rigidbody _rigidLHand;
	[SerializeField] Rigidbody _rigidRHand;
	[SerializeField] Rigidbody _rigidLKnee;
	[SerializeField] Rigidbody _rigidRKnee;
	[SerializeField] Rigidbody _rigidLFoot;
	[SerializeField] Rigidbody _rigidRFoot;

	[SerializeField] List<CommandData> _commands = new List<CommandData>();

	List<InputHistory> _inputHistories = new List<InputHistory>();
	Define.Button _inputButton = 0;

	float _standHeight;

	public Vector2 position { get { return _rootRigid.transform.position; } }

	Transform _rivalTrans;
	public Transform rivalTrans { set { _rivalTrans = value; } }

	Dictionary<Define.BodyPart, PartInfo> _bodyParts = new Dictionary<Define.BodyPart, PartInfo>();

	int _playerNo = 0;
	Define.Condition _condition = 0;
	CommandData _playingCommand = null;

	string _inputUD;
	string _inputLR;
	string _inputG;
	string _inputP;
	string _inputK;

	float _commandCount = 0;

	void RegisterBodyPart(Define.BodyPart part, Rigidbody rigidbody) {
		_bodyParts[part] = new PartInfo(){
			body = rigidbody,
			pos = rigidbody.transform.localPosition
		};
	}

	public void Setup(int no) {
		_playerNo = no;
		_inputUD = string.Format("{0}P_UD", _playerNo);
		_inputLR = string.Format("{0}P_LR", _playerNo);
		_inputG = string.Format("{0}P_G", _playerNo);
		_inputP = string.Format("{0}P_P", _playerNo);
		_inputK = string.Format("{0}P_K", _playerNo);

		RegisterBodyPart(Define.BodyPart.Body, _rigidBody);
		RegisterBodyPart(Define.BodyPart.Head, _rigidHead);
		RegisterBodyPart(Define.BodyPart.LShoulder, _rigidLShoulder);
		RegisterBodyPart(Define.BodyPart.LArm, _rigidLArm);
		RegisterBodyPart(Define.BodyPart.LHand, _rigidLHand);
		RegisterBodyPart(Define.BodyPart.RShoulder, _rigidRShoulder);
		RegisterBodyPart(Define.BodyPart.RArm, _rigidRArm);
		RegisterBodyPart(Define.BodyPart.RHand, _rigidRHand);
		RegisterBodyPart(Define.BodyPart.LKnee, _rigidLKnee);
		RegisterBodyPart(Define.BodyPart.LFoot, _rigidLFoot);
		RegisterBodyPart(Define.BodyPart.RKnee, _rigidRKnee);
		RegisterBodyPart(Define.BodyPart.RFoot, _rigidRFoot);
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
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 0.2f);

		if(rivalVec.x < 0)
			_condition |= Define.Condition.Reverce;
		else _condition &= ~Define.Condition.Reverce;

		var position = transform.position;
		position.z = 0;

		rotation = Quaternion.LookRotation(rivalVec, Vector3.up);
		rotation.eulerAngles += Vector3.up * (rivalVec.x > 0 ? 45f: -45f);
		_head.rotation = Quaternion.Lerp(_head.rotation, rotation, 0.01f);

		if(_playingCommand != null) {
			_commandCount += Time.deltaTime;
			position.x += _playingCommand.groundMove * Time.deltaTime;
			if((_condition & Define.Condition.Ground) != 0) {
				if(_commandCount > _playingCommand.totalTime || !_playingCommand.IsKeep(_inputButton)) {
					FinishCommand();
				}
			}
		}
		transform.position = position;

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
					if(command.force.sqrMagnitude > 0) {
						var force = command.force;
						if((_condition & Define.Condition.Reverce) != 0)
							force.x = -force.x;
						_rootRigid.AddForce(force, ForceMode.VelocityChange);
					}
					if((command.condition & Define.Condition.Air) != 0) {
						_condition |= Define.Condition.Air;
						_condition &= ~Define.Condition.Ground;

						_rootCollider.height = 0;
						_rootRigid.constraints &= ~RigidbodyConstraints.FreezeRotationX;
					}
					_condition |= command.condition;
					_playingCommand = command;
					_commandCount = 0;
				}
				break;
			}
		}

		foreach(var part in _bodyParts.Values) {
			part.body.transform.localPosition = part.pos;
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
		_playingCommand = null;		
	}

	void OnCollisionEnter(Collision collision) {
		foreach(var contact in collision.contacts) {
			if(contact.normal.y > 0.5f) {
				_rootCollider.height = _standHeight;
				_condition |= Define.Condition.Ground;
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