using System;
using System.Runtime.Serialization;

namespace GameStore.Model
{
    /// <summary>רכישה - איזה משתמש קנה איזה משחק, מתי ובכמה. שורה בטבלת Purchases</summary>
    [DataContract]
    public class Purchase : BaseEntity
    {
        [DataMember] public int UserId { get; set; }
        [DataMember] public int GameId { get; set; }
        [DataMember] public DateTime PurchaseDate { get; set; }
        [DataMember] public decimal PricePaid { get; set; }

        // שדות תצוגה שמגיעים מ-JOIN (נוח להציג ברשימה בלי קריאות נוספות לשרת)
        [DataMember] public string GameTitle { get; set; }
        [DataMember] public string GameImageUrl { get; set; }
        [DataMember] public string Username { get; set; }
    }
}
