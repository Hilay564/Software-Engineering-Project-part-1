/* =====================================================================
   GameZone - סקריפט יצירת מסד הנתונים
   ---------------------------------------------------------------------
   איך מריצים:
   1. ב-Visual Studio: View > SQL Server Object Explorer
   2. פותחים את (localdb)\MSSQLLocalDB > לחיצה ימנית על Databases > Add New Database
      ונותנים לו את השם GameStoreDB
   3. לחיצה ימנית על GameStoreDB > New Query > מדביקים את כל הקובץ > Execute (Ctrl+Shift+E)

   הסקריפט מוחק טבלאות קיימות ויוצר אותן מחדש עם נתוני דוגמה.
   ===================================================================== */

-- מוחקים קודם את הטבלאות שתלויות באחרות (בגלל המפתחות הזרים)
IF OBJECT_ID('dbo.Purchases', 'U') IS NOT NULL DROP TABLE dbo.Purchases;
IF OBJECT_ID('dbo.Games',     'U') IS NOT NULL DROP TABLE dbo.Games;
IF OBJECT_ID('dbo.Users',     'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Genres',    'U') IS NOT NULL DROP TABLE dbo.Genres;
IF OBJECT_ID('dbo.Cities',    'U') IS NOT NULL DROP TABLE dbo.Cities;
GO

/* ---------- טבלת ערים (ממלאת את ה-ComboBox בטופס ההרשמה) ---------- */
CREATE TABLE dbo.Cities
(
    Id    INT IDENTITY(1,1) PRIMARY KEY,      -- IDENTITY = המספר נוצר אוטומטית
    Name  NVARCHAR(50) NOT NULL UNIQUE         -- NVARCHAR תומך בעברית
);

/* ---------- טבלת ז'אנרים (קטגוריות של משחקים) ---------- */
CREATE TABLE dbo.Genres
(
    Id    INT IDENTITY(1,1) PRIMARY KEY,
    Name  NVARCHAR(50) NOT NULL UNIQUE
);

/* ---------- טבלת משתמשים ---------- */
CREATE TABLE dbo.Users
(
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(30)  NOT NULL UNIQUE,     -- אי אפשר שני משתמשים עם אותו שם
    PasswordHash  NVARCHAR(64)  NOT NULL,            -- לא שומרים סיסמה! רק Hash (SHA-256)
    FirstName     NVARCHAR(30)  NOT NULL,
    LastName      NVARCHAR(30)  NOT NULL,
    Email         NVARCHAR(100) NOT NULL,
    Phone         NVARCHAR(15)  NOT NULL,
    BirthDate     DATE          NOT NULL,
    Gender        NVARCHAR(10)  NOT NULL,
    CityId        INT           NOT NULL REFERENCES dbo.Cities(Id),  -- מפתח זר
    IsAdmin       BIT           NOT NULL DEFAULT 0,                    -- 1 = מנהל
    JoinDate      DATETIME      NOT NULL DEFAULT GETDATE()
);

/* ---------- טבלת משחקים (המוצרים של החנות) ---------- */
CREATE TABLE dbo.Games
(
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Title        NVARCHAR(100)  NOT NULL,
    Description  NVARCHAR(1000) NOT NULL,
    Developer    NVARCHAR(100)  NOT NULL,
    Price        DECIMAL(8,2)   NOT NULL CHECK (Price >= 0),
    ReleaseYear  INT            NOT NULL,
    MinAge       INT            NOT NULL DEFAULT 3,
    GenreId      INT            NOT NULL REFERENCES dbo.Genres(Id),
    ImageUrl     NVARCHAR(300)  NOT NULL DEFAULT ''
);

/* ---------- טבלת רכישות (קשר רבים-לרבים בין משתמשים למשחקים) ---------- */
CREATE TABLE dbo.Purchases
(
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT          NOT NULL REFERENCES dbo.Users(Id) ON DELETE CASCADE,  -- מחיקת משתמש מוחקת את הרכישות שלו
    GameId        INT          NOT NULL REFERENCES dbo.Games(Id) ON DELETE CASCADE,
    PurchaseDate  DATETIME     NOT NULL DEFAULT GETDATE(),
    PricePaid     DECIMAL(8,2) NOT NULL,
    CONSTRAINT UQ_Purchase UNIQUE (UserId, GameId)    -- אי אפשר לקנות אותו משחק פעמיים
);
GO

