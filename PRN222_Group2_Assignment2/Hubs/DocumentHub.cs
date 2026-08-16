using Microsoft.AspNetCore.SignalR;

namespace PRN222_Group2_Assignment2.Hubs
{
    public class DocumentHub : Hub
    {
        public async Task NotifyEditingSubject(int subjectId, string subjectCode, string userName)
        {
            await Clients.Others.SendAsync("UserEditingSubject", userName, subjectId, subjectCode);
        }

        public async Task NotifyFinishedEditingSubject(int subjectId, string userName)
        {
            await Clients.Others.SendAsync("UserFinishedEditingSubject", userName, subjectId);
        }

        public async Task SendDocumentUploaded(int subjectId, string documentTitle, int newDocCount)
        {
            await Clients.All.SendAsync("DocumentUploaded", subjectId, documentTitle, newDocCount);
        }

        public async Task SendDocumentDeleted(int subjectId, int documentId, int newDocCount)
        {
            await Clients.All.SendAsync("DocumentDeleted", subjectId, documentId, newDocCount);
        }

        public async Task SendSubjectUpdated(int subjectId, string actionType)
        {
            await Clients.All.SendAsync("SubjectUpdated", subjectId, actionType);
        }
    }
}
