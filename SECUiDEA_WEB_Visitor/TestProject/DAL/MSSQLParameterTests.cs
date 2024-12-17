using System.Data;
using CoreDAL.Configuration;
using CoreDAL.Configuration.Interface;
using CoreDAL.Configuration.Models;
using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;
using CoreDAL.ORM.Interfaces;

namespace TestProject.DAL
{
    public class MSSQLParameterTests
    {
        private readonly IDatabaseSetup _setup;
        private const string TEST_SECTION = "TEST_SECUIDEA_MSSQL";

        public MSSQLParameterTests()
        {
            var connectionInfo = new MsSqlConnectionInfo
            {
                Server = "localhost",
                Database = "TEST_S1ACCESS",
                UserId = "sa",
                Password = "s1access!",
                IntegratedSecurity = false
            };

            _setup = new DatabaseSetup(DatabaseType.MSSQL, 
                new FileIOHelper.Helpers.RegistryHelper("Software\\SECUIDEA"), 
                TEST_SECTION);
            _setup.UpdateConnectionInfo(connectionInfo);
        }

        // 프로시저보다 적은 파라미터를 가진 클래스
        public class LessParametersTest : SQLParam
        {
            [DbParameter]
            public string Required1 { get; set; }

            [DbParameter]
            public int Required2 { get; set; }

            [DbParameter(dbType: DbType.Int32, direction: ParameterDirection.Output)]
            public int OutputValue { get; set; }
            
            // Optional 파라미터는 생략
        }

        // 프로시저보다 많은 파라미터를 가진 클래스
        public class MoreParametersTest : SQLParam
        {
            [DbParameter]
            public string Param1 { get; set; }

            [DbParameter]
            public int Param2 { get; set; }

            [DbParameter(dbType: DbType.Int32, direction: ParameterDirection.Output)]
            public int OutputValue { get; set; }

            [DbParameter]
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
            Assert.Equal("Test", row["Col1"]);
            Assert.Equal(42, Convert.ToInt32(row["Col2"]));
            Assert.Equal(0M, Convert.ToDecimal(row["Col3"])); // Optional parameter default value
            Assert.Equal(42, parameters.OutputValue); // Output parameter value
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
            Assert.Equal("Test", row["Col1"]);
            Assert.Equal(42, Convert.ToInt32(row["Col2"]));
            Assert.Equal(42, parameters.OutputValue);
        }

        [Fact]
        public async Task TestLessParametersWithDictionary()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["Required1"] = "Test",
                ["Required2"] = 42
                // Optional 파라미터 생략
            };

            // Act
            var result = await _setup.DAL.ExecuteProcedureAsync(_setup, "TEST_LESS_PARAMETERS", parameters);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.DataSet);
            Assert.Single(result.DataSet.Tables);
            Assert.Single(result.DataSet.Tables[0].Rows);

            var row = result.DataSet.Tables[0].Rows[0];
            Assert.Equal("Test", row["Col1"]);
            Assert.Equal(42, Convert.ToInt32(row["Col2"]));
            Assert.Equal(0M, Convert.ToDecimal(row["Col3"])); // Optional parameter default value
            Assert.Equal(12, result.ReturnValue); // RETURN 값
        }

        [Fact]
        public async Task TestMoreParametersWithDictionary()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["Param1"] = "Test",
                ["Param2"] = 42,
                ["ExtraParam"] = "Ignored", // 프로시저에 없는 추가 파라미터
                ["OutputValue"] = 0
            };

            // Act
            var result = await _setup.DAL.ExecuteProcedureAsync(_setup, "TEST_MORE_PARAMETERS", parameters);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.DataSet);
            Assert.Single(result.DataSet.Tables);
            Assert.Single(result.DataSet.Tables[0].Rows);

            var row = result.DataSet.Tables[0].Rows[0];
            Assert.Equal("Test", row["Col1"]);
            Assert.Equal(42, Convert.ToInt32(row["Col2"]));
            Assert.Equal(50, result.ReturnValue);
        }

        [Fact]
        public async Task TestParameterCaseSensitivity()
        {
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                ["REQUIRED1"] = "Test",  // 대문자로 파라미터명 지정
                ["required2"] = 42       // 소문자로 파라미터명 지정
            };

            // Act
            var result = await _setup.DAL.ExecuteProcedureAsync(_setup, "TEST_LESS_PARAMETERS", parameters);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.DataSet);
            var row = result.DataSet.Tables[0].Rows[0];
            Assert.Equal("Test", row["Col1"]);
            Assert.Equal(42, Convert.ToInt32(row["Col2"]));
        }
    }
}