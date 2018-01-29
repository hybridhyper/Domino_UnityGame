/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;

public class ControladorMaestro : MonoBehaviour{
    public virtual string DeleteSpecialChars(string imageName){
        return "";
    }
    public virtual void GameStarts() { }
    public virtual void ReadjustBoard(int myPosition, string orientationName) { }
    public virtual void CheckDragDrop(Vector3 recolocationPositionValue, LastChipObject lastChipSetObject) { }
    public virtual Transform GetMiniDrop(string _miniDropName) { return null; }
    public virtual void MoreTime() { }
    public virtual void SkipQuestion() { }
    public virtual void PowerUpSetFirstChip() { }
    public virtual void PowerUpDeleteChip() { }
    public virtual void NewQuestion() { }
    public virtual void CleanBoardButton() { }
    public virtual void CleanAllBoard() { }
    public virtual void ShowCoins() { }
    public virtual void ResetGame() { }
    public virtual void ResumeGame() { }
    public virtual void StartAnimation(){}
    public virtual void StopAnimationHand(){}
    public virtual void StartAnimationHand(){}
    public virtual void CheckEnabledChip(Transform chip){}
    public virtual bool GetStartedGame() {
        return false;
    }
}