# הסבר מלא על הפרויקט: מה הקוד עושה ולמה

המסמך הזה נועד לסגור את הפער מהשיעורים. כדאי לקרוא אותו לפי הסדר, ולפתוח במקביל את הקבצים שמוזכרים.
בסוף יש רשימה של שאלות שמורים נוטים לשאול בהגנה על פרויקט, עם תשובות.

---

## 1. התמונה הגדולה: למה יש 4 פרויקטים?

הפרויקט בנוי ב**שכבות**. כל שכבה אחראית על דבר אחד ומדברת רק עם השכבה שלידה:

```
┌────────────────────┐     ┌────────────────────┐     ┌────────────────────┐     ┌──────────────┐
│  GameStore.App     │     │  GameStore.Service │     │ GameStore.ViewModel│     │  SQL Server  │
│  (UWP - המסכים)    │ ──► │  (WCF - שירות רשת) │ ──► │ (Connection, XxxDB)│ ──► │ (הטבלאות)    │
└────────────────────┘     └────────────────────┘     └────────────────────┘     └──────────────┘
                                     ▲                          ▲
                                     └──── GameStore.Model ─────┘
                                     (User, Game... משותף לשניהם)
```

**למה לא לכתוב הכול באפליקציה?**
- **הפרדת אחריות:** אם מחליפים מסד נתונים, משנים רק את ViewModel.
- **אבטחה:** האפליקציה אף פעם לא מתחברת ישירות למסד הנתונים. רק השרת יודע את מחרוזת החיבור.
- **כמה לקוחות:** אותו שירות יכול לשרת גם אתר, גם אפליקציית טלפון וגם UWP.

### דוגמה: מה קורה כשמשתמש לוחץ "התחבר"?

1. `LoginPage.xaml.cs` בודק שהשדות לא ריקים, ואז קורא ל-`ServiceHelper.Client.LoginAsync("dani", "Dani1234")`
2. הבקשה נשלחת ברשת (HTTP) לשירות ה-WCF
3. `GameService.Login` מקבל אותה וקורא ל-`db.Users.Login(...)`
4. `UserDB.Login` מחשב Hash לסיסמה ומריץ `SELECT ... WHERE Username=@Username AND PasswordHash=@Hash`
5. SQL Server מחזיר שורה (או כלום)
6. `UserDB.CreateModel` הופך את השורה לאובייקט `User`
7. השירות מחזיר את ה-`User` ברשת לאפליקציה
8. האפליקציה שומרת אותו ב-`Session.CurrentUser` ועוברת לדף הבית של המשתמש. התפריט מתעדכן לבד.

אם מבינים את 8 השלבים האלה, מבינים את כל הפרויקט. **כל** פעולה עובדת באותה צורה.

---

## 2. מסד הנתונים: `Database/CreateDatabase.sql`

| טבלה | מה היא שומרת | קשרים |
|------|---------------|--------|
| `Cities` | ערים (בשביל ה-ComboBox בהרשמה) | |
| `Genres` | ז'אנרים של משחקים | |
| `Users` | משתמשים | `CityId` ← Cities |
| `Games` | משחקים (המוצרים) | `GenreId` ← Genres |
| `Purchases` | מי קנה מה | `UserId` ← Users, `GameId` ← Games |

מושגים שחשוב להכיר:
- **PRIMARY KEY**: מזהה ייחודי לכל שורה. `IDENTITY(1,1)` אומר שהמספר נוצר אוטומטית (1, 2, 3...).
- **FOREIGN KEY** (`REFERENCES`): עמודה שמצביעה על שורה בטבלה אחרת. אי אפשר להכניס `CityId=999` אם אין עיר כזאת.
- **UNIQUE**: אסור שני ערכים זהים. כך אין שני משתמשים עם אותו Username.
- **`ON DELETE CASCADE`**: כשמוחקים משתמש, הרכישות שלו נמחקות אוטומטית.
- **`UNIQUE (UserId, GameId)`** ב-Purchases: אי אפשר לקנות אותו משחק פעמיים.
- `Purchases` היא **טבלת קשר רבים-לרבים**: למשתמש יש הרבה משחקים, ומשחק נקנה ע"י הרבה משתמשים.
- **NVARCHAR** (ולא VARCHAR): תומך בעברית.

