using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour {
	public static NPCManager s;

	public Transform npcSpawnPos;

	public List<NPCBase> ActiveNPCS = new List<NPCBase> ();

	//List<NPCBase> spawnableNPCs = new List<NPCBase> ();

	// Start is called before the first frame update
	void Awake () {
		s = this;

		KillAllNPCs ();
	}

	public void SpawnNPCPeriodic () {
		if (DataHandler.s.myPlayerInteger != 0)
			return;

		SpawnNPC (GS.a.myNPCPrefab);

		GS.a.npcSpawnCount--;
		if (GS.a.npcSpawnCount != 0 && !GameObjectiveMaster.s.isFinished)
			Invoke ("SpawnNPCPeriodic", GS.a.npcSpawnDelay * Random.Range (0.9f, 1.1f));
	}

	public void SpawnNPC (NPCBase myNPCPrefab) {
		int y = Random.Range (0, CardHandler.s.allCards.GetLength (1));
		int x = 0;
		IndividualCard myCard = CardHandler.s.allCards[x, y];
		int trials = 0;
		while (myCard.myOccupants.Count > (Mathf.FloorToInt (trials / CardHandler.s.allCards.Length) + 1)) {
			x++;
			if (x > CardHandler.s.allCards.GetLength (0)) {
				x = 0;
				y++;
				if (y > CardHandler.s.allCards.GetLength (1)) {
					y = 0;
				}
			}
			myCard = CardHandler.s.allCards[x, y];
			trials++;
		}

		NPCBase myNPC = SpawnNPCAtLocation (myCard, myNPCPrefab);

	}

	NPCBase SpawnNPCAtLocation (IndividualCard myCard, NPCBase myNPCPrefab) {
		if (myNPCPrefab == null) {
			DataLogger.LogError ("Trying to spawn null npc");
			return null;
		}

		NPCBase myNPC = Instantiate (myNPCPrefab.gameObject, npcSpawnPos.position, Quaternion.identity).GetComponent<NPCBase> ();
		myNPC.transform.localScale = new Vector3 (1, 1, 1) * GS.a.gridSettings.scaleMultiplier;
		myNPC.Spawn (myCard);

		if (DataHandler.s.myPlayerInteger == 0) {
			ActiveNPCS.Add (myNPC);
			SendNPCAction (myCard.x, myCard.y, myNPC.GetComponent<NPCBase> (), ActionType.Spawn, -1);
		}

		return myNPC;
	}

	public void KillAllNPCs () {
		for (int i = ActiveNPCS.Count-1; i>= 0; i--) {
			if (ActiveNPCS[i] != null)
				ActiveNPCS[i].Die (true);
		}
		ActiveNPCS.Clear ();
	}

	public void StopAllNPCs () {
		foreach (NPCBase npc in ActiveNPCS) {
			npc.StopAllCoroutines ();
			npc.CancelInvoke ();
		}
	}


	//-----------------------------------------------------------------------------------------------Networking

	public enum ActionType { Spawn, GoToPos, SelectCard, Activate, Die }

	public void ReceiveNPCAction (int x, int y, int index, ActionType action, int data) {
		IndividualCard card = null;
		try {
			if (x != -1 && y != -1)
				card = CardHandler.s.allCards[x, y];
		} catch {
			DataLogger.LogError ("ReceiveNPCAction couldnt find the card " + x.ToString () + "-" + y.ToString ());
		}

		try {
			NPCBase myNPC = null;
			if (action != ActionType.Spawn)
				myNPC = ActiveNPCS[index];

			switch (action) {
			case ActionType.Spawn:
				DataLogger.LogError ("Customs NPCs are not implemented to spawn yet! Spawning default level npc");
				myNPC = SpawnNPCAtLocation (card, GS.a.myNPCPrefab);
				ActiveNPCS.SetIndex (index, myNPC);
				break;
			case ActionType.Die:
			case ActionType.SelectCard:
			case ActionType.Activate:
			case ActionType.GoToPos:
				myNPC.ReceiveNPCAction (card, action, data);
				break;
			default:
				DataLogger.LogError ("Unrecognized power up action PUF");
				break;
			}
		} catch (System.Exception e) {
			DataLogger.LogError (this.name, e);
		}
	}


	//there exists only unselect, so unselect this card if we were selecting it wrongly
	public void ReceiveNetworkCorrection (IndividualCard myCard) {
		foreach (NPCBase npc in ActiveNPCS) {
			npc.ReceiveNetworkCorrection (myCard);
		}
	}

	public void SendNPCAction (int x, int y, NPCBase source, ActionType action, int data) {
		int myIndex = ActiveNPCS.IndexOf (source);
		if (myIndex != -1)
			DataHandler.s.SendNPCAction (x, y, myIndex, action, data);
		else
			DataLogger.LogError ("Trying to sent action for unregistered NPC: " + source.gameObject.name);
	}
}