/* =========================== נתוני דוגמה =========================== */

INSERT INTO dbo.Cities (Name) VALUES
(N'ירושלים'), (N'תל אביב'), (N'חיפה'), (N'באר שבע'), (N'ראשון לציון'),
(N'פתח תקווה'), (N'אשדוד'), (N'נתניה'), (N'חולון'), (N'רמת גן'),
(N'הרצליה'), (N'כפר סבא'), (N'רעננה'), (N'מודיעין'), (N'אילת');

INSERT INTO dbo.Genres (Name) VALUES
(N'אקשן'), (N'הרפתקאות'), (N'אסטרטגיה'), (N'ספורט'), (N'מרוצים'), (N'פאזל'), (N'RPG');

-- סיסמאות הדוגמה (מה שמקלידים במסך ההתחברות):
--   admin / Admin123   (מנהל)
--   dani  / Dani1234   (משתמש רגיל)
--   noa   / Noa12345   (משתמש רגיל)
INSERT INTO dbo.Users (Username, PasswordHash, FirstName, LastName, Email, Phone, BirthDate, Gender, CityId, IsAdmin) VALUES
(N'admin', N'3b612c75a7b5048a435fb6ec81e52ff92d6d795a8b5a9c17070f6a63c97a53b2', N'מנהל', N'המערכת', N'admin@gamezone.co.il', N'050-0000000', '1990-01-01', N'זכר',  1, 1),
(N'dani',  N'9c1d433f62bb06fcdb5d3d3730a836100ba9aac7dee5aca0fc7ce75a5321b3c9', N'דני',  N'כהן',    N'dani@mail.com',        N'052-1234567', '2008-05-14', N'זכר',  2, 0),
(N'noa',   N'6d1f593f91dc796446fd52ecea4a78a71199fef81f034c1bb623f8d35f7b96ef', N'נועה', N'לוי',    N'noa@mail.com',         N'054-7654321', '2007-11-02', N'נקבה', 3, 0);

-- משחקים בדיוניים (לא משחקים אמיתיים). ImageUrl ריק = האפליקציה מציגה תמונת ברירת מחדל
INSERT INTO dbo.Games (Title, Description, Developer, Price, ReleaseYear, MinAge, GenreId, ImageUrl) VALUES
(N'Sky Raiders',      N'טייסים נלחמים בשמי עיר עתידנית. משחק אקשן מהיר עם 30 שלבים ומצב שני שחקנים.',           N'Blue Comet Studio', 89.90, 2024, 12, 1, N''),
(N'Lost Temple',      N'חוקרת צעירה מחפשת מקדש אבוד בג''ונגל. חידות, מלכודות וסיפור מרתק.',                       N'Jungle Bytes',      119.00, 2023, 7,  2, N''),
(N'Kingdom Builder',  N'בנה ממלכה מאפס: נהל משאבים, בנה ערים והגן על הגבולות מפני פולשים.',                       N'Crown Games',       149.90, 2025, 12, 3, N''),
(N'Street Football',  N'כדורגל רחוב 3 על 3 עם קבוצות מכל העולם וטורנירים אונליין.',                               N'Kickoff Labs',      59.90,  2024, 3,  4, N''),
(N'Neon Drift',       N'מרוצי לילה ברחובות מוארים בניאון. שדרג את המכונית וזכה באליפות.',                          N'Turbo Pixel',       99.00,  2022, 7,  5, N''),
(N'Block Logic',      N'מאות חידות היגיון עם קוביות צבעוניות. מתאים לכל המשפחה.',                                 N'Brainy Box',        29.90,  2021, 3,  6, N''),
(N'Dragon Oath',      N'משחק תפקידים בעולם פנטזיה: בחר גיבור, השג ציוד ויצא למסעות.',                              N'Ember Forge',       179.00, 2025, 16, 7, N''),
(N'Space Colony',     N'הקם מושבה על כוכב לכת רחוק ודאג שהמתיישבים ישרדו.',                                       N'Orbit Works',       129.90, 2023, 12, 3, N'');

INSERT INTO dbo.Purchases (UserId, GameId, PricePaid) VALUES
(2, 1, 89.90), (2, 6, 29.90), (3, 2, 119.00);
GO
