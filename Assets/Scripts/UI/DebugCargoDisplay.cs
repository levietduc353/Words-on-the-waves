using System.Text;
using TMPro;
using UnityEngine;
using WordsOnTheWaves.Core;

namespace WordsOnTheWaves.UI
{
    /// <summary>
    /// Script dùng để test xem data sách trên xe ngựa đã được truyền đúng sang Scene Bãi biển hay chưa.
    /// Cách dùng: Tạo một UI TextMeshPro trong Scene Bãi biển, ném script này vào nó.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class DebugCargoDisplay : MonoBehaviour
    {
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            if (DataManager.Instance == null)
            {
                _text.text = "DataManager is missing! (Chạy game từ MainMenu chưa?)";
                return;
            }

            var cargo = DataManager.Instance.GetWagonCargo();
            if (cargo == null || cargo.Count == 0)
            {
                _text.text = "Cargo is Empty! (Không mang sách nào hoặc lỗi truyền data)";
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b>--- HÀNG HÓA TRÊN XE ---</b>");
            
            foreach (var item in cargo)
            {
                // Chỉ hiển thị những sách có số lượng > 0
                if (item.Value > 0)
                {
                    sb.AppendLine($"- {item.Key}: <color=yellow>{item.Value}</color> cuốn");
                }
            }
            
            _text.text = sb.ToString();
        }
    }
}
