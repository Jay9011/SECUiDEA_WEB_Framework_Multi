namespace TestProject.Model;

public class TestHelper
{
    public static string CreateTempFile()
    {
        string tempFilePath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.ini");
        return tempFilePath;
    }

    public static void DeleteTempFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}