**למה `PasswordHash` ולא `Password`?** לא שומרים סיסמאות במסד הנתונים. שומרים "טביעת אצבע" (Hash) שלהן. פירוט בסעיף 4.

---

## 3. שכבת Model: `GameStore.Model`

כל מחלקה מייצגת **שורה אחת** בטבלה. לדוגמה, `Game.cs`:

```csharp
[DataContract]
public class Game : BaseEntity
{
    [DataMember] public string Title { get; set; }
    [DataMember] public decimal Price { get; set; }
    ...
    [DataMember] public string GenreName { get; set; }  // מגיע מ-JOIN
}
```

- **`: BaseEntity`** היא **ירושה**. ל-`BaseEntity` יש `Id`, וכל הישויות יורשות אותו. כך לא כותבים `Id` חמש פעמים.
- **`[DataContract]` / `[DataMember]`** אומרים ל-WCF שמותר לשלוח את האובייקט ברשת ואילו שדות לשלוח. בלי זה השירות לא יוכל להחזיר `Game`.
- **`GenreName`** הוא לא עמודה בטבלה `Games`. הוא מגיע מ-JOIN עם `Genres`, כדי שהמסך יוכל להציג "אקשן" ולא "1".

---

## 4. שכבת הגישה לנתונים: `GameStore.ViewModel`

זה החלק הכי "הנדסי" בפרויקט, ושווה להבין אותו טוב.

### `Connection.cs`: הדרישה של המורה
```csharp
public class Connection
{
    internal static SqlConnection Open() { ... }   // פותח חיבור ל-SQL Server

    public UserDB Users { get; } = new UserDB();
    public GameDB Games { get; } = new GameDB();
    ...
}
```
מחלקה אחת שנותנת גישה ל**כל הטבלאות**. בשירות כותבים: `db.Games.SelectAll()`, `db.Users.Login(...)`.
את מחרוזת החיבור היא קוראת מ-`Web.config` של השירות (`ConfigurationManager`).

### `BaseDB<T>`: מחלקה אבסטרקטית וגנרית
יש 5 טבלאות, ולכל אחת צריך "תביא הכול", "תביא לפי Id", "מחק". במקום לכתוב את זה 5 פעמים:

```csharp
public abstract class BaseDB<T> where T : BaseEntity
{
    protected abstract string TableName { get; }          // כל טבלה אומרת מה השם שלה
    protected abstract string SelectSql { get; }          // ואיך עושים לה SELECT
    protected abstract T CreateModel(SqlDataReader r);    // ואיך הופכים שורה לאובייקט

    public List<T> SelectAll() { ... }     // כתוב פעם אחת, עובד לכולן
    public T SelectById(int id) { ... }
    public bool Delete(int id) { ... }
}
```
- **`<T>` (גנרי):** `UserDB : BaseDB<User>` מחזיר `List<User>`, ו-`GameDB : BaseDB<Game>` מחזיר `List<Game>`.
- **`abstract`:** "כל מי שיורש **חייב** לממש את זה". הקומפיילר לא ייתן לשכוח.
- **`where T : BaseEntity`:** T חייב להיות ישות עם Id.

### איך שאילתה רצה (`BaseDB.Select`)
```csharp
using (SqlConnection connection = Connection.Open())          // 1. פותחים חיבור
using (SqlCommand command = new SqlCommand(sql, connection))  // 2. מכינים פקודה
{
    command.Parameters.AddRange(parameters);                   // 3. מוסיפים פרמטרים
    using (SqlDataReader reader = command.ExecuteReader())     // 4. מריצים
        while (reader.Read())                                  // 5. עוברים שורה-שורה
            list.Add(CreateModel(reader));                     // 6. שורה -> אובייקט
}
```
**`using`** סוגר את החיבור אוטומטית בסוף, גם אם הייתה שגיאה. בלי זה החיבורים נשארים פתוחים והשרת נתקע.

שלוש פעולות בסיס:
| מתודה | פקודת ADO.NET | מתי |
|-------|---------------|-----|
| `Select` | `ExecuteReader` | SELECT שמחזיר שורות |
| `Execute` | `ExecuteNonQuery` | INSERT / UPDATE / DELETE (מחזיר כמה שורות השתנו) |
| `Scalar` | `ExecuteScalar` | ערך יחיד: `COUNT(*)`, `SUM`, ה-Id החדש |

