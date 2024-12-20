using System.Data;
using CoreDAL.Configuration;
using CoreDAL.Configuration.Interface;
using CoreDAL.Configuration.Models;
using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;
using FileIOHelper;
using FileIOHelper.Helpers;

namespace TestProject.DAL;

public class OracleDALTests
{
    private readonly OracleConnectionInfo _connectionInfo;
    private const string TEST_SECTION = "TEST_SECUIDEA";
    
    public OracleDALTests()
    {
        _connectionInfo = new OracleConnectionInfo
        {
            Host = "localhost",
            Port = 1521,
            ServiceName = "FREE",
            UserId = "TEST_SECUIDEA",
            Password = "ca1208!"
        };
    }

    public class GetEmployeesParameters : SQLParam
    {
        [DbParameter("p_department")]
        public string Department { get; set; }

        [DbParameter("p_result1")]
        public object result1 { get; set; }
        
        [DbParameter("p_result2")]
        public object result2 { get; set; }
        
        [DbParameter("p_result3")]
        public object result3 { get; set; }

        [DbParameter("p_test")]
        public int test { get; set; }
    }

    public class InsertEmployeeParameters : SQLParam
    {
        [DbParameter] // 프로퍼티명 - 파라미터명 테스트
        public string PNAME { get; set; }

        [DbParameter] // 프로퍼티명 - 파라미터명 테스트
        public string PDEPARTMENT { get; set; }
        
        [DbParameter] // 프로퍼티명 - 파라미터명 테스트
        public string PSALARY { get; set; }
        
        [DbParameter("p_emp_id")] 
        public int EmployeeId { get; set; }
    }
    
    public class DeleteEmployeeParameters : SQLParam
    {
        [DbParameter("p_department")]
        public string DEPARTMENT { get; set; }
        
        [DbParameter] // 프로퍼티명 - 파라미터명 테스트
        public int DeletedCount { get; set; }
    }

    [Fact]
    public async Task TestOracleWithContainer()
    {
        // Arrange
        IIOHelper ioHelper = new RegistryHelper("Software\\SECUIDEA");
        DatabaseSetupContainer container = new DatabaseSetupContainer(new Dictionary<string, (DatabaseType dbType, IIOHelper ioHelper)>
        {
            { TEST_SECTION, (DatabaseType.ORACLE, ioHelper) }
        });
        
        container.UpdateSetup(TEST_SECTION, _connectionInfo);
        var setup = container.GetSetup(TEST_SECTION);
        
        // Act & Assert
        await TestOracleOperation(setup);
    }
    
    [Fact]
    public async Task TestOracleWithoutContainer()
    {
        // Arrange
        var setup = new DatabaseSetup(DatabaseType.ORACLE, new RegistryHelper("Software\\SECUIDEA"), TEST_SECTION);
        setup.UpdateConnectionInfo(_connectionInfo);
        
        // Act & Assert
        await TestOracleOperation(setup);
    }

    private async Task TestOracleOperation(IDatabaseSetup setup)
    {
        // Test connection
        SQLResult connectionResult = await setup.DAL.TestConnectionAsync(setup);
        Assert.True(connectionResult.IsSuccess, $"Connection failed: {connectionResult.Message}");
        
        // Test getting employees by department
        var getEmployeesParams = new GetEmployeesParameters { Department = "IT" };
        SQLResult getEmployeesResult = await setup.DAL.ExecuteProcedureAsync(setup, "GET_EMPLOYEES_BY_DEPT", getEmployeesParams);
        
        Assert.True(getEmployeesResult.IsSuccess);
        Assert.NotNull(getEmployeesResult.DataSet);
        Assert.True(getEmployeesResult.DataSet.Tables.Count > 0);
        Assert.True(getEmployeesResult.DataSet.Tables[0].Rows.Count > 0);
        
        // Test inserting employee
        var insertEmployeeParams = new InsertEmployeeParameters
        {
            PNAME = "Test Employee",
            PDEPARTMENT = "TEST",
            PSALARY = "1000"
        };
        
        SQLResult insertEmployeeResult = await setup.DAL.ExecuteProcedureAsync(setup, "INSERT_EMPLOYEE", insertEmployeeParams);
        
        Assert.True(insertEmployeeResult.IsSuccess);
        Assert.True(insertEmployeeParams.EmployeeId > 0);
        
        // verify inserted employee
        var getInsertedEmployeeParams = new GetEmployeesParameters { Department = "TEST" };
        SQLResult getInsertedEmployeeResult = await setup.DAL.ExecuteProcedureAsync(setup, "GET_EMPLOYEES_BY_DEPT", getInsertedEmployeeParams);
        
        Assert.True(getInsertedEmployeeResult.IsSuccess);
        Assert.NotNull(getInsertedEmployeeResult.DataSet);
        Assert.True(getInsertedEmployeeResult.DataSet.Tables.Count > 0);
        Assert.True(getInsertedEmployeeResult.DataSet.Tables[0].Rows.Count > 0);
        
        DataRow insertedEmployee = getInsertedEmployeeResult.DataSet.Tables[0].Rows[0];
        Assert.Equal(insertEmployeeParams.PNAME, insertedEmployee["NAME"].ToString());
        Assert.Equal(insertEmployeeParams.PDEPARTMENT, insertedEmployee["DEPARTMENT"].ToString());
        Assert.Equal(insertEmployeeParams.PSALARY, insertedEmployee["SALARY"].ToString());
        
        // Test deleting employee
        var deleteEmployeeParams = new DeleteEmployeeParameters { DEPARTMENT = insertEmployeeParams.PDEPARTMENT };
        SQLResult deleteEmployeeResult = await setup.DAL.ExecuteProcedureAsync(setup, "DELETE_EMPLOYEE_BY_DEPT", deleteEmployeeParams);
        
        Assert.True(deleteEmployeeResult.IsSuccess);
        Assert.True(deleteEmployeeParams.DeletedCount > 0);
        
        // verify deleted employee
        var getDeletedEmployeeParams = new GetEmployeesParameters { Department = insertEmployeeParams.PDEPARTMENT };
        SQLResult getDeletedEmployeeResult = await setup.DAL.ExecuteProcedureAsync(setup, "GET_EMPLOYEES_BY_DEPT", getDeletedEmployeeParams);
        
        Assert.True(getDeletedEmployeeResult.IsSuccess);
        Assert.False(getDeletedEmployeeResult.DataSet.Tables[0].Rows.Count > 0);
    }
}