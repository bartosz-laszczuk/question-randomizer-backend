namespace QuestionRandomizer.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;
using QuestionRandomizer.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of IRandomizationRepository
/// </summary>
public class RandomizationRepository : IRandomizationRepository
{
    private readonly FirestoreDb _firestoreDb;

    public RandomizationRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<Randomization>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .WhereEqualTo(nameof(Randomization.UserId), userId)
            .OrderByDescending(nameof(Randomization.CreatedAt));

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var randomization = doc.ConvertTo<Randomization>();
                randomization.Id = doc.Id;
                return randomization;
            })
            .ToList();
    }

    public async Task<Randomization?> GetActiveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .WhereEqualTo(nameof(Randomization.UserId), userId)
            .WhereEqualTo(nameof(Randomization.IsActive), true)
            .Limit(1);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        var doc = snapshot.Documents.FirstOrDefault();
        if (doc == null)
        {
            return null;
        }

        var randomization = doc.ConvertTo<Randomization>();
        randomization.Id = doc.Id;
        return randomization;
    }

    public async Task<Randomization?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Randomizations).Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
        {
            return null;
        }

        var randomization = snapshot.ConvertTo<Randomization>();
        randomization.Id = snapshot.Id;

        // Verify ownership
        if (randomization.UserId != userId)
        {
            return null;
        }

        return randomization;
    }

    public async Task<Randomization> CreateAsync(Randomization randomization, CancellationToken cancellationToken = default)
    {
        var docRef = await _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .AddAsync(randomization, cancellationToken);

        randomization.Id = docRef.Id;
        return randomization;
    }

    public async Task<bool> UpdateAsync(Randomization randomization, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Randomizations).Document(randomization.Id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var existingRandomization = snapshot.ConvertTo<Randomization>();
        if (existingRandomization.UserId != randomization.UserId)
        {
            return false;
        }

        await docRef.SetAsync(randomization, SetOptions.Overwrite, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Randomizations).Document(id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var randomization = snapshot.ConvertTo<Randomization>();
        if (randomization.UserId != userId)
        {
            return false;
        }

        // Hard delete for randomizations
        await docRef.DeleteAsync(cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> ClearCurrentQuestionAsync(string randomizationId, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Randomizations).Document(randomizationId);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var randomization = snapshot.ConvertTo<Randomization>();
        if (randomization.UserId != userId)
        {
            return false;
        }

        // Clear CurrentQuestionId
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { nameof(Randomization.CurrentQuestionId), FieldValue.Delete }
        }, cancellationToken: cancellationToken);

        return true;
    }
}
