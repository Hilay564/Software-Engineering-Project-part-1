using System.Runtime.Serialization;

namespace GameStore.Model
{
    /// <summary>עיר - מייצגת שורה בטבלת Cities</summary>
    [DataContract]
    public class City : BaseEntity
    {
        [DataMember] public string Name { get; set; }

        public override string ToString() => Name;
    }
}
