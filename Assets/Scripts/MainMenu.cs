using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button continueButton;
    [SerializeField] private string gameSceneName = "Game";

    private void Start()
    {
        continueButton.interactable = SaveSystem.HasSave();
    }

    public void OnNewGame()
    {
        SaveSystem.ClearSave();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnContinue()
    {
        if (!SaveSystem.HasSave()) return;

        SceneManager.LoadScene(gameSceneName);
    }
}
