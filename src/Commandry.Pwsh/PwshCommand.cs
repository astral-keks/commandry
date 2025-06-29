using System.Management.Automation;

namespace Commandry
{
    public abstract class PwshCommand : Command
    {
        protected void ReportProgress(ProgressRecord progress)
        {
            switch (progress.RecordType)
            {
                case ProgressRecordType.Processing:
                    Progress?.Report(progress.PercentComplete, $"{Name}: {progress.StatusDescription} {progress.PercentComplete}%");
                    break;
                case ProgressRecordType.Completed:
                    Progress?.Report(100, $"{Name}: {progress.StatusDescription}");
                    break;
            }
        }
    }
}
