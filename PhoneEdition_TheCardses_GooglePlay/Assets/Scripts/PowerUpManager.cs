﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour {

	public static PowerUpManager s;

	[HideInInspector]
	public bool canActivatePowerUp = true;

	public enum PUpTypes { equipment, potion};

	public PowerUpBase[] equipmentPUps;
	public PowerUpBase[] poitonPUps;

	PowerUpBase activePUp;

	//for posion handling
	//public PowerUp_Poison pPoison;


	[Tooltip ("//--------CARD TYPES---------\n// 0 = any type\n// 1-7 = normal cards\n// 8-14 = dragons\n//---------------------------\n// 1 = Earth\n// 2 = Fire\n// 3 = Ice\n// 4 = Light\n// 5 = Nether\n// 6 = Poison\n// 7 = Shadow\n//---------------------------\n// 8 = Earth Dragon\n// 9 = Fire Dragon\n//10 = Ice Dragon\n//11 = Light Dragon\n//12 = Nether Dragon\n//13 = Poison Dragon\n//14 = Shadow Dragon\n//---------------------------")]
	public Color[] genericColors = new Color[16];
	public GameObject genericIndicatorPrefab;
	public GameObject indicatorScoreboardPrefab;

	public Transform throwToCardStartPos;

	// Use this for initialization
	void Awake () {	   
		s = this;	
	}


	public void EnablePowerUp (PUpTypes type, int id, int elementalType, int power, float amount) {
		if (LocalPlayerController.s.canSelect == false)
			return;
		if (canActivatePowerUp == false)
			return;

		if (type == PUpTypes.equipment) {
			equipmentPUps[id].Enable (elementalType, power, amount);
			activePUp = equipmentPUps[id];
		} else {
			poitonPUps[id].Enable (elementalType, power, amount);
			activePUp = poitonPUps[id];
		}
	}

	public void PowerUpDisabledCallback () {
		CharacterStuffController.s.PowerUpDisabledCallback ();
	}

	public void DisablePowerUps () {
		if(activePUp != null)
		activePUp.Disable ();
	}

	public delegate void Hook (IndividualCard card);
	public Hook activateHook;
	public void ActivateInvoke (IndividualCard card) {
		try{
			if(activateHook != null)
				activateHook.Invoke (card);
		} catch (System.Exception e) {
			DataLogger.LogError (this.name, e);
		}
	}

	public Hook selectHook;
	public void SelectInvoke (IndividualCard card) {
		try{
			if (selectHook != null)
				selectHook.Invoke (card);
		} catch (System.Exception e) {
			DataLogger.LogError (this.name, e);
		}
	}

	public delegate void Hook2 ();
	public Hook2 checkHook;
	public void CheckInvoke () {
		try{
			if (checkHook != null)
				checkHook.Invoke ();
		} catch (System.Exception e) {
			DataLogger.LogError (this.name, e);
		}
	}



	public enum ActionType {Enable, Activate, SelectCard, Disable}

	public void ReceiveEnemyPowerUpActions (int player, int x, int y, PUpTypes type, int id, int power, float amount, ActionType action) {
		IndividualCard card = null;
		try {
			if (x != -1 && y != -1)
				card = CardHandler.s.allCards[x, y];
		} catch {
			DataLogger.LogError ("ReceiveEnemyPowerUpActions couldnt find the card " + x.ToString() + "-" + y.ToString());
		}

		if (type == PUpTypes.equipment) {
			equipmentPUps[id].ReceiveAction (player, card,power,amount, action);
		} else {
			poitonPUps[id].ReceiveAction (player, card, power, amount, action);
		}
	}

	//there exists only unselect, so unselect this card if we were selecting it wrongly
	public void ReceiveNetworkCorrection (IndividualCard myCard){
		activePUp.ReceiveNetworkCorrection (myCard);
	}

	public void SendPowerUpAction (int x, int y, PUpTypes pUpType, int id, int power, float amount, ActionType action) {
		DataHandler.s.SendPowerUpAction (x, y, pUpType, id, power, amount, action);
	}

	/// <summary>
	/// This is used when locally selecting a posion card. Network posioning goes through the poison power up script, not here.
	/// </summary>
	/// <param name="myPlayerinteger"></param>
	/// <param name="myCard"></param>
	/// <param name="message"></param>
	public void ChoosePoisonCard (int myPlayerinteger, IndividualCard myCard, string message){
		   
		if (myCard.isPoison) {
			//pPoison.ChoosePoisonCard (myPlayerinteger, myCard, message);
			myCard.isPoison = false;
		}
	}
}