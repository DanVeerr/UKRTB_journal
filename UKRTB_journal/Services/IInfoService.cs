namespace UKRTB_journal.Services
{
    public interface IInfoService
    {
        string GetStudentName(int studentId);

        string GetStudentSurName(int studentId);

        string GetTypeOfFileName(int typeOfFile);

        string GetGroupName(int groupId);
    }
}
