using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR;
using PRN222_Group2_Assignment1.Services;
using PRN222_Group2_Assignment1.ViewModels;

namespace PRN222_Group2_Assignment2.Hubs
{
    public class DocumentHub(IRagChatService ragChatService) : Hub
    {
        public async IAsyncEnumerable<ChatStreamPacket> StreamChatMessage(
            SendChatRequest request, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var httpContext = Context.GetHttpContext();
            var userIdStr = httpContext?.Session.GetString("UserId") ?? httpContext?.Session.GetInt32("UserId")?.ToString();
            int userId = int.TryParse(userIdStr, out var parsedId) ? parsedId : 1;

            await foreach (var packet in ragChatService.StreamChatQueryAsync(request, userId, cancellationToken))
            {
                yield return packet;
            }
        }

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
