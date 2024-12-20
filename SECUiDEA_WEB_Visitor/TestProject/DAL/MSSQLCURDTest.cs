using System.Data;
using CoreDAL.Configuration;
using CoreDAL.Configuration.Interface;
using CoreDAL.Configuration.Models;
using CoreDAL.ORM;
using CoreDAL.ORM.Extensions;
using FileIOHelper.Helpers;

namespace TestProject.DAL;

public class MSSQLCURDTest
{
    private readonly MsSqlConnectionInfo _connectionInfo;
    private const string TEST_SECTION = "TEST_COMPLETE_MSSQL";

    public MSSQLCURDTest()
    {
        _connectionInfo = new MsSqlConnectionInfo
        {
            Server = "localhost",
            Database = "TEST_S1ACCESS",
            UserId = "sa",
            Password = "s1access!"
        };
    }

    #region Parameter Classes

    public class EmployeeParameters : SQLParam
    {
        [DbParameter] 
        public string Department { get; set; }

        [DbParameter]
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

        [DbParameter("p_emp_id")]
        public int EmployeeId { get; set; }
    }

    public class UpdateEmployeeParameters : SQLParam
    {
        [DbParameter]
        public int Id { get; set; }

        [DbParameter]
        public decimal NewSalary { get; set; }

        [DbParameter]
        public string NewDepartment { get; set; }
    }

    public class DeleteEmployeeParameters : SQLParam
    {
        [DbParameter("p_department")] 
        public string Department { get; set; }

        [DbParameter()]
        public int DeletedCount { get; set; }
    }
    
