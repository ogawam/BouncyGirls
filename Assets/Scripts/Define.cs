using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Define {

	public enum Button {
		L = (1<<0),
		R = (1<<1),
		U = (1<<2),
		D = (1<<3),
		G = (1<<4),
		P = (1<<5),
		K = (1<<6)
	}

	public enum Condition {
		Down	= (1<<0),	// やられ
		Air		= (1<<1),	// 空中
		Crouch	= (1<<2),	// しゃがみ
		Full	= (1<<3),	// フルパワー
		Guard	= (1<<4),	// ガード
		Throw	= (1<<5),	// 投げ
		Ground	= (1<<6),	// 地上
	}

	public enum DamagePoint {
		Head		= (1<<0),
		Body		= (1<<1),
		LShoulder	= (1<<2),
		RShoulder	= (1<<3),
		LArm		= (1<<4),
		RArm		= (1<<5),
		LHand		= (1<<6),
		RHand		= (1<<7),
		LKnee		= (1<<8),
		RKnee		= (1<<9),
		LFoot		= (1<<10),
		RFoot		= (1<<11),
	}

	public enum AnimKeyType {
		Int,
		Float,
		Bool
	}
}