### פרמטרים ו-SQL Injection (שאלה קלאסית!)
**לא נכון ומסוכן:**
```csharp
"SELECT * FROM Users WHERE Username = '" + username + "'"
```
אם מישהו יקליד בשם המשתמש `' OR '1'='1`, השאילתה תחזיר את כל המשתמשים, והוא ייכנס בלי סיסמה.

**נכון (מה שעשינו):**
```csharp
"SELECT * FROM Users WHERE Username = @Username", P("@Username", username)
```
SQL Server מתייחס לפרמטר כ**נתון** ולא כקוד, כך שהתקפה כזאת לא עובדת.

### Hash לסיסמאות: `PasswordHelper.cs`
- **SHA-256** הופך כל טקסט ל-64 תווים: `"Dani1234"` הופך ל-`"9c1d43...b3c9"`.
- זו פונקציה **חד-כיוונית**: אי אפשר לחזור מה-Hash לסיסמה.
- **בהרשמה** שומרים את `Hash(password)`.
- **בהתחברות** מחשבים `Hash(מה שהוקלד)` ובודקים אם הוא שווה למה שבטבלה.
- אם מישהו גונב את מסד הנתונים, הוא לא רואה את הסיסמאות.

### `SCOPE_IDENTITY()`
```sql
INSERT INTO Games (...) VALUES (...); SELECT CAST(SCOPE_IDENTITY() AS INT);
```
מחזיר את ה-Id שנוצר הרגע. כך `Register` ו-`AddGame` יודעים מה ה-Id של השורה החדשה.

### JOIN
```sql
SELECT t.*, g.Name AS GenreName FROM Games t INNER JOIN Genres g ON t.GenreId = g.Id
```
"תביא כל משחק, ולידו את שם הז'אנר שלו". `t` ו-`g` הם כינויים (aliases). המוסכמה שלנו: הטבלה הראשית תמיד נקראת `t`, וכך `BaseDB` יכול להוסיף `WHERE t.Id = @Id` לכל טבלה.

---

## 5. שירות הרשת: `GameStore.Service`

### `IGameService.cs`: החוזה
```csharp
[ServiceContract]
public interface IGameService
{
    [OperationContract] User Login(string username, string password);
    [OperationContract] List<Game> GetAllGames();
    ...
}
```
- **interface** הוא רק רשימת פעולות, בלי מימוש.
- **`[ServiceContract]`** מסמן ממשק שהוא שירות WCF.
- **`[OperationContract]`** מסמן פעולה שאפשר לקרוא לה מרחוק.

### `GameService.svc.cs`: המימוש
כל פעולה קצרה מאוד: היא מעבירה את הבקשה ל-`Connection`.
```csharp
public List<Game> GetAllGames() => db.Games.SelectAll();
```
(`=>` הוא כתיב מקוצר ל-`{ return ...; }`)

בנוסף, יש בשירות **בדיקות צד-שרת** (`IsValidUser`, `IsValidGame`). למה לבדוק שוב אם האפליקציה כבר בודקת? כי **אסור לסמוך על הלקוח**: מישהו יכול לשלוח בקשה ישירה לשירות בלי לעבור באפליקציה.

עוד החלטות בשירות:
- `Register` תמיד קובע `IsAdmin = false`, כדי שאי אפשר יהיה להירשם כמנהל.
- `BuyGame` לוקח את המחיר מטבלת Games ולא מהלקוח, כדי שאי אפשר יהיה לזייף מחיר.
- `UpdateUser` לא משנה `IsAdmin`. לזה יש פעולה נפרדת `SetUserAdmin` שרק דף המנהל קורא לה.

### `Web.config`
- `connectionStrings`: לאן להתחבר. `(LocalDB)\MSSQLLocalDB` הוא SQL Server קטן שמגיע עם Visual Studio.
- `serviceMetadata httpGetEnabled`: מאפשר ל-Visual Studio "לקרוא" את השירות כשמוסיפים Service Reference.
- `includeExceptionDetailInFaults`: בזמן פיתוח, מראה שגיאות מפורטות.

### Service Reference: מה זה בעצם?
כשמוסיפים Service Reference באפליקציה, Visual Studio קורא את החוזה מהשירות ו**מייצר קוד**:
- מחלקה `GameServiceClient` עם כל הפעולות, בגרסה אסינכרונית: `LoginAsync`, `GetAllGamesAsync`...
- עותק של המחלקות `User`, `Game` וכו' בצד הלקוח, בתוך `GameStore.App.GameServiceReference`.

