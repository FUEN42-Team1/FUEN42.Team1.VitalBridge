namespace MvcDemo.Models.Utilities
{
    using BCrypt.Net;
    public class HashUtility
    {
        /// <summary>
        /// 使用 BCrypt 對密碼進行雜湊
        /// </summary>
        /// <param name="password">要雜湊的密碼</param>
        /// <returns>雜湊後的密碼</returns>
        public static string HashPassword(string password)
        {
            // 建議使用 EnhancedHashPassword 以獲得更好的預設安全性
            // workFactor 預設值為 12，數字越高越安全但運算時間越長
            return BCrypt.EnhancedHashPassword(password, workFactor: 12);
            
        }

        /// <summary>
        /// 驗證密碼是否正確
        /// </summary>
        /// <param name="password">原始密碼</param>
        /// <param name="hashedPassword">雜湊後的密碼</param>
        /// <returns>密碼是否正確</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // 使用 EnhancedVerify 來驗證密碼
            return BCrypt.EnhancedVerify(password, hashedPassword);
        }
    }
}
