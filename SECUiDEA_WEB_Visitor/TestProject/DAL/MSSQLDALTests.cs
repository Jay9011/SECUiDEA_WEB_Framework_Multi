using System.Data;
using CoreDAL.Configuration;
using CoreDAL.Configuration.Interface;
using CoreDAL.Configuration.Models;
using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;
using FileIOHelper;
using FileIOHelper.Helpers;

namespace TestProject.DAL;

public class MSSQLDALTests
{
    private readonly MsSqlConnectionInfo _connectionInfo;
    private const string TEST_SECTION = "TEST_SECUIDEA_MSSQL";
    
    public MSSQLDALTests()
    {
        _connectionInfo = new MsSqlConnectionInfo
        {
            Server = "localhost",
            Database = "TEST_S1ACCESS",
            UserId = "sa",
            Password = "s1access!",
            IntegratedSecurity = false
        };
    }

    public class GetEmployeesParameters : SQLParam
    {
        [DbParameter]
        public string Department { get; set; }

        [DbParameter(dbType:DbType.Int32, direction:ParameterDirection.Output)]
        public int test { get; set; }
    }

    public class InsertEmployeeParameters : SQLParam
    {
        [DbParameter] 
        public string PNAME { get; set; }

        [DbParameter] 
        public string PDEPARTMENT { get; set; }
        
        [DbParameter] 
        public decimal PSALARY { get; set; }
        
        [DbParameter("p_emp_id", direction:ParameterDirection.Output)] 
        public int EmployeeId { get; set; }
    }
    
    public class DeleteEmployeeParameters : SQLParam
    {
        [DbParameter("p_department")]
        public string DEPARTMENT { get; set; }
        
        [DbParameter(direction:ParameterDirection.Output)]
        public int DeletedCount { get; set; }
    }

    [Fact]
    public async Task TestMsSqlWithContainer()
    {
        // Arrange
        IIOHelper ioHelper = new RegistryHelper("Software\\SECUIDEA");
        DatabaseSetupContainer container = new DatabaseSetupContainer(new Dictionary<string, (DatabaseType dbType, IIOHelper ioHelper)>
        {
            { TEST_SECTION, (DatabaseType.MSSQL, ioHelper) }
        });
        
        container.UpdateSetup(TEST_SECTION, _connectionInfo);
        var setup = container.GetSetup(TEST_SECTION);
        
        // Act & Assert
        await TestMsSqlOperation(setup);
    }
    
    [Fact]
    public async Task TestMsSqlWithoutContainer()
    {
        // Arrange
        var setup = new DatabaseSetup(DatabaseType.MSSQL, new RegistryHelper("Software\\SECUIDEA"), TEST_SECTION);
        setup.UpdateConnectionInfo(_connectionInfo);
        
        // Act & Assert
        await TestMsSqlOperation(setup);
    }

    private async Task TestMsSqlOperation(IDatabaseSetup setup)
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
        Assert.Equal(10, getEmployeesParams.test); // Check output parameter
        
        // Test inserting employee
        var insertEmployeeParams = new InsertEmployeeParameters
        {
            PNAME = "Test Employee",
            PDEPARTMENT = "TEST",
            PSALARY = 1000.00M
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
        Assert.Equal(insertEmployeeParams.PSALARY, Convert.ToDecimal(insertedEmployee["SALARY"]));
        
        // Test deleting employee
        var deleteEmployeeParams = new DeleteEmployeeParameters { DEPARTMENT = insertEmployeeParams.PDEPARTMENT };
        SQLResult deleteEmployeeResult = await setup.DAL.ExecuteProcedureAsync(setup, "DELETE_EMPLOYEE_BY_DEPT", deleteEmployeeParams);
        
        Assert.True(deleteEmployeeResult.IsSuccess);
        Assert.True(deleteEmployeeParams.DeletedCount > 0);
        
        // verify deleted employee
        var getDeletedEmployeeParams = new GetEmployeesParameters { Department = insertEmployeeParams.PDEPARTMENT };
        SQLResult getDeletedEmployeeResult = await setup.DAL.ExecuteProcedureAsync(setup, "GET_EMPLOYEES_BY_DEPT", getDeletedEmployeeParams);
        
        Assert.True(getDeletedEmployeeResult.IsSuccess);
        Assert.True(getDeletedEmployeeResult.DataSet.Tables[0].Rows.Count == 0);
    }
}