לכן באפליקציה כתוב `using GameStore.App.GameServiceReference;` ולא `using GameStore.Model;`.

---

## 6. האפליקציה: `GameStore.App` (UWP)

### מבנה כל דף
כל דף מורכב משני קבצים:
- **`XxxPage.xaml`**: איך הדף **נראה** (פקדים, מיקום, עיצוב)
- **`XxxPage.xaml.cs`** (code-behind): מה הדף **עושה** (לחיצות, קריאות לשרת)

פקד עם `x:Name="UsernameBox"` ב-XAML זמין בקוד בתור משתנה `UsernameBox`.

### `App.xaml`: עיצוב אחיד
כל הצבעים והסגנונות מוגדרים פעם אחת:
```xml
<Style x:Key="PageTitleStyle" TargetType="TextBlock"> ... </Style>
```
וכל דף משתמש בהם: `Style="{StaticResource PageTitleStyle}"`. כך כל הדפים נראים אותו דבר, וכדי לשנות צבע בכל האפליקציה משנים רק במקום אחד.

### `MainPage`: המעטפת
- **`NavigationView`**: תפריט הצד.
- **`Frame` (ContentFrame)**: האזור שבו הדפים מתחלפים.
- `BuildMenu()` בונה את התפריט לפי מי שמחובר: אורח, משתמש או מנהל.
- כל פריט בתפריט שומר ב-`Tag` את סוג הדף (`typeof(GamesPage)`), ובלחיצה מתבצע `ContentFrame.Navigate(thatType)`.
- `Session.Changed += BuildMenu`: בכל התחברות או התנתקות התפריט נבנה מחדש אוטומטית (**אירוע / event**).

### ניווט בין דפים
```csharp
Frame.Navigate(typeof(GameDetailsPage), game.Id);   // שולחים פרמטר
```
ובדף היעד:
```csharp
protected override async void OnNavigatedTo(NavigationEventArgs e)
{
    int gameId = (int)e.Parameter;                   // מקבלים אותו
```
`OnNavigatedTo` רץ **בכל פעם** שנכנסים לדף. שם טוענים נתונים מהשרת.

`Frame.BackStack.Clear()` מוחק את היסטוריית הניווט. משתמשים בזה אחרי התחברות והתנתקות, כדי שכפתור "חזור" לא יחזיר לדף שכבר לא רלוונטי.

### `Helpers/Session.cs`: מי מחובר
מחלקה **`static`**, כלומר יש ממנה עותק אחד לכל האפליקציה. כל דף יכול לבדוק `Session.IsLoggedIn` או `Session.IsAdmin`.
דפים מוגנים בודקים את זה ב-`OnNavigatedTo` ומעבירים לדף ההתחברות אם צריך.

### `async` / `await`
```csharp
private async void LoginButton_Click(...)
{
    User user = await ServiceHelper.Client.LoginAsync(...);
```
קריאה לשרת לוקחת זמן. בזכות `await`, המסך לא "קופא" בזמן ההמתנה: הקוד ממשיך מהשורה הבאה רק כשהתשובה מגיעה.

### `try / catch / finally`
```csharp
try     { ... קריאה לשרת ... }
catch (Exception ex) { await ServiceHelper.ShowErrorAsync(ex); }  // השרת לא זמין: מציגים הודעה במקום קריסה
finally { LoginButton.IsEnabled = true; }                          // רץ תמיד
```

### Binding ו-DataTemplate (הצגת רשימות)
```xml
<GridView x:Name="GamesGrid">
    <GridView.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Title}" />
            <TextBlock Text="{Binding Price, Converter={StaticResource PriceConverter}}" />
```
בקוד: `GamesGrid.ItemsSource = filtered;`. ה-GridView יוצר כרטיס לכל משחק ברשימה, ו-`{Binding Title}` לוקח את `Title` מאותו משחק.

### Converters: `Helpers/Converters.cs`
משנים ערך לפני שהוא מוצג:
- `PriceConverter`: `89.9` הופך ל-`"₪89.90"` (ו-0 הופך ל-"חינם")
- `ImageUrlConverter`: כתובת ריקה הופכת לתמונת ברירת מחדל
- `DateConverter`: תאריך הופך ל-`dd/MM/yyyy`

