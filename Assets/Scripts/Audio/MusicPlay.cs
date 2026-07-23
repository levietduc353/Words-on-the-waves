using UnityEngine;
using WordsOnTheWaves.UI;

namespace WordsOnTheWaves.Audio
{
    /// <summary>
    /// Gắn vào bất kỳ GameObject nào trong 1 Scene cụ thể.
    /// Khi Scene đó được load, nó sẽ tự động ném bài nhạc này cho hệ thống âm thanh tổng (PersistentSettingsCanvas) phát.
    /// </summary>
    public class MusicPlay : MonoBehaviour
    {
        public static MusicPlay Instance { get; private set; }

        [Tooltip("Bài nhạc bạn muốn phát tự động khi vào Scene này")]
        [SerializeField] private AudioClip _sceneMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (_sceneMusic == null)
            {
                Debug.LogWarning("[MusicPlay] Chưa gán file nhạc vào ô Scene Music!");
                return;
            }

            if (PersistentSettingsCanvas.Instance == null)
            {
                Debug.LogWarning("[MusicPlay] Không tìm thấy PersistentSettingsCanvas trong game! Hãy chắc chắn bạn đã chạy game từ scene có chứa nó.");
                return;
            }

            Debug.Log($"[MusicPlay] Yêu cầu phát nhạc: {_sceneMusic.name}");
            PersistentSettingsCanvas.Instance.PlayMusic(_sceneMusic);
        }
    }
}
