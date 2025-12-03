using UnityEngine;

public static class SaveSystem
{
    private const string HasSaveKey = "HasSave";
    private const string CheckpointKey = "LastCheckpoint";

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(HasSaveKey, 0) == 1;
    }

    public static void SaveCheckpoint(int checkpointId)
    {
        PlayerPrefs.SetInt(HasSaveKey, 1);
        PlayerPrefs.SetInt(CheckpointKey, checkpointId);
        PlayerPrefs.Save();
    }

    public static int LoadCheckpoint()
    {
        return PlayerPrefs.GetInt(CheckpointKey, 0);
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(HasSaveKey);
        PlayerPrefs.DeleteKey(CheckpointKey);
    }
}
