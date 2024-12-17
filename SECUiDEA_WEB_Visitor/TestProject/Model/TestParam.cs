using CoreDAL.ORM;

namespace TestProject.Model;

public class TestParam : SQLParam
{
    public int Param1 { get; set; }
    public string? Param2 { get; set; }
}