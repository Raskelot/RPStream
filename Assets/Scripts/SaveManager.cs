///Not working for this project

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    #region Instance
    private static SaveManager instance;
    public static SaveManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<SaveManager>();
                if (instance == null)
                {
                    instance = new GameObject("SaveManager", typeof(SaveManager)).GetComponent<SaveManager>();
                }
            }

            return instance;
        }
        set
        {
            instance = value;
        }
    }

    public SaveState State
    {
        get
        {
            return state;
        }

        set
        {
            state = value;
        }
    }
    #endregion

    [Header("Logic")]
    [SerializeField] private string savefileName = "data.arek";
    [SerializeField] private bool loadOnStart = true;
    private SaveState state;
    private BinaryFormatter formatter;

    private void Start()
    {
        //Initalize the formatter, make this script persists
        formatter = new BinaryFormatter();
        DontDestroyOnLoad(this.gameObject);

        //If LoadOnStart is toggled, try loading our save file
        if (loadOnStart)
            Load();
    }

    private void Save()
    {
        //If theres no previous state loaded, create a new one
        if (state == null)
            state = new SaveState();

        //Set the time at which we've tried saving
        //state.LastSaveTime = DateTime.Now;

        //Opem a physical file, on your disk to hold the save
        var file = new FileStream(savefileName, FileMode.OpenOrCreate, FileAccess.Write);
        formatter.Serialize(file, state);
        file.Close();
    }

    private void Load()
    {
        //Open a physical file, on your disk to hold the save
        var file = new FileStream(savefileName, FileMode.Open, FileAccess.Read);
        if (file != null)
        {
            //If we found the file, open and read it
            state = (SaveState)formatter.Deserialize(file);
            file.Close();
        }
        else
        {
            Debug.Log("No save file found, creating a new entry.");
            Save();
        }
    }

}
