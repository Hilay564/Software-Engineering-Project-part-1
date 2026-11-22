using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using GameStore.Model;

namespace GameStore.ViewModel
{
    /// <summary>
    /// מחלקת בסיס אבסטרקטית וגנרית לכל מחלקות הטבלאות (UserDB, GameDB ...).
    ///
    /// גנרית (T) = אותו קוד עובד לכל סוג ישות. UserDB הוא BaseDB&lt;User&gt;,
    ///              GameDB הוא BaseDB&lt;Game&gt; וכו'.
    /// אבסטרקטית  = אי אפשר ליצור ממנה אובייקט ישירות. כל מחלקה שיורשת
    ///              חייבת לממש את החלקים שמסומנים abstract.
    ///
    /// כך הקוד החוזר (פתיחת חיבור, הרצת שאילתה, קריאת שורות) כתוב פעם אחת בלבד.
    /// </summary>
    public abstract class BaseDB<T> where T : BaseEntity
    {
        /// <summary>שם הטבלה במסד הנתונים (למשל "Users")</summary>
        protected abstract string TableName { get; }

        /// <summary>
        /// שאילתת ה-SELECT הבסיסית. מוסכמה: הטבלה הראשית תמיד מקבלת את הכינוי t,
        /// כדי שנוכל להוסיף WHERE t.Id = ... גם כשיש JOIN.
        /// </summary>
        protected abstract string SelectSql { get; }

        /// <summary>לפי מה למיין ב-SelectAll</summary>
        protected virtual string OrderBy => "t.Id";

        /// <summary>
        /// הופך שורה אחת מהתוצאה (reader) לאובייקט C#.
        /// כל טבלה יודעת אילו עמודות יש לה, לכן כל מחלקה מממשת את זה בעצמה.
        /// </summary>
        protected abstract T CreateModel(SqlDataReader reader);

        // =================== פעולות כלליות שעובדות לכל טבלה ===================

        public List<T> SelectAll()
        {
            return Select(SelectSql + " ORDER BY " + OrderBy);
        }

        public T SelectById(int id)
        {
            // FirstOrDefault = האיבר הראשון, או null אם הרשימה ריקה
            return Select(SelectSql + " WHERE t.Id = @Id", P("@Id", id)).FirstOrDefault();
        }

        public bool Delete(int id)
        {
            return Execute("DELETE FROM " + TableName + " WHERE Id = @Id", P("@Id", id)) > 0;
        }

        public int Count()
        {
            return Convert.ToInt32(Scalar("SELECT COUNT(*) FROM " + TableName));
        }

        // =================== כלי עזר למחלקות היורשות ===================

        /// <summary>מריץ שאילתת SELECT ומחזיר רשימת אובייקטים</summary>
        protected List<T> Select(string sql, params SqlParameter[] parameters)
        {
            List<T> list = new List<T>();

            // using סוגר את החיבור אוטומטית בסוף, גם אם נזרקה שגיאה
            using (SqlConnection connection = Connection.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())           // כל עוד יש עוד שורה
                        list.Add(CreateModel(reader));
                }
            }
            return list;
        }

        /// <summary>מריץ INSERT / UPDATE / DELETE ומחזיר כמה שורות הושפעו</summary>
        protected int Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = Connection.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>מריץ שאילתה שמחזירה ערך בודד (COUNT, SUM, ה-Id החדש...)</summary>
        protected object Scalar(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = Connection.Open())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// יוצר פרמטר לשאילתה. חשוב מאוד: לעולם לא מחברים קלט של משתמש ישירות
        /// למחרוזת ה-SQL - זה פותח פרצת אבטחה שנקראת SQL Injection.
        /// עם פרמטרים, SQL Server מתייחס לערך כנתון בלבד ולא כקוד.
        /// </summary>
        protected static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
}
