# GameZone: חנות משחקי מחשב

פרויקט בהנדסת תוכנה, חלק א'. אפליקציית UWP שמתחברת לשירות רשת (WCF), והשירות מתחבר למסד נתונים SQL Server.

> 📖 **הסבר מפורט על כל הקוד, איך הוא עובד ושאלות שהמורה עשוי לשאול: [docs/EXPLANATION.md](docs/EXPLANATION.md)**
> 📄 **תיק הפרויקט (Word): [docs/GameZone_Project_Portfolio.docx](docs/GameZone_Project_Portfolio.docx)**

## מבנה הפתרון (Solution)

```
GameStore.sln
├── Database/CreateDatabase.sql   ← יצירת הטבלאות ונתוני דוגמה
├── GameStore.Model/              ← מחלקות הישויות (User, Game, City, Genre, Purchase)
├── GameStore.ViewModel/          ← גישה למסד הנתונים: Connection + מחלקה לכל טבלה
├── GameStore.Service/            ← שירות הרשת WCF (IGameService + GameService)
└── GameStore.App/                ← אפליקציית UWP (כל הדפים)
```

```
 [ GameStore.App (UWP) ]  --HTTP/SOAP-->  [ GameStore.Service (WCF) ]  -->  [ ViewModel: Connection ]  -->  [ SQL Server ]
        View                                   Service                         Data Access                   Database
```

## כיסוי דרישות המשימה

| # | דרישה | איפה זה בפרויקט |
|---|-------|-----------------|
| 1 | מסד נתונים מלא | `Database/CreateDatabase.sql`: 5 טבלאות עם מפתחות זרים ונתוני דוגמה |
| 2 | שירות רשת עם מחלקת Connection | `GameStore.Service` + `GameStore.ViewModel/Connection.cs` |
| 3 | מחלקה לכל ישות | `GameStore.Model/*.cs` |
| 4 | דף שער ודפי תוכן | `Pages/HomePage`, `Pages/AboutPage` |
| 5 | טופס הרשמה עם ComboBox ובדיקות | `Pages/RegisterPage` + `Controls/UserForm` + `Helpers/Validator.cs` |
| 6 | טופס התחברות | `Pages/LoginPage` |
| 7 | דפים ראשיים למנהל ולמשתמש | `Pages/AdminHomePage`, `Pages/UserHomePage` |
| 8 | דפי הצגת מוצרים | `Pages/GamesPage`, `Pages/GameDetailsPage`, `Pages/MyGamesPage` |
| 9 | שינוי פרטים ע"י משתמש | `Pages/EditProfilePage` |
| 10 | שינוי פרטי משתמשים ע"י מנהל | `Pages/ManageUsersPage` |
| 11 | הוספת מוצרים ע"י מנהל | `Pages/AddGamePage` (הוספה, עריכה, ז'אנר חדש) + מחיקה ב-`GameDetailsPage` |
| ✓ | עיצוב אחיד | כל הסגנונות ב-`App.xaml`, ותפריט משותף ב-`MainPage` |
| 12 | קובץ Word | `docs/GameZone_Project_Portfolio.docx` |

## איך מריצים (Windows + Visual Studio 2022)

### 0. התקנות
ב-Visual Studio Installer צריך שיהיו מסומנים:
- **ASP.NET and web development** (כולל *Windows Communication Foundation* תחת Individual components)
- **Universal Windows Platform development**
- **Data storage and processing** (בשביל SQL Server LocalDB)

ב-Windows: Settings > Privacy & security > For developers > **Developer Mode = On**

### 1. מסד הנתונים
1. פותחים את `GameStore.sln`
2. View > **SQL Server Object Explorer**
3. `(localdb)\MSSQLLocalDB` > Databases > לחיצה ימנית > **Add New Database** > שם: `GameStoreDB`
4. לחיצה ימנית על `GameStoreDB` > **New Query**, מדביקים את התוכן של `Database/CreateDatabase.sql` ולוחצים Execute

### 2. הרצת השירות
1. לחיצה ימנית על **GameStore.Service** > Set as Startup Project
2. לוחצים על `GameService.svc` ואז F5. אמור להיפתח דפדפן עם כתובת `http://localhost:50505/GameService.svc`
3. משאירים את השירות רץ

### 3. חיבור האפליקציה לשירות (Service Reference), פעם אחת בלבד
1. לחיצה ימנית על **GameStore.App** > Add > **Service Reference...** (או Connected Service)
2. בוחרים **WCF Web Service**
3. URI: `http://localhost:50505/GameService.svc`, ואז לוחצים **Go**
4. Namespace: **`GameServiceReference`** (חשוב: בדיוק בשם הזה, כי הקוד משתמש בו)
5. Next, ואז Finish

Visual Studio ייצור את המחלקה `GameServiceClient` ואת כל המחלקות (User, Game...) בצד הלקוח.

### 4. הרצת הכול ביחד
1. לחיצה ימנית על ה-Solution > **Configure Startup Projects** > **Multiple startup projects**
2. GameStore.Service = Start, ‏GameStore.App = Start
3. בסרגל העליון בוחרים פלטפורמה **x64** (או x86) ולוחצים F5

### משתמשים לבדיקה
| שם משתמש | סיסמה | סוג |
|-----------|-------|-----|
| admin | Admin123 | מנהל |
| dani | Dani1234 | משתמש |
| noa | Noa12345 | משתמש |

## בעיות נפוצות
- **"לא ניתן להתחבר לשירות הרשת"**: השירות לא רץ. מריצים אותו (שלב 2) לפני האפליקציה.
- **שגיאות קומפילציה על `GameServiceReference`**: עוד לא הוספתם Service Reference (שלב 3), או שנתתם לו שם אחר.
- **שגיאה על מסד הנתונים**: ודאו שמסד הנתונים נקרא בדיוק `GameStoreDB`, ושהסקריפט רץ בלי שגיאות. מחרוזת החיבור נמצאת ב-`GameStore.Service/Web.config`.
- **פרויקט לא נטען ב-Visual Studio**: חסר workload (ראו שלב 0).
