using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EffectController : MonoBehaviour {

	[SerializeField] Renderer _target;
	[SerializeField] Vector2 _uvsSpeed;
	[SerializeField] float _uvsTime;
	[SerializeField] float _fadeTime;
	[SerializeField] bool _randomAngle;

	// Use this for initialization
	IEnumerator Start () {
		if(_randomAngle) {
			_target.transform.localEulerAngles += Vector3.left * Random.value * 360;
		}
		_target.material.DOOffset(_uvsSpeed, "_MainTex", _uvsTime).SetEase(Ease.Linear);
		if(_fadeTime > 0) {
			float sec = 0;
			Color col = _target.material.GetColor("_TintColor");
			float alpha = col.a;
			while(sec < _fadeTime) {
				var rate = Mathf.Min(1, sec / _fadeTime);
				col.a = alpha * (1 - rate);
				_target.material.SetColor("_TintColor", col);
				sec += Time.deltaTime;
				yield return 0;
			}
			Destroy(gameObject);
		}
	}

	void Update() {
		transform.rotation = Camera.main.transform.rotation;
	}
}
