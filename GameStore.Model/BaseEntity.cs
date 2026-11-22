using System.Runtime.Serialization;

namespace GameStore.Model
{
    /// <summary>
    /// מחלקת בסיס לכל הישויות. לכל שורה בכל טבלה יש מספר מזהה (Id),
    /// אז במקום לכתוב Id בכל מחלקה - כולן יורשות ממחלקה זו (ירושה / Inheritance).
    /// </summary>
    [DataContract]   // אומר ל-WCF: את המחלקה הזאת מותר לשלוח דרך שירות הרשת
    public class BaseEntity
    {
        [DataMember] // השדה הזה יישלח ברשת
        public int Id { get; set; }
    }
}
