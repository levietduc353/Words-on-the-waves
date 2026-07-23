using UnityEngine;
using UnityEngine.UI;

namespace WordsOnTheWaves.UI
{
    /// <summary>
    /// Canvas cài đặt tồn tại xuyên suốt các Scene, không bị phá hủy.
    /// Quản lý âm lượng của Nhạc nền (Music) và Toàn hệ thống (Global/SFX).
    /// </summary>
    public class PersistentSettingsCanvas : MonoBehaviour
    {
        public static PersistentSettingsCanvas Instance { get; private set; }

        [Header("Audio Settings")]
        [Tooltip("Kéo thả AudioSource phát nhạc nền (Music) vào đây")]
        [SerializeField] private AudioSource _musicSource;

        [Header("UI Controls")]
        [Tooltip("Thanh trượt điều chỉnh nhạc nền")]
        [SerializeField] private Slider _musicSlider;
        
        [Tooltip("Thanh trượt điều chỉnh âm thanh toàn game (SFX + mọi thứ)")]
        [SerializeField] private Slider _globalAudioSlider;

        private void Awake()
        {
            // Singleton pattern & DontDestroyOnLoad để giữ Canvas này qua các scene
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
            // Load lại cấu hình âm thanh đã lưu trước đó (mặc định là 1.0 tức 100%)
            float savedMusicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
            float savedGlobalVol = PlayerPrefs.GetFloat("GlobalVolume", 1f);

            // Gán giá trị lưu trữ lên slider và áp dụng vào game
            if (_musicSlider != null)
            {
                _musicSlider.value = savedMusicVol;
                _musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }
            SetMusicVolume(savedMusicVol);

            if (_globalAudioSlider != null)
            {
                _globalAudioSlider.value = savedGlobalVol;
                _globalAudioSlider.onValueChanged.AddListener(SetGlobalVolume);
            }
            SetGlobalVolume(savedGlobalVol);
        }

        /// <summary>
        /// Hàm điều chỉnh âm lượng của Nhạc nền
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            if (_musicSource != null)
            {
                _musicSource.volume = volume;
            }
            PlayerPrefs.SetFloat("MusicVolume", volume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Hàm điều chỉnh âm thanh tổng của toàn bộ game (Global)
        /// Sử dụng AudioListener.volume sẽ ảnh hưởng đến mọi âm thanh phát ra
        /// </summary>
        public void SetGlobalVolume(float volume)
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat("GlobalVolume", volume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Phát một bài nhạc mới (dùng cho việc chuyển Scene).
        /// Sẽ không phát lại nếu bài đó đang chạy.
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (_musicSource == null)
            {
                Debug.LogError("[PersistentSettingsCanvas] Chưa kéo thả AudioSource vào ô Music Source trong Inspector!");
                return;
            }
            if (clip == null) return;

            // Chống giật nhạc nếu scene mới dùng chung bài nhạc với scene cũ
            if (_musicSource.clip == clip && _musicSource.isPlaying)
            {
                Debug.Log($"[PersistentSettingsCanvas] Bài {clip.name} đang phát rồi, bỏ qua.");
                return;
            }

            _musicSource.clip = clip;
            _musicSource.Play();
            Debug.Log($"[PersistentSettingsCanvas] Bắt đầu phát nhạc: {clip.name} với âm lượng: {_musicSource.volume}");
        }
    }
}
