using System;
using System.Collections.Generic;

namespace WordsOnTheWaves.Data
{
    /// <summary>
    /// Toàn bộ nội dung dialogue trong game. Load từ dialogues_config.json.
    /// Key của Dictionary là CustomerType (string) hoặc BookGenre (string).
    /// </summary>
    [Serializable]
    public class DialogueData
    {
        /// <summary>
        /// Câu chào hỏi theo loại khách.
        /// Key: "Normal" | "NeedAdvice" | "Special"
        /// </summary>
        public Dictionary<string, List<string>> greetings;

        /// <summary>
        /// Gợi ý thể loại sách — dùng khi khách kiểu NeedAdvice / Special mô tả nhu cầu.
        /// Key: "Crime" | "Drama" | "Fact" | "Fantasy" | "Classic" | "Kids" | "Travel"
        /// </summary>
        public Dictionary<string, List<string>> genreHints;

        /// <summary>
        /// Phản hồi khi player tư vấn.
        /// Key: "accept" (đúng genre) | "reject" (sai genre)
        /// </summary>
        public Dictionary<string, List<string>> responses;

        /// <summary>
        /// Câu tạm biệt theo loại khách.
        /// Key: "Normal" | "NeedAdvice" | "Special"
        /// </summary>
        public Dictionary<string, List<string>> farewell;
    }

    /// <summary>
    /// Cấu hình tiền tệ của game. Load từ coins_config.json.
    /// </summary>
    [Serializable]
    public class CoinConfig
    {
        /// <summary>Số coin bắt đầu ngày đầu tiên.</summary>
        public int startingCoins;

        /// <summary>Giá bán tối thiểu của bất kỳ cuốn sách nào (bảo vệ khỏi giá âm sau buff).</summary>
        public int minBookSellPrice;

        /// <summary>Bonus coin cố định thêm vào khi tư vấn đúng (ngoài bonusMultiplier).</summary>
        public int adviceBonusFlat;

        /// <summary>
        /// Chi phí mở khóa từng địa điểm (một lần duy nhất).
        /// Key: location id (ví dụ: "beach_resort")
        /// </summary>
        public Dictionary<string, int> locationUnlockCosts;
    }

    [Serializable]
    public class GenreProximity
    {
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public BookGenre genre;
        
        [Newtonsoft.Json.JsonProperty(ItemConverterType = typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public List<BookGenre> nearGenres;
    }

    [Serializable]
    public class GenreMatrixConfig
    {
        public List<GenreProximity> matrix;
    }

    [Serializable]
    public class PlayerProfile
    {
        public int currentCoins;
    }
}
