using Microsoft.Build.Framework;
namespace msbuild.tasks.MsTest
{
    public class MsTest : CBase, ITask
    {
        public bool Execute()
        {
            return base.Run();
        }
    }
}
