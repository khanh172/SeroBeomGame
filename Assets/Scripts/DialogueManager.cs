using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;        // Panel gốc chứa UI thoại
    public TextMeshProUGUI nameText;        // Text hiển thị tên nhân vật
    public TextMeshProUGUI dialogueText;    // Text hiển thị nội dung thoại
    public GameObject clickIcon;            // Icon/Text "Click to continue"
    public Button nextButton;               // Button phủ toàn panel để Next (tùy chọn)

    [Header("Settings")]
    public float typeSpeed = 0.03f;         // Tốc độ typewriter

    [Header("Events")]
    public UnityEvent onDialogueEnd;        // Sự kiện sẽ invoke khi toàn bộ hội thoại kết thúc

    // Biến nội bộ
    private List<DialogueLine> dialogueLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool cancelTyping = false;
    private bool isDialogueActive = false;

    // Cho phép bên ngoài kiểm tra xem Dialogue đang chạy hay không
    public bool IsDialogueActive => isDialogueActive;

    void Awake()
    {
        // Ẩn panel ban đầu
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Đăng ký listener cho nextButton nếu có
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnClickNextLine);
        }
    }

    void Update()
    {
        if (!isDialogueActive)
            return;

        // Khi click chuột cũng Next:
        if (Input.GetMouseButtonDown(0))
        {
            OnClickNextLine();
        }
    }

    /// <summary>
    /// Bắt đầu một cuộc hội thoại với danh sách DialogueLine
    /// </summary>
    public void StartDialogueWithLines(List<DialogueLine> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] StartDialogueWithLines: lines rỗng!");
            // Nếu muốn invoke kết thúc ngay, bạn có thể gọi onDialogueEnd ở đây
            onDialogueEnd?.Invoke();
            return;
        }

        if (isDialogueActive)
        {
            Debug.Log("[DialogueManager] Dialogue đang chạy, bỏ qua StartDialogue.");
            return;
        }

        dialogueLines = lines;
        currentLineIndex = 0;
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (clickIcon != null)
            clickIcon.SetActive(false);

        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        cancelTyping = false;

        DialogueLine line = dialogueLines[currentLineIndex];

        if (nameText != null)
            nameText.text = line.speakerName;

        if (dialogueText != null)
            dialogueText.text = "";

        string content = line.content;
        for (int i = 0; i < content.Length; i++)
        {
            if (cancelTyping)
            {
                dialogueText.text = content;
                break;
            }
            dialogueText.text += content[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;

        if (clickIcon != null)
            clickIcon.SetActive(true);
    }

    public void OnClickNextLine()
    {
        if (!isDialogueActive)
            return;

        if (isTyping)
        {
            cancelTyping = true;
        }
        else
        {
            currentLineIndex++;
            if (currentLineIndex < dialogueLines.Count)
            {
                if (clickIcon != null)
                    clickIcon.SetActive(false);

                StartCoroutine(TypeLine());
            }
            else
            {
                EndDialogue();
            }
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (clickIcon != null)
            clickIcon.SetActive(false);

        onDialogueEnd?.Invoke();
    }

    public void ForceEndDialogue()
    {
        if (!isDialogueActive) return;
        StopAllCoroutines();
        EndDialogue();
    }
}
