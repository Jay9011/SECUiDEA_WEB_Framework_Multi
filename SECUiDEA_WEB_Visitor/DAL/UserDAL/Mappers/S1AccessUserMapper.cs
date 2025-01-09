using System;
using System.Data;
using System.Threading.Tasks;
using CoreDAL.Configuration.Interface;
using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;
using CoreDAL.ORM.Interfaces;
using UserDAL.Entities;
using UserDALInterface.Entities;
using UserDALInterface.Mappers;

namespace UserDAL.Mappers
{
    public class S1AccessUserMapper : IUserMapper
    {
        #region 의존 주입

        private readonly IDatabaseSetup _db;

        #endregion
        
        public S1AccessUserMapper(IDatabaseSetup databaseSetup)
        {
            #region 의존 주입

            _db = databaseSetup;

            #endregion
        }
        
        public async Task<IUserEntity> GetUserByIdAsync(ISQLParam param = null)
        {
            try
            {
                // 프로시저 실행
                SQLResult result = await _db.DAL.ExecuteProcedureAsync(_db, "SECU_WEB_LOGIN_SEL", param);
                
                // 실패했거나 결과가 없으면 null 반환
                if (!result.IsSuccess || result.DataSet == null || result.DataSet.Tables.Count == 0)
                {
                    return null;
                }
                
                // 결과를 객체로 변환
                if (result.DataSet.Tables[0].Rows.Count > 0)
                {
                    DataRow row = result.DataSet.Tables[0].Rows[0];
                    // AuthType이 존재하고 AuthType이 1 이상이어야 정상적으로 가져온 것
                    if (row["AuthType"] == DBNull.Value || Convert.ToInt32(row["AuthType"]) < 1)
                    {
                        return null;
                    }
                    
                    // DataRow를 UserEntity로 변환
                    return row.ToObject<UserEntity>();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return null;
        }
    }
}