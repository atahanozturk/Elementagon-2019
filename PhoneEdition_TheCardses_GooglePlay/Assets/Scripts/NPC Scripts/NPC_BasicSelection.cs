using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_BasicSelection : NPCBase {

	public float cardSelectTime = 3f;

	public override IEnumerator MainLoop () {
		yield return new WaitForSeconds (0.5f);
		IndividualCard curTarget = null;
		List<IndividualCard> myCards;

		while (true){

			if (SelectedCardCount < 1) {
				do {
					myCards = GetRandomizedMoveableCardList ();
					yield return new WaitForSeconds (0.5f);
				} while (myCards.Count == 0);

				curTarget = GetRandomizedMoveableCardList ()[0];
				yield return MoveToPosition (curTarget);

				if (!Select (curTarget))
					Denied ();

				yield return new WaitForSeconds (selectWaitTime);

				if (curTarget == null)
					continue;
				if (curTarget.cBase == null)
					continue;
			}

			if (SelectedCardCount < 1)
				continue;

			bool targetSet = false;
			if (ShouldIDecideCorrectly ()) {
				do {
					myCards = GetRandomizedMoveableCardList ();
					yield return new WaitForSeconds (0.5f);
				} while (myCards.Count == 0);
				for (int i = 0; i < myCards.Count; i++) {
					if (curTarget == null)
						break;
					if (curTarget.cBase == null)
						break;
					if (myCards[i].cBase != null) {
						if (curTarget.cBase.dynamicCardID == myCards[i].cBase.dynamicCardID) {
							curTarget = myCards[i];
							targetSet = true;
							break;
						}
					}
				}
			} 
			if (!targetSet) {
				do {
					myCards = GetRandomizedMoveableCardList ();
					yield return new WaitForSeconds (0.5f);
				} while (myCards.Count == 0);
				curTarget = myCards[0];
			}

			yield return MoveToPosition (curTarget);

			if (!Select (curTarget))
				Denied ();

			if (curTarget == null)
				continue;
			if (curTarget.cBase == null)
				continue;

			CheckCards ();

			yield return new WaitForSeconds (cardSelectTime * RandomTimeMultiplier());
		}
	}
}
