namespace QuestionRandomizer.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;
using QuestionRandomizer.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of ICategoryRepository
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly FirestoreDb _firestoreDb;

    public CategoryRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<Category>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(FirestoreCollections.Categories)
            .WhereEqualTo(nameof(Category.UserId), userId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var category = doc.ConvertTo<Category>();
                category.Id = doc.Id;
                return category;
            })
            .ToList();
    }

    public async Task<Category?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Categories).Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
        {
            return null;
        }

        var category = snapshot.ConvertTo<Category>();
        category.Id = snapshot.Id;

        // Verify ownership
        if (category.UserId != userId)
        {
            return null;
        }

        return category;
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        var docRef = await _firestoreDb.Collection(FirestoreCollections.Categories)
            .AddAsync(category, cancellationToken);

        category.Id = docRef.Id;
        return category;
    }

    public async Task<bool> UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Categories).Document(category.Id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var existingCategory = snapshot.ConvertTo<Category>();
        if (existingCategory.UserId != category.UserId)
        {
            return false;
        }

        await docRef.SetAsync(category, SetOptions.Overwrite, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Categories).Document(id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var category = snapshot.ConvertTo<Category>();
        if (category.UserId != userId)
        {
            return false;
        }

        // Soft delete by setting IsActive = false
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { nameof(Category.IsActive), false },
            { nameof(Category.UpdatedAt), FieldValue.ServerTimestamp }
        }, cancellationToken: cancellationToken);

        return true;
    }
}
