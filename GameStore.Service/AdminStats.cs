using System.Runtime.Serialization;

namespace GameStore.Service
{
    /// <summary>נתונים מסכמים שמוצגים בדף הראשי של המנהל</summary>
    [DataContract]
    public class AdminStats
    {
        [DataMember] public int UsersCount { get; set; }
        [DataMember] public int GamesCount { get; set; }
        [DataMember] public int PurchasesCount { get; set; }
        [DataMember] public decimal TotalRevenue { get; set; }
    }
}