    public class ORMTest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public int Number { get; set; }
    }

    #endregion

    [Fact]
    public async Task TestORMAsyncMethod()
    {
        var setup = CreateDatabaseSetup();
        await ExecuteTestScenario(setup, true, true);
    }

    [Fact]
    public async Task TestDictionaryAsyncMethod()
    {
        var setup = CreateDatabaseSetup();
        await ExecuteTestScenario(setup, false, true);
    }

    [Fact]
    public async Task TestORMSyncMethod()
    {
        var setup = CreateDatabaseSetup();
        await ExecuteTestScenario(setup, true, false);
    }

    [Fact]
    public async Task TestDictionarySyncMethod()
    {
        var setup = CreateDatabaseSetup();
        await ExecuteTestScenario(setup, false, false);
    }

    private IDatabaseSetup CreateDatabaseSetup()
    {
        var setup = new DatabaseSetup(DatabaseType.MSSQL,
            new RegistryHelper("Software\\SECUIDEA"),
            TEST_SECTION);
        setup.UpdateConnectionInfo(_connectionInfo);
        return setup;
    }

    private async Task ExecuteTestScenario(IDatabaseSetup setup, bool useORM, bool useAsync)
    {
        // 1. Test Insert
        var employeeId = await TestInsert(setup, useORM, useAsync);
        Assert.True(employeeId > 0, "Insert failed: Employee ID should be greater than 0");

        // 2. Test Update with Multiple Results
        var updateSuccess = await TestUpdate(setup, employeeId, useORM, useAsync);
        Assert.True(updateSuccess, "Update failed");

        // 3. Test Delete
        var deleteSuccess = await TestDelete(setup, useORM, useAsync);
        Assert.True(deleteSuccess, "Delete failed");
    }

    private async Task<int> TestInsert(IDatabaseSetup setup, bool useORM, bool useAsync)
    {
        int employeeId = 0;

        if (useORM)
        {
            var insertParams = new InsertEmployeeParameters
            {
                PNAME = "Test Employee",
                PDEPARTMENT = "IT",
                PSALARY = 50000M
            };

            var result = useAsync
                ? await setup.DAL.ExecuteProcedureAsync(setup, "INSERT_EMPLOYEE", insertParams)
                : setup.DAL.ExecuteProcedure(setup, "INSERT_EMPLOYEE", insertParams);

            Assert.True(result.IsSuccess);
            employeeId = insertParams.EmployeeId;
        }
        else
        {
            var parameters = new Dictionary<string, object>
            {
                ["PNAME"] = "Test Employee",
                ["PDEPARTMENT"] = "IT",
                ["PSALARY"] = 50000M
            };

            var result = useAsync
                ? await setup.DAL.ExecuteProcedureAsync(setup, "INSERT_EMPLOYEE", parameters)
                : setup.DAL.ExecuteProcedure(setup, "INSERT_EMPLOYEE", parameters);

            Assert.True(result.IsSuccess);
            
            // 마지막으로 삽입된 ID 조회
            var getLastIdResult = useAsync
                ? await setup.DAL.ExecuteProcedureAsync(setup, "GET_LAST_EMPLOYEE_ID")
                : setup.DAL.ExecuteProcedure(setup, "GET_LAST_EMPLOYEE_ID");

            Assert.True(getLastIdResult.IsSuccess);
            employeeId = getLastIdResult.ReturnValue;
            Assert.True(employeeId > 0, "Failed to get last inserted employee ID");
        }

        // Verify inserted data
        var verifyParams = new Dictionary<string, object>
        {
            ["Department"] = "IT",
            ["test"] = 0
        };

        var verifyResult = useAsync
            ? await setup.DAL.ExecuteProcedureAsync(setup, "GET_EMPLOYEES_BY_DEPT", verifyParams)
            : setup.DAL.ExecuteProcedure(setup, "GET_EMPLOYEES_BY_DEPT", verifyParams);

        Assert.True(verifyResult.IsSuccess);
        Assert.NotNull(verifyResult.DataSet);
        Assert.True(verifyResult.DataSet.Tables.Count > 0);
        Assert.True(verifyResult.DataSet.Tables[0].Rows.Count > 0);

        var insertedRow = verifyResult.DataSet.Tables[0].Rows[0];
        Assert.Equal("Test Employee", insertedRow["Name"].ToString());
        Assert.Equal("IT", insertedRow["Department"].ToString());
        Assert.Equal(50000M, Convert.ToDecimal(insertedRow["Salary"]));

        return employeeId;
    }

    private async Task<bool> TestUpdate(IDatabaseSetup setup, int employeeId, bool useORM, bool useAsync)
    {
        if (useORM)
        {
            var updateParams = new UpdateEmployeeParameters
            {
                Id = employeeId,
                NewSalary = 55000M,
                NewDepartment = "HR"
            };

            var result = useAsync
                ? await setup.DAL.ExecuteProcedureAsync(setup, "UPDATE_EMPLOYEE_WITH_RESULTS", updateParams)
                : setup.DAL.ExecuteProcedure(setup, "UPDATE_EMPLOYEE_WITH_RESULTS", updateParams);

            ValidateUpdateResult(result, employeeId);
        }
        else
        {
            var parameters = new Dictionary<string, object>
            {
                ["Id"] = employeeId,
                ["NewSalary"] = 55000M,
                ["NewDepartment"] = "HR"
            };

            var result = useAsync
                ? await setup.DAL.ExecuteProcedureAsync(setup, "UPDATE_EMPLOYEE_WITH_RESULTS", parameters)
                : setup.DAL.ExecuteProcedure(setup, "UPDATE_EMPLOYEE_WITH_RESULTS", parameters);

            ValidateUpdateResult(result, employeeId);
        }

        return true;
    }

    private void ValidateUpdateResult(SQLResult result, int employeeId)
    {
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.DataSet);
        Assert.Equal(5, result.DataSet.Tables.Count);

        var ormResultList = result.DataSet.Tables[0].ToObject<ORMTest>();
        var ormResult = result.DataSet.Tables[1].Rows[0].ToObject<ORMTest>();
        
        // Validate each result set
        for (int i = 0; i < 5; i++)
        {
            Assert.True(result.DataSet.Tables[i].Rows.Count > 0);
        }

        // Verify return value matches input Id
        Assert.Equal(employeeId, result.ReturnValue);
    }

    private async Task<bool> TestDelete(IDatabaseSetup setup, bool useORM, bool useAsync)
    {
        if (useORM)
        {
            var deleteParams = new DeleteEmployeeParameters
            {
                Department = "HR"
            };

            var result = useAsync
                ? await setup.DAL.ExecuteProcedureAsync(setup, "DELETE_EMPLOYEE_BY_DEPT", deleteParams)
                : setup.DAL.ExecuteProcedure(setup, "DELETE_EMPLOYEE_BY_DEPT", deleteParams);

            Assert.True(result.IsSuccess);
            Assert.True(deleteParams.DeletedCount > 0);
        }
        else
        {
            var parameters = new Dictionary<string, object>
            {
                ["p_department"] = "HR",
                ["DeletedCount"] = 0
            };

            var result = useAsync
                ? await setup.DAL.ExecuteProcedureAsync(setup, "DELETE_EMPLOYEE_BY_DEPT", parameters)
                : setup.DAL.ExecuteProcedure(setup, "DELETE_EMPLOYEE_BY_DEPT", parameters);

            Assert.True(result.IsSuccess);
            Assert.True(result.ReturnValue > 0);
        }

        // Verify deletion
        var verifyParams = new Dictionary<string, object>
        {
            ["Department"] = "HR",
            ["test"] = 0
        };

        var verifyResult = useAsync
            ? await setup.DAL.ExecuteProcedureAsync(setup, "GET_EMPLOYEES_BY_DEPT", verifyParams)
            : setup.DAL.ExecuteProcedure(setup, "GET_EMPLOYEES_BY_DEPT", verifyParams);

        Assert.True(verifyResult.IsSuccess);
        Assert.Equal(0, verifyResult.DataSet.Tables[0].Rows.Count);

        return true;
    }
}