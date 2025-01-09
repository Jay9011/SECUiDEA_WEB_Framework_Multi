using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;

namespace UserDAL.ParamClasses.SECU_WEB
{
    public class SECU_WEB_LOGIN_SEL: SQLParam
    {
        public enum Types
        {
            /// <summary>
            /// ID를 가지고 사용자 정보를 찾는다.
            /// </summary>
            GetUser = 2,
        }
        
        [DbParameter]
        public Types Type { get; set; }
        [DbParameter]
        public string ID { get; set; }
        [DbParameter]
        public string OldPassword { get; set; }
        [DbParameter]
        public int? UpdateID { get; set; }
    }
}