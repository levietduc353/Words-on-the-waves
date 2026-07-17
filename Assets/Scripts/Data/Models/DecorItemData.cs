using System;
using System.Collections.Generic;

namespace WordsOnTheWaves.Data
{
    /// <summary>
    /// Các loại buff mà Decor và Memento có thể cung cấp.
    /// </summary>
    public enum BuffType
    {
        /// <summary>Tăng % giá bán sách.</summary>
        PriceBonus,

        /// <summary>Tăng % tốc độ phục vụ / thời gian khách ở lại.</summary>
        CustomerSpeed,

        /// <summary>Tăng % tỉ lệ thu hút thêm khách hàng.</summary>
        Attraction
    }

    /// <summary>
    /// Dữ liệu một vật trang trí. Load từ decor_config.json.
    /// </summary>
    [Serializable]
    public class DecorItemData
    {
        /// <summary>ID duy nhất (ví dụ: "decor_lamp").</summary>
        public string id;

        /// <summary>Tên hiển thị trên UI.</summary>
        public string displayName;

        /// <summary>Tên Prefab trong Resources/Prefabs/ để spawn vật thể 3D.</summary>
        public string prefabName;

        /// <summary>Loại buff mà vật trang trí này cung cấp.</summary>
        public BuffType buffType;

        /// <summary>Phần trăm buff (ví dụ: 5.0 = +5%).</summary>
        public float buffPercent;
    }

    [Serializable]
    internal class DecorItemDataList
    {
        public List<DecorItemData> decorItems;
    }
}
