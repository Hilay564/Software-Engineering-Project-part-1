using System.Runtime.Serialization;

namespace GameStore.Model
{
    /// <summary>ז'אנר (קטגוריה) של משחק - שורה בטבלת Genres</summary>
    [DataContract]
    public class Genre : BaseEntity
    {
        [DataMember] public string Name { get; set; }

        public override string ToString() => Name;
    }
}
