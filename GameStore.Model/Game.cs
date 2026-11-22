using System.Runtime.Serialization;

namespace GameStore.Model
{
    /// <summary>משחק - המוצר שהחנות מוכרת. שורה בטבלת Games</summary>
    [DataContract]
    public class Game : BaseEntity
    {
        [DataMember] public string Title { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public string Developer { get; set; }
        [DataMember] public decimal Price { get; set; }
        [DataMember] public int ReleaseYear { get; set; }
        [DataMember] public int MinAge { get; set; }
        [DataMember] public int GenreId { get; set; }
        [DataMember] public string GenreName { get; set; }  // מגיע מ-JOIN עם Genres
        [DataMember] public string ImageUrl { get; set; }
    }
}
