using System;
using System.Collections.Generic;

namespace WordsOnTheWaves.Data
{
    /// <summary>
    /// 7 thể loại sách trong game.
    /// </summary>
    public enum BookGenre
    {
        Crime,
        Drama,
        Fact,
        Fantasy,
        Classic,
        Kids,
        Travel
    }

    /// <summary>
    /// Dữ liệu một cuốn sách cụ thể. Load từ books_config.json.
    /// </summary>
    [Serializable]
    public class BookData
    {
        /// <summary>ID duy nhất của sách (ví dụ: "book_crime_01").</summary>
        public string id;

        /// <summary>Thể loại sách.</summary>
        public BookGenre genre;

        /// <summary>Giá bán cho khách hàng (đơn vị: coin).</summary>
        public int sellPrice;

        /// <summary>Mô tả sách — dùng trong hệ thống tư vấn khách hàng.</summary>
        public string description;
    }

    // --- Wrapper dùng cho JsonUtility (nếu cần), chính là root object của JSON ---
    [Serializable]
    internal class BookDataList
    {
        public List<BookData> books;
    }
}
