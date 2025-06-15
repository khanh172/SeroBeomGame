using System.Collections.Generic;
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

    [Header("Dialogue")]
    [Tooltip("DialogueManager chịu trách nhiệm hiển thị hội thoại")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Dialogue Lines for Intro before gameplay")]
    [Tooltip("Thiết lập danh sách dòng thoại (speakerName và content)")]
    [SerializeField] private List<DialogueLine> introDialogueLines;

    private bool isStartingDialogue = false;

    private void Awake()
    {
        // Ban đầu show menu, ẩn guide
        if (playCanvas != null) playCanvas.gameObject.SetActive(true);
        if (guideCanvas != null) guideCanvas.gameObject.SetActive(false);

        guideButton.onClick.AddListener(OpenGuide);
        closeGuideButton.onClick.AddListener(CloseGuide);
        playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    private void OpenGuide()
    {
        if (playCanvas != null) playCanvas.gameObject.SetActive(false);
        if (guideCanvas != null) guideCanvas.gameObject.SetActive(true);
    }

    private void CloseGuide()
    {
        if (guideCanvas != null) guideCanvas.gameObject.SetActive(false);
        if (playCanvas != null) playCanvas.gameObject.SetActive(true);
    }

    private void OnPlayButtonClicked()
    {
        // Nếu đã đang show dialogue, ignore
        if (isStartingDialogue) return;

        // Ẩn hết các UI menu
        if (playCanvas != null) playCanvas.gameObject.SetActive(false);
        if (guideCanvas != null) guideCanvas.gameObject.SetActive(false);

        // Bắt đầu hiển thị dialogue
        StartIntroDialogue();
    }

    private void StartIntroDialogue()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("[UIManager] dialogueManager chưa gán!");
            // Nếu không có DialogueManager, load scene ngay
            LoadGameplayScene();
            return;
        }
        if (introDialogueLines == null || introDialogueLines.Count == 0)
        {
            // Không có dòng thoại, load ngay
            LoadGameplayScene();
            return;
        }

        isStartingDialogue = true;

        // Đăng ký sự kiện kết thúc hội thoại
        dialogueManager.onDialogueEnd.AddListener(OnIntroDialogueEnd);

        // Bắt đầu dialogue
        dialogueManager.StartDialogueWithLines(new List<DialogueLine>(introDialogueLines));
    }

    private void OnIntroDialogueEnd()
    {
        // Chỉ xử lý 1 lần
        dialogueManager.onDialogueEnd.RemoveListener(OnIntroDialogueEnd);
        isStartingDialogue = false;

        // Chuyển scene gameplay
        LoadGameplayScene();
    }

    private void LoadGameplayScene()
    {
        // Nếu cần có loading screen có thể gọi ở đây
        SceneManager.LoadScene("Gameplay");
    }
}
