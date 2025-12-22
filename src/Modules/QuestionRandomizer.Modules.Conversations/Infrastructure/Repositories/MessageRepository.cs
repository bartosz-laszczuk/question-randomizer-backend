namespace QuestionRandomizer.Modules.Conversations.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Modules.Conversations.Domain.Entities;
using QuestionRandomizer.Modules.Conversations.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of IMessageRepository
/// </summary>
public class MessageRepository : IMessageRepository
{
    private readonly FirestoreDb _firestoreDb;

    public MessageRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<Message>> GetByConversationIdAsync(string conversationId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(FirestoreCollections.Messages)
            .WhereEqualTo(nameof(Message.ConversationId), conversationId)
            .OrderBy(nameof(Message.Timestamp));

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var message = doc.ConvertTo<Message>();
                message.Id = doc.Id;
                return message;
            })
            .ToList();
    }

    public async Task<Message?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Messages).Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
        {
            return null;
        }

        var message = snapshot.ConvertTo<Message>();
        message.Id = snapshot.Id;
        return message;
    }

    public async Task<Message> CreateAsync(Message message, CancellationToken cancellationToken = default)
    {
        var docRef = await _firestoreDb.Collection(FirestoreCollections.Messages)
            .AddAsync(message, cancellationToken);

        message.Id = docRef.Id;
        return message;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Messages).Document(id);

        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        await docRef.DeleteAsync(cancellationToken: cancellationToken);
        return true;
    }
}
