using System;
using System.Collections.Generic;

namespace WordsOnTheWaves.Data
{
    /// <summary>
    /// Dữ liệu một bãi biển / địa điểm trên bản đồ. Load từ locations_config.json.
    /// </summary>
    [Serializable]
    public class LocationData
    {
        /// <summary>ID duy nhất (ví dụ: "beach", "cafe").</summary>
        public string id;

        /// <summary>Tên scene Unity tương ứng.</summary>
        public string sceneName;

        /// <summary>Phí di chuyển đến địa điểm này (đơn vị: coin). 0 = miễn phí.</summary>
        public int travelFee;
    }

    [Serializable]
    internal class LocationDataList
    {
        public List<LocationData> locations;
    }
}
