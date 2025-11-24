namespace QuestionRandomizer.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;
using QuestionRandomizer.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of IConversationRepository
/// </summary>
public class ConversationRepository : IConversationRepository
{
    private readonly FirestoreDb _firestoreDb;

    public ConversationRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<Conversation>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(FirestoreCollections.Conversations)
            .WhereEqualTo(nameof(Conversation.UserId), userId)
            .OrderByDescending(nameof(Conversation.UpdatedAt));

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var conversation = doc.ConvertTo<Conversation>();
                conversation.Id = doc.Id;
                return conversation;
            })
            .ToList();
    }

    public async Task<Conversation?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Conversations).Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
        {
            return null;
        }

        var conversation = snapshot.ConvertTo<Conversation>();
        conversation.Id = snapshot.Id;

        // Verify ownership
        if (conversation.UserId != userId)
        {
            return null;
        }

        return conversation;
    }

    public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        var docRef = await _firestoreDb.Collection(FirestoreCollections.Conversations)
            .AddAsync(conversation, cancellationToken);

        conversation.Id = docRef.Id;
        return conversation;
    }

    public async Task<bool> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Conversations).Document(conversation.Id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var existingConversation = snapshot.ConvertTo<Conversation>();
        if (existingConversation.UserId != conversation.UserId)
        {
            return false;
        }

        await docRef.SetAsync(conversation, SetOptions.Overwrite, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Conversations).Document(id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var conversation = snapshot.ConvertTo<Conversation>();
        if (conversation.UserId != userId)
        {
            return false;
        }

        // Hard delete for conversations
        await docRef.DeleteAsync(cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> UpdateTimestampAsync(string conversationId, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Conversations).Document(conversationId);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var conversation = snapshot.ConvertTo<Conversation>();
        if (conversation.UserId != userId)
        {
            return false;
        }

        // Update timestamp
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { nameof(Conversation.UpdatedAt), FieldValue.ServerTimestamp }
        }, cancellationToken: cancellationToken);

        return true;
    }
}
