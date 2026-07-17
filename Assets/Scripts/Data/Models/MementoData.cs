using System;
using System.Collections.Generic;

namespace WordsOnTheWaves.Data
{
    /// <summary>
    /// Dữ liệu một kỷ vật (Memento) nhận từ Special Customer. Load từ memento_config.json.
    /// BuffType dùng chung enum từ DecorItemData.cs.
    /// </summary>
    [Serializable]
    public class MementoData
    {
        /// <summary>ID duy nhất (ví dụ: "memento_detective_badge").</summary>
        public string id;

        /// <summary>Tên hiển thị trên UI.</summary>
        public string displayName;

        /// <summary>Tên Prefab trong Resources/Prefabs/ để hiển thị vật thể 3D.</summary>
        public string prefabName;

        /// <summary>Loại buff mà kỷ vật này cung cấp (chia sẻ enum với DecorItemData).</summary>
        public BuffType buffType;

        /// <summary>Phần trăm buff (ví dụ: 12.0 = +12%).</summary>
        public float buffPercent;

        /// <summary>Mô tả câu chuyện / lore của kỷ vật.</summary>
        public string description;
    }

    [Serializable]
    internal class MementoDataList
    {
        public List<MementoData> mementos;
    }
}
