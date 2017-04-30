using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {

	void Awake() {
		_instance = this;
	}

	[SerializeField] PlayerController[] _players;
	[SerializeField] float _distToScrn;
	[SerializeField] float _scrnMin;
	[SerializeField] float _heightOffset;
	[SerializeField] float _heightRefer;

	[SerializeField] EffectController _impactEffect;

	void Start () {
		Application.targetFrameRate = 60;
		_players[0].rival = _players[1];
		_players[0].Setup(1);
		_players[1].rival = _players[0];
		_players[1].Setup(2);
	}
	
	void Update () {
		var pos = Camera.main.transform.position;
		var z = pos.z;
		var v = _players[0].position - _players[1].position;
		var center = (_players[0].position + _players[1].position) * 0.5f;
		var size = _scrnMin + v.sqrMagnitude * _distToScrn;
		center.y += (Mathf.Max(0, _heightRefer - Mathf.Abs(v.y)) / _heightRefer) * _heightOffset * size;
		Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, 0.2f);
		pos = Vector2.Lerp(pos, center, 0.2f);
		pos.z = z;
		Camera.main.transform.position = pos;
	}

	public void CreateImpactEffect(Vector3 pos) {
		Instantiate(_impactEffect, pos, Camera.main.transform.rotation);
	}
}
