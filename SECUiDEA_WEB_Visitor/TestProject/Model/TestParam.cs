using CoreDAL.Interface;

namespace TestProject.Model;

public class TestParam : ISQLParam
{
    public int Param1 { get; set; }
    public string? Param2 { get; set; }
}