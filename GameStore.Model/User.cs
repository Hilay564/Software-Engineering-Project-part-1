using System;
using System.Runtime.Serialization;

namespace GameStore.Model
{
    /// <summary>
    /// משתמש במערכת - שורה בטבלת Users.
    /// שימו לב: Password משמש רק כדי *לשלוח* סיסמה חדשה לשרת (בהרשמה / שינוי סיסמה).
    /// השרת אף פעם לא מחזיר סיסמה ללקוח - הוא שומר רק Hash.
    /// </summary>
    [DataContract]
    public class User : BaseEntity
    {
        [DataMember] public string Username { get; set; }
        [DataMember] public string Password { get; set; }
        [DataMember] public string FirstName { get; set; }
        [DataMember] public string LastName { get; set; }
        [DataMember] public string Email { get; set; }
        [DataMember] public string Phone { get; set; }
        [DataMember] public DateTime BirthDate { get; set; }
        [DataMember] public string Gender { get; set; }
        [DataMember] public int CityId { get; set; }
        [DataMember] public string CityName { get; set; }   // לא עמודה בטבלה - מגיע מ-JOIN עם Cities
        [DataMember] public bool IsAdmin { get; set; }
        [DataMember] public DateTime JoinDate { get; set; }

        public string FullName => FirstName + " " + LastName;
    }
}
