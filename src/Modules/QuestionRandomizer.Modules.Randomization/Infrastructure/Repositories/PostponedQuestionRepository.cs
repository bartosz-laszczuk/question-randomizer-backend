namespace QuestionRandomizer.Modules.Randomization.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Modules.Randomization.Domain.Entities;
using QuestionRandomizer.Modules.Randomization.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of IPostponedQuestionRepository
/// </summary>
public class PostponedQuestionRepository : IPostponedQuestionRepository
{
    private readonly FirestoreDb _firestoreDb;
    private readonly IRandomizationRepository _randomizationRepository;

    public PostponedQuestionRepository(FirestoreDb firestoreDb, IRandomizationRepository randomizationRepository)
    {
        _firestoreDb = firestoreDb;
        _randomizationRepository = randomizationRepository;
    }

    public async Task<List<PostponedQuestion>> GetByRandomizationIdAsync(string randomizationId, string userId, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            return new List<PostponedQuestion>();
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.PostponedQuestions);

        var snapshot = await collection.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var postponedQuestion = doc.ConvertTo<PostponedQuestion>();
                postponedQuestion.Id = doc.Id;
                return postponedQuestion;
            })
            .ToList();
    }

    public async Task<PostponedQuestion> AddAsync(string randomizationId, string userId, PostponedQuestion postponedQuestion, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            throw new UnauthorizedAccessException("Randomization not found or access denied");
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.PostponedQuestions);

        var docRef = await collection.AddAsync(postponedQuestion, cancellationToken);
        postponedQuestion.Id = docRef.Id;
        return postponedQuestion;
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
            .Collection(FirestoreCollections.PostponedQuestions);

        // Find document with matching questionId
        var query = collection.WhereEqualTo(nameof(PostponedQuestion.QuestionId), questionId);
        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Documents.Any())
        {
            return false;
        }

        // Delete the first matching document
        await snapshot.Documents.First().Reference.DeleteAsync(cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> UpdateTimestampAsync(string randomizationId, string userId, string questionId, CancellationToken cancellationToken = default)
    {
        // Verify randomization belongs to user
        var randomization = await _randomizationRepository.GetByIdAsync(randomizationId, userId, cancellationToken);
        if (randomization == null)
        {
            return false;
        }

        var collection = _firestoreDb.Collection(FirestoreCollections.Randomizations)
            .Document(randomizationId)
            .Collection(FirestoreCollections.PostponedQuestions);

        // Find document with matching questionId
        var query = collection.WhereEqualTo(nameof(PostponedQuestion.QuestionId), questionId);
        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Documents.Any())
        {
            return false;
        }

        // Update timestamp
        await snapshot.Documents.First().Reference.UpdateAsync(new Dictionary<string, object>
        {
            { nameof(PostponedQuestion.Timestamp), FieldValue.ServerTimestamp }
        }, cancellationToken: cancellationToken);

        return true;
    }
}
