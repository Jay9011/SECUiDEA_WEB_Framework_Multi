using CoreDAL.ORM;

namespace UserDAL.ParamClasses.S1Access
{
    public class WEB_ADMIN_LOGIN_SEL: SQLParam
    {
        public enum Types
        {
            /// <summary>
            /// ID를 가지고 사용자 정보를 찾는다.
            /// </summary>
            FindUser = 2,
        }
        
        public string ID { get; set; }
        public Types Type { get; set; }
        public string Auth { get; set; }
        public string OldPassword { get; set; }
        public string UserMac { get; set; }
        public int? UpdateID { get; set; }
        public string UserLanguage { get; set; }
        public int? UserLanguageNum { get; set; }
        public char? Ver { get; set; }
    }
}