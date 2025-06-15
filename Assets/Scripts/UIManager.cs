using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] private Canvas playCanvas;    
    [SerializeField] private Canvas guideCanvas;   

    [Header("Buttons")]
    [SerializeField] private Button guideButton;   
    [SerializeField] private Button closeGuideButton; 
    [SerializeField] private Button playButton;    

    private void Awake()
    {
        playCanvas.gameObject.SetActive(true);
        guideCanvas.gameObject.SetActive(false);

        guideButton.onClick.AddListener(OpenGuide);
        closeGuideButton.onClick.AddListener(CloseGuide);
        playButton.onClick.AddListener(StartGame);
    }

    private void OpenGuide()
    {
        playCanvas.gameObject.SetActive(false);
        guideCanvas.gameObject.SetActive(true);
    }

    private void CloseGuide()
    {
        guideCanvas.gameObject.SetActive(false);
        playCanvas.gameObject.SetActive(true);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
}
