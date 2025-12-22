namespace QuestionRandomizer.Modules.Randomization.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of ISelectedCategoryRepository
/// </summary>
public class SelectedCategoryRepository : ISelectedCategoryRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IRandomizationRepository _randomizationRepository;

    public SelectedCategoryRepository(FirestoreDb firestoreDb, IRandomizationRepository randomizationRepository)
    {
        _firestoreDb = firestoreDb;
        _randomizationRepository = randomizationRepository;
    }

    public async Task<List<SelectedCategory>> GetByRandomizationIdAsync(string randomizationId, string userId, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            return new List<SelectedCategory>();
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.SelectedCategories);

        var snapshot = await collection.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var selectedCategory = doc.ConvertTo<SelectedCategory>();
                selectedCategory.Id = doc.Id;
                return selectedCategory;
            })
            .ToList();
    }

    public async Task<SelectedCategory> AddAsync(string randomizationId, string userId, SelectedCategory selectedCategory, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            throw new UnauthorizedAccessException("Randomization not found or access denied");
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.SelectedCategories);

        var docRef = await collection.AddAsync(selectedCategory, cancellationToken);
        selectedCategory.Id = docRef.Id;
        return selectedCategory;
    }

    public async Task<bool> DeleteByCategoryIdAsync(string randomizationId, string userId, string categoryId, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            return false;
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.SelectedCategories);

        // Find document with matching categoryId
        var query = collection.WhereEqualTo(nameof(SelectedCategory.CategoryId), categoryId);
        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Documents.Any())
        {
            return false;
        }

        // Delete the first matching document
        await snapshot.Documents.First().Reference.DeleteAsync(cancellationToken: cancellationToken);
        return true;
    }
}
