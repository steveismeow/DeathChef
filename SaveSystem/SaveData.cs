using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int runNumber;

    public bool
        hasDied,
        madeItToInterWaveScene,
        beatWave30,
        interWaveAfter30,
        madeItToWaveBlock2,
        metTheDevil,
        devilStartDialogue1,
        beatWave51,
        beatTheGame;
}

//public interface ISaveable
//{
//    void PopulateSaveData(SaveData a_SaveData);
//    void LoadFromSaveData(SaveData a_SaveData);
//}
