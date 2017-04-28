using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	static protected T _instance = null;
	static public T instance { 
		get { 
			if (_instance == null)
				_instance = FindObjectOfType <T> ();
			return _instance; 
		}
	}
}