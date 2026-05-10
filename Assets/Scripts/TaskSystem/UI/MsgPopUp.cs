using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace haw.pd20.tasksystem.ui
{
    [RequireComponent(typeof(Animator))]
    public class MsgPopUp : MonoBehaviour
    {
        // the animator scales the popup along the y-axis between 0 (closed) and 1 (open).
        public bool IsOpen { get; private set; }

        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private Image headerImage;

        private Animator animator;
        private static readonly int HashOpen = Animator.StringToHash("open");
        private static readonly int HashClose = Animator.StringToHash("close");
        private static readonly int HashOpenAnim = Animator.StringToHash("open-anim");
        private static readonly int HashCloseAnim = Animator.StringToHash("close-anim");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void SetTitle(string text) => title.text = text;
        public void SetMessage(string text) => message.text = text;
        public void SetHeaderColor(Color color) => headerImage.color = color;

        public void Open()
        {
            animator.SetTrigger(HashOpen);
            IsOpen = true;
        }

        public void Close()
        {
            animator.SetTrigger(HashClose);
            IsOpen = false;
        }

        public void OpenAnim()
        {
            animator.SetTrigger(HashOpenAnim);
            IsOpen = true;
        }

        public void CloseAnim()
        {
            animator.SetTrigger(HashCloseAnim);
            IsOpen = false;
        }
    }
}