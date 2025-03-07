using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PowerUp_NeutralizePoison : PowerUp_Active_Instant {
	public override void Enable (int _power, float _amount, Color _effectColor) {
		base.Enable (_power, _amount, _effectColor);

		StartCoroutine (_Enable(power,amount));
	}

	IEnumerator _Enable (int power, float amount) {
		yield return new WaitForSeconds (0.4f);

		Disable ();

		yield break;

		List<IndividualCard> myCards = GetRandomizedSelectabeCardList ();
		power = Mathf.Clamp (power, 0, myCards.Count);

		for (int i = 0; i < power; i++) {
			if (myCards[i].isRevealed) {
				power++;
				if (power > myCards.Count) {
					break;
				} else {
					continue;
				}
			}
			print ("Revealing: " + myCards[i].x.ToString () + " - " + myCards[i].y.ToString ());
			Reveal (myCards[i], amount);
			yield return new WaitForSeconds (0.05f);
		}

		Disable ();
	}
}