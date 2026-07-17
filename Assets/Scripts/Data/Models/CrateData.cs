using System;
using System.Collections.Generic;

namespace WordsOnTheWaves.Data
{
    /// <summary>
    /// Một slot trong kiện hàng — xác định thể loại sách và số lượng drop.
    /// </summary>
    [Serializable]
    public class CrateGenreSlot
    {
        /// <summary>Thể loại sách trong slot này.</summary>
        public BookGenre genre;

        /// <summary>Số lượng sách tối thiểu drop từ slot này (có thể = 0).</summary>
        public int dropMin;

        /// <summary>Số lượng sách tối đa drop từ slot này.</summary>
        public int dropMax;
    }

    /// <summary>
    /// Dữ liệu một kiện hàng trong shop. Load từ crates_config.json.
    /// </summary>
    [Serializable]
    public class CrateData
    {
        /// <summary>ID duy nhất của kiện hàng (ví dụ: "crate_mystery").</summary>
        public string id;

        /// <summary>Tên hiển thị trên UI shop.</summary>
        public string displayName;

        /// <summary>Giá mua kiện hàng (đơn vị: coin).</summary>
        public int cratePrice;

        /// <summary>Mô tả mờ ảo — player thấy cái này, không thấy nội dung thật.</summary>
        public string hint;

        /// <summary>
        /// Các slot thể loại. Mỗi slot roll độc lập khi mở kiện hàng.
        /// Logic: foreach slot → count = Random(dropMin, dropMax) → lấy ngẫu nhiên 'count' sách thuộc genre đó.
        /// </summary>
        public List<CrateGenreSlot> genreSlots;
    }

    [Serializable]
    internal class CrateDataList
    {
        public List<CrateData> crates;
    }
}
