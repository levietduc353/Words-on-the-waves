using System;
using System.Collections.Generic;

namespace WordsOnTheWaves.Data
{
    /// <summary>
    /// 3 loại khách hàng trong game.
    /// </summary>
    public enum CustomerType
    {
        /// <summary>Tự chọn sách và tự trả tiền, không cần tư vấn.</summary>
        Normal,

        /// <summary>Cần tư vấn: player chọn đúng thể loại → nhận bonus tiền.</summary>
        NeedAdvice,

        /// <summary>Giống NeedAdvice nhưng cung cấp Memento khi hoàn thành đúng.</summary>
        Special
    }

    /// <summary>
    /// Quy tắc spawn cho từng loại khách hàng. Load từ customers_config.json.
    /// </summary>
    [Serializable]
    public class CustomerSpawnRule
    {
        /// <summary>Loại khách hàng.</summary>
        public CustomerType customerType;

        /// <summary>Tỉ lệ xuất hiện (weighted random). Không cần tổng = 100.</summary>
        public int spawnWeight;

        /// <summary>Hệ số nhân giá bán khi tư vấn đúng (Normal = 1.0).</summary>
        public float bonusMultiplier;
    }

    /// <summary>
    /// Dữ liệu riêng của một Special Customer. Load từ customers_config.json.
    /// </summary>
    [Serializable]
    public class SpecialCustomerData
    {
        /// <summary>ID duy nhất của special customer (ví dụ: "special_detective").</summary>
        public string id;

        /// <summary>Thể loại sách mà special customer này yêu cầu.</summary>
        public BookGenre preferredGenre;

        /// <summary>ID của Memento sẽ trao cho player khi tư vấn đúng.</summary>
        public string mementoId;
    }

    /// <summary>
    /// Wrapper cho toàn bộ customers_config.json.
    /// </summary>
    [Serializable]
    internal class CustomerConfig
    {
        public List<CustomerSpawnRule> spawnRules;
        public List<SpecialCustomerData> specialCustomers;
    }
}