### בדיקות קלט: `Helpers/Validator.cs`
כל בדיקה מחזירה `null` (תקין) או טקסט של שגיאה:
```csharp
public static string Email(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return "יש להזין כתובת אימייל";
    if (!EmailRegex.IsMatch(value.Trim())) return "כתובת אימייל לא תקינה";
    return null;
}
```
ו-`Validator.Show(errorTextBlock, error)` מציג את השגיאה מתחת לשדה ומחזיר `true` אם תקין.

**למה `ok &= ...` ולא `if`?** `&=` מריץ את **כל** הבדיקות, כך שהמשתמש רואה את כל השגיאות בבת אחת, ולא רק את הראשונה.

**Regex (ביטוי רגולרי)** הוא תבנית טקסט:
| ביטוי | פירוש |
|-------|--------|
| `^[A-Za-z][A-Za-z0-9_]{3,15}$` | שם משתמש: מתחיל באות, ועוד 3-15 אותיות/ספרות/קו תחתון |
| `^05\d-?\d{7}$` | טלפון: 05, ספרה, מקף אופציונלי, 7 ספרות |
| `[A-Z]` | יש לפחות אות גדולה אחת |

כללי הסיסמה: 8-20 תווים, אות גדולה, אות קטנה, ספרה ובלי רווחים, ובנוסף **אימות סיסמה** (שתי הסיסמאות זהות).

### `Controls/UserForm`: רכיב לשימוש חוזר
שלושה דפים צריכים את אותם שדות משתמש: הרשמה, עריכת פרופיל וניהול משתמשים. במקום להעתיק את הקוד 3 פעמים, יצרנו **UserControl** אחד עם:
- `Configure(newUser)`: מצב הרשמה (שם משתמש וסיסמה חובה) או מצב עריכה (שם משתמש נעול, סיסמה לא חובה)
- `LoadCitiesAsync()`: ממלא את ה-**ComboBox** של הערים מהשרת
- `Fill(user)`: ממלא את השדות מאובייקט
- `Validate()`: בודק הכול
- `ToUser()`: יוצר אובייקט User מהשדות

---

## 7. הדפים: מה כל אחד עושה

| דף | מי רואה | מה הוא עושה |
|----|---------|---------------|
| `HomePage` | כולם | דף שער: הסבר קצר וכפתורים לקטלוג, התחברות והרשמה |
| `AboutPage` | כולם | דף תוכן: הסבר מלא על האפליקציה |
| `LoginPage` | אורח | התחברות. מנהל מועבר ללוח המנהל, ומשתמש לדף שלו |
| `RegisterPage` | אורח | הרשמה עם כל הבדיקות. בודק מול השרת אם שם המשתמש תפוס, ומחבר אוטומטית |
| `GamesPage` | כולם | קטלוג: חיפוש בטקסט וסינון לפי ז'אנר (ComboBox) |
| `GameDetailsPage` | כולם | פרטי משחק. אורח רואה "התחבר כדי לרכוש", משתמש יכול לרכוש, ומנהל יכול גם לערוך ולמחוק |
| `UserHomePage` | משתמש | דף ראשי: סטטיסטיקה אישית ומשחקים חדשים |
| `MyGamesPage` | משתמש | המשחקים שרכשתי |
| `EditProfilePage` | משתמש | שינוי פרטים אישיים וסיסמה |
| `AdminHomePage` | מנהל | לוח בקרה: מספר משתמשים, משחקים, רכישות, הכנסות ורכישות אחרונות |
| `ManageUsersPage` | מנהל | רשימת משתמשים: עריכה, מחיקה ומתן/ביטול הרשאת מנהל |
| `AddGamePage` | מנהל | הוספת משחק או עריכת משחק קיים, והוספת ז'אנר חדש |

---

## 8. שאלות שמורה עשוי לשאול, ותשובות

