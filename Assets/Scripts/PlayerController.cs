using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	Rigidbody _rootRigid;
	CapsuleCollider _rootCollider;
	Animator _animator;

	[SerializeField] Transform _head;

	[SerializeField] KeyCode _up;
	[SerializeField] KeyCode _down;
	[SerializeField] KeyCode _left;
	[SerializeField] KeyCode _right;

	[SerializeField] float _forceV;
	[SerializeField] float _forceH;

	[SerializeField] List<CommandData> _commands = new List<CommandData>();

	List<Define.Button> _inputButtonLog = new List<Define.Button>();
	Define.Button _inputButton = 0;

	float _standHeight;

	public Vector2 position { get { return _rootRigid.transform.position; } }

	Transform _rivalTrans;
	public Transform rivalTrans { set { _rivalTrans = value; } }

	float _jumpWait = 0;
	float _landWait = 0;

	int _playerNo = 0;
	Define.Condition _condition = 0;
	CommandData _playingCommand = null;

	string _inputUD;
	string _inputLR;
	string _inputG;
	string _inputP;
	string _inputK;

	public void Setup(int no) {
		_playerNo = no;
		_inputUD = string.Format("{0}P_UD", _playerNo);
		_inputLR = string.Format("{0}P_LR", _playerNo);
		_inputG = string.Format("{0}P_G", _playerNo);
		_inputP = string.Format("{0}P_P", _playerNo);
		_inputK = string.Format("{0}P_K", _playerNo);
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

		var position = transform.position;
		position.z = 0;

		var constrains = _rootRigid.constraints;
		if(_jumpWait <= 0 && _landWait <= 0) {
			if(Physics.Raycast(position, Vector3.down, 1f, 1 << LayerMask.NameToLayer("Terrain"))) {
				_rootCollider.height = _standHeight;
				constrains |= RigidbodyConstraints.FreezeRotationX;
				if(_animator.GetFloat("jump") > 0) {
					_animator.SetFloat("jump",0);
					Debug.Log("land");
				}
			}
		}
		else {
			_rootCollider.height = 0;
			constrains &= ~RigidbodyConstraints.FreezeRotationX;
		}
		_rootRigid.constraints = constrains;

		_landWait -= Time.deltaTime;

		rotation = Quaternion.LookRotation(rivalVec, Vector3.up);
		rotation.eulerAngles += Vector3.up * (rivalVec.x > 0 ? 45f: -45f);
		_head.rotation = Quaternion.Lerp(_head.rotation, rotation, 0.25f);
/*
		if(_jumpWait > 0) {
			_jumpWait -= Time.deltaTime;
			if(_jumpWait <= 0) {
				_rootRigid.AddForce(Vector3.up * _forceV);
				_landWait = 0.5f;
				Debug.Log("jump");
			}
		}
		else if(_animator.GetFloat("jump") == 0) { 
			if(Input.GetKeyDown(_up)) {
				_jumpWait = 4 / 60f;
				_animator.SetFloat("jump",0.5f);	
			}
			if(Input.GetKey(_down)) {
				_animator.SetBool("crouch",true);
			}
			else {
				_animator.SetBool("crouch",false);
			}
			if(Input.GetKey(_left)) {
				_animator.SetFloat("move",-2f);
				position += Vector3.left * _forceH * Time.deltaTime;
			}
			else if(Input.GetKey(_right)) {
				_animator.SetFloat("move",2f);
				position += Vector3.right * _forceH * Time.deltaTime;
			}
			else {
				_animator.SetFloat("move",0);
			}
		}
		if(Input.GetKeyDown(_down)) {
			_rootRigid.AddForce(Vector3.down * _forceV);
		}

		if(Input.GetKeyDown(KeyCode.Space)) {
			_rootRigid.rotation = Quaternion.identity;
		}
*/
		if(_playingCommand != null) {
			position.x += _playingCommand.groundMove * Time.deltaTime;
			if(_playingCommand.totalTime == 0) {
				if(_inputButton == 0) {
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
					_playingCommand = null;
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
			_inputButtonLog.Insert(0, input & (input ^ _inputButton));
		}
		if(_inputButtonLog.Count > 16) {
			_inputButtonLog.RemoveRange(16, _inputButtonLog.Count - 16);
		}
		_inputButton = input;

		foreach(var command in _commands) {
			if(command.IsSuccess(_condition, _inputButtonLog.ToArray())) {
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
				_playingCommand = command;
			}
		}
	}

	void OnGUI() {
		GUILayout.BeginHorizontal();
		if(_playerNo > 1) {
			GUILayout.Space(Screen.width / 2);
		}
		GUILayout.BeginVertical();
		foreach(var button in _inputButtonLog) {
			GUILayout.Label(button.ToString());
		}
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
}