using System.Data;
using CoreDAL.Configuration;
using CoreDAL.Configuration.Interface;
using CoreDAL.Configuration.Models;
using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;
using CoreDAL.ORM.Interfaces;

namespace TestProject.DAL
{
    public class OracleParameterTests
    {
        private readonly IDatabaseSetup _setup;
        private const string TEST_SECTION = "TEST_SECUIDEA_ORACLE";

        public OracleParameterTests()
        {
            var connectionInfo = new OracleConnectionInfo
            {
                Host = "localhost",
                Port = 1521,
                ServiceName = "FREE",
                UserId = "TEST_SECUIDEA",
                Password = "ca1208!"
            };

            _setup = new DatabaseSetup(DatabaseType.ORACLE,
                new FileIOHelper.Helpers.RegistryHelper("Software\\SECUIDEA"),
                TEST_SECTION);
            _setup.UpdateConnectionInfo(connectionInfo);
        }

        // 프로시저보다 적은 파라미터를 가진 클래스
        public class LessParametersTest : SQLParam
        {
            [DbParameter("p_Required1")]
            public string Required1 { get; set; }

            [DbParameter("p_Required2")]
            public int Required2 { get; set; }
            
            [DbParameter("p_Optional")] // p_Optional 파라미터는 생략
            public int Optional { get; set; }

            [DbParameter("p_ResultCursor", DbType.Object, ParameterDirection.Output)]
            public object ResultCursor { get; set; }
        }

        // 프로시저보다 많은 파라미터를 가진 클래스
        public class MoreParametersTest : SQLParam
        {
            [DbParameter("p_Param1")]
            public string Param1 { get; set; }

            [DbParameter("p_Param2")]
            public int Param2 { get; set; }

            [DbParameter("p_ResultCursor", DbType.Object, ParameterDirection.Output)]
            public object ResultCursor { get; set; }

            [DbParameter("p_ExtraParam")]
            public string ExtraParam { get; set; } // 프로시저에 없는 추가 파라미터
        }

        [Fact]
        public async Task TestLessParametersWithSQLParam()
        {
            // Arrange
            var parameters = new LessParametersTest
            {
                Required1 = "Test",
                Required2 = 42
            };

            // Act
            var result = await _setup.DAL.ExecuteProcedureAsync(_setup, "TEST_LESS_PARAMETERS", parameters);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.DataSet);
            Assert.Single(result.DataSet.Tables);
            Assert.Single(result.DataSet.Tables[0].Rows);

            var row = result.DataSet.Tables[0].Rows[0];
            Assert.Equal("Test", row["COL1"]); // Oracle은 대문자로 컬럼명을 반환
            Assert.Equal(42, Convert.ToInt32(row["COL2"]));
            Assert.Equal(0, Convert.ToInt32(row["COL3"])); // Optional parameter default value
        }

        [Fact]
        public async Task TestMoreParametersWithSQLParam()
        {
            // Arrange
            var parameters = new MoreParametersTest
            {
                Param1 = "Test",
                Param2 = 42,
                ExtraParam = "Ignored" // 이 파라미터는 프로시저에서 무시됨
            };

            // Act
            var result = await _setup.DAL.ExecuteProcedureAsync(_setup, "TEST_MORE_PARAMETERS", parameters);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.DataSet);
            Assert.Single(result.DataSet.Tables);
            Assert.Single(result.DataSet.Tables[0].Rows);

            var row = result.DataSet.Tables[0].Rows[0];
            Assert.Equal("Test", row["COL1"]);
            Assert.Equal(42, Convert.ToInt32(row["COL2"]));
        }

        [Fact]
        public async Task TestLessParametersWithDictionary()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["p_Required1"] = "Test",
                ["p_Required2"] = 42,
                ["p_ResultCursor"] = null  // Oracle의 경우 REF CURSOR를 위한 파라미터 필요
            };

            // Act
            var result = await _setup.DAL.ExecuteProcedureAsync(_setup, "TEST_LESS_PARAMETERS", parameters);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.DataSet);
            Assert.Single(result.DataSet.Tables);
            Assert.Single(result.DataSet.Tables[0].Rows);

            var row = result.DataSet.Tables[0].Rows[0];
            Assert.Equal("Test", row["COL1"]);
            Assert.Equal(42, Convert.ToInt32(row["COL2"]));
            Assert.Equal(0, Convert.ToInt32(row["COL3"])); // Optional parameter default value
        }

        [Fact]
        public async Task TestMoreParametersWithDictionary()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["p_Param1"] = "Test",
                ["p_Param2"] = 42,
                ["p_ExtraParam"] = "Ignored",
                ["p_ResultCursor"] = null,
                ["p_OutputValue"] = 0
            };

            // Act
            var result = await _setup.DAL.ExecuteProcedureAsync(_setup, "TEST_MORE_PARAMETERS", parameters);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.DataSet);
            Assert.Single(result.DataSet.Tables);
            Assert.Single(result.DataSet.Tables[0].Rows);

            var row = result.DataSet.Tables[0].Rows[0];
            Assert.Equal("Test", row["COL1"]);
            Assert.Equal(42, Convert.ToInt32(row["COL2"]));
        }

        [Fact]
        public async Task TestParameterCaseSensitivity()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["P_REQUIRED1"] = "Test",  // 대문자로 파라미터명 지정
                ["p_required2"] = 42,      // 소문자로 파라미터명 지정
                ["p_ResultCursor"] = null  // REF CURSOR 파라미터
            };

            // Act
            var result = await _setup.DAL.ExecuteProcedureAsync(_setup, "TEST_LESS_PARAMETERS", parameters);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.DataSet);
            var row = result.DataSet.Tables[0].Rows[0];
            Assert.Equal("Test", row["COL1"]);
            Assert.Equal(42, Convert.ToInt32(row["COL2"]));
        }
    }
}