**ש: מה התפקיד של מחלקת Connection?**
ת: היא נקודת הכניסה היחידה למסד הנתונים. היא פותחת חיבורים לפי מחרוזת החיבור שב-Web.config, ונותנת גישה לכל טבלה דרך מאפיין (`Users`, `Games` וכו'). כל מאפיין הוא אובייקט שיורש מ-`BaseDB<T>`.

**ש: למה צריך שירות רשת? למה לא להתחבר למסד הנתונים מהאפליקציה?**
ת: אבטחה (פרטי החיבור לא נמצאים אצל הלקוח), הפרדת שכבות, ואפשרות לשרת כמה סוגי לקוחות. בנוסף, ל-UWP אין גישה נוחה ל-SQL Server ישירות.

**ש: מה זה `[DataContract]`?**
ת: סימון שאומר ל-WCF שמותר להמיר את המחלקה ל-XML ולשלוח אותה ברשת (סריאליזציה). `[DataMember]` מסמן אילו שדות לשלוח.

**ש: איך שמרת על הסיסמאות?**
ת: שומרים רק Hash של SHA-256. בהתחברות מחשבים Hash למה שהוקלד ומשווים. השרת אף פעם לא מחזיר את ה-Hash ללקוח (`Password = ""` ב-`CreateModel`).

**ש: מה זה SQL Injection ואיך מנעת אותו?**
ת: הזרקת קוד SQL דרך שדה קלט. מנעתי אותו בכך שכל השאילתות משתמשות ב-`SqlParameter` ולא בחיבור מחרוזות.

**ש: מה ההבדל בין abstract class ל-interface?**
ת: interface (כמו `IGameService`) הוא רק חוזה, בלי קוד. abstract class (כמו `BaseDB<T>`) יכולה להכיל גם קוד מוכן (`SelectAll`, `Delete`) וגם חלקים שהיורשים חייבים לממש (`CreateModel`).

**ש: למה יש בדיקות קלט גם בלקוח וגם בשרת?**
ת: בלקוח, בשביל חוויית משתמש: הודעה מיידית ליד השדה. בשרת, בשביל אבטחה, כי אי אפשר לסמוך על מה שמגיע מהרשת.

**ש: מה עושה `async`/`await`?**
ת: מאפשר לחכות לתשובה מהשרת בלי לחסום את ממשק המשתמש.

**ש: איך התפריט יודע להשתנות כשמתחברים?**
ת: `Session` מפעיל אירוע `Changed` בכל התחברות או התנתקות. `MainPage` נרשם לאירוע ובונה את התפריט מחדש.

**ש: איך מונעים ממשתמש רגיל להיכנס לדף מנהל?**
ת: (1) הפריט לא מופיע לו בתפריט. (2) כל דף מנהל בודק `Session.IsAdmin` ב-`OnNavigatedTo` ומעביר לדף הבית אם הבדיקה נכשלת.

**ש: מה קורה כשמוחקים משתמש שקנה משחקים?**
ת: בזכות `ON DELETE CASCADE` במפתח הזר, הרכישות שלו נמחקות אוטומטית ע"י SQL Server.

---

## 9. איך להפוך את הפרויקט לשלך (מומלץ מאוד)

בדף המשימה כתוב *"העבודה חייבת להכיל קוד שכל תלמיד כתב בעצמו"*. כמה רעיונות להרחבות קטנות שתכתוב לבד, לפי הדוגמאות שכבר יש בקוד:

1. **מועדפים / רשימת משאלות**: טבלה `Wishlist(UserId, GameId)`, מחלקה `WishlistDB` (העתק את המבנה של `PurchaseDB`), פעולה בשירות וכפתור ב-`GameDetailsPage`.
2. **דירוג משחקים**: עמודת `Rating` או טבלת `Reviews`, והצגת ממוצע בדף המשחק.
3. **מיון בקטלוג**: ComboBox נוסף ב-`GamesPage` למיון לפי מחיר או שנה (רמז: `OrderBy` / `OrderByDescending` ב-`ApplyFilter`).
4. **ניהול ערים למנהל**: דף שמוסיף ערים (כמו "הוספת ז'אנר" ב-`AddGamePage`).
5. **שינוי עיצוב**: צבעים משלך ב-`App.xaml`.

אחרי שתבין את זרימת ה-Login מסעיף 1, כל אחת מההרחבות האלה היא חזרה על אותה תבנית: **טבלה, Model, DB, Service, Service Reference (Update), דף**.

> שים לב: אחרי כל שינוי בשירות (פעולה חדשה), צריך לעדכן את ה-Service Reference באפליקציה: לחיצה ימנית על `Connected Services > GameServiceReference` ואז **Update**.
