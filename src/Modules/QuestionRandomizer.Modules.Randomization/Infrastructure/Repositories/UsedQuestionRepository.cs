namespace QuestionRandomizer.Modules.Randomization.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of IUsedQuestionRepository
/// </summary>
public class UsedQuestionRepository : IUsedQuestionRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IRandomizationRepository _randomizationRepository;

    public UsedQuestionRepository(FirestoreDb firestoreDb, IRandomizationRepository randomizationRepository)
    {
        _firestoreDb = firestoreDb;
        _randomizationRepository = randomizationRepository;
    }

    public async Task<List<UsedQuestion>> GetByRandomizationIdAsync(string randomizationId, string userId, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            return new List<UsedQuestion>();
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.UsedQuestions);

        var snapshot = await collection.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var usedQuestion = doc.ConvertTo<UsedQuestion>();
                usedQuestion.Id = doc.Id;
                return usedQuestion;
            })
            .ToList();
    }

    public async Task<UsedQuestion> AddAsync(string randomizationId, string userId, UsedQuestion usedQuestion, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            throw new UnauthorizedAccessException("Randomization not found or access denied");
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.UsedQuestions);

        var docRef = await collection.AddAsync(usedQuestion, cancellationToken);
        usedQuestion.Id = docRef.Id;
        return usedQuestion;
    }

    public async Task<bool> DeleteByQuestionIdAsync(string randomizationId, string userId, string questionId, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            return false;
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.UsedQuestions);

        // Find document with matching questionId
        var query = collection.WhereEqualTo(nameof(UsedQuestion.QuestionId), questionId);
        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Documents.Any())
        {
            return false;
        }

        // Delete the first matching document
        await snapshot.Documents.First().Reference.DeleteAsync(cancellationToken: cancellationToken);
        return true;
    }

    public async Task UpdateCategoryAsync(string randomizationId, string userId, string categoryId, string categoryName, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            return;
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.UsedQuestions);

        // Find all documents with matching categoryId
        var query = collection.WhereEqualTo(nameof(UsedQuestion.CategoryId), categoryId);
        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Documents.Any())
        {
            return;
        }

        // Batch update all matching documents
        var batch = _firestoreDb.StartBatch();

        foreach (var doc in snapshot.Documents)
        {
            batch.Update(doc.Reference, new Dictionary<string, object>
            {
                { nameof(UsedQuestion.CategoryName), categoryName }
            });
        }

        await batch.CommitAsync(cancellationToken);
    }
}
