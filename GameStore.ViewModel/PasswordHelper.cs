using System.Security.Cryptography;
using System.Text;

namespace GameStore.ViewModel
{
    /// <summary>
    /// הצפנה חד-כיוונית (Hash) של סיסמאות.
    /// SHA-256 הופך כל סיסמה למחרוזת של 64 תווים. אי אפשר לחזור מה-Hash לסיסמה,
    /// אבל אותה סיסמה תמיד נותנת אותו Hash - כך אפשר לבדוק התחברות בלי לשמור את הסיסמה עצמה.
    /// אם מישהו יגנוב את מסד הנתונים, הוא לא יראה את הסיסמאות.
    /// </summary>
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));   // כל בייט -> 2 תווים הקסדצימליים
                return sb.ToString();
            }
        }
    }
}
