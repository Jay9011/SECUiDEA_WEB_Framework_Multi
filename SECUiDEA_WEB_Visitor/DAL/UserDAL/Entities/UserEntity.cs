using System;
using Newtonsoft.Json;
using UserDALInterface.Entities;

namespace UserDAL.Entities
{
    /// <summary>
    /// 사용자 정보 엔티티 (DB와 직접 연결되는 부분)
    /// </summary>
    public class UserEntity: IUserEntity
    {
        [JsonProperty("UserID")]
        public string Id { get; set; }

        [JsonProperty("UserPassword")]
        public string Password { get; set; }
        
        public DateTime LastLogin { get; set; }
        
        /// <summary>
        /// 계정 타입
        /// </summary>
        public string AuthType { get; set; }
        /// <summary>
        /// Unique ID
        /// </summary>
        public string Seq { get; set; }
        
        /// <summary>
        /// 사용자 이름
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 사용자 번호
        /// </summary>
        public string Tel { get; set; }
        /// <summary>
        /// Person 조직 ID
        /// </summary>
        public int? OrgID { get; set; }
        /// <summary>
        /// Person 조직명
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// Person 직급 ID
        /// </summary>
        public int? GradeID { get; set; }
        /// <summary>
        /// Person 상태 ID
        /// </summary>
        public int? PersonStatudID { get; set; }
        public int? EqUserLevelID { get; set; }
        public int? VisitSabunPW { get; set; }
        public int? AccessAuthority { get; set; }
        public int? ReservedWord { get; set; }
        public int? DeleteFlag { get; set; }
        public string LoginIP { get; set; }
        public DateTime? InsertDate { get; set; }
    }
}