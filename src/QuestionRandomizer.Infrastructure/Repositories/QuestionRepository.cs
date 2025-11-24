namespace QuestionRandomizer.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Domain.Entities;
using QuestionRandomizer.Domain.Interfaces;
using QuestionRandomizer.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of IQuestionRepository
/// </summary>
public class QuestionRepository : IQuestionRepository
{
    private readonly FirestoreDb _firestoreDb;

    public QuestionRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<Question>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(FirestoreCollections.Questions)
            .WhereEqualTo(nameof(Question.UserId), userId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var question = doc.ConvertTo<Question>();
                question.Id = doc.Id;
                return question;
            })
            .ToList();
    }

    public async Task<Question?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Questions).Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
        {
            return null;
        }

        var question = snapshot.ConvertTo<Question>();
        question.Id = snapshot.Id;

        // Verify ownership
        if (question.UserId != userId)
        {
            return null;
        }

        return question;
    }

    public async Task<Question> CreateAsync(Question question, CancellationToken cancellationToken = default)
    {
        var docRef = await _firestoreDb.Collection(FirestoreCollections.Questions)
            .AddAsync(question, cancellationToken);

        question.Id = docRef.Id;
        return question;
    }

    public async Task<bool> UpdateAsync(Question question, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Questions).Document(question.Id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var existingQuestion = snapshot.ConvertTo<Question>();
        if (existingQuestion.UserId != question.UserId)
        {
            return false;
        }

        await docRef.SetAsync(question, SetOptions.Overwrite, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Questions).Document(id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var question = snapshot.ConvertTo<Question>();
        if (question.UserId != userId)
        {
            return false;
        }

        // Soft delete by setting IsActive = false
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { nameof(Question.IsActive), false },
            { nameof(Question.UpdatedAt), FieldValue.ServerTimestamp }
        }, cancellationToken: cancellationToken);

        return true;
    }

    public async Task<List<Question>> GetByCategoryIdAsync(string categoryId, string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(FirestoreCollections.Questions)
            .WhereEqualTo(nameof(Question.UserId), userId)
            .WhereEqualTo(nameof(Question.CategoryId), categoryId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var question = doc.ConvertTo<Question>();
                question.Id = doc.Id;
                return question;
            })
            .ToList();
    }

    public async Task<List<Question>> CreateManyAsync(List<Question> questions, CancellationToken cancellationToken = default)
    {
        var batch = _firestoreDb.StartBatch();
        var collection = _firestoreDb.Collection(FirestoreCollections.Questions);
        var createdQuestions = new List<Question>();

        foreach (var question in questions)
        {
            var docRef = collection.Document();
            batch.Set(docRef, question);

            var createdQuestion = question;
            createdQuestion.Id = docRef.Id;
            createdQuestions.Add(createdQuestion);
        }

        await batch.CommitAsync(cancellationToken);
        return createdQuestions;
    }

    public async Task UpdateManyAsync(List<Question> questions, CancellationToken cancellationToken = default)
    {
        var batch = _firestoreDb.StartBatch();
        var collection = _firestoreDb.Collection(FirestoreCollections.Questions);

        foreach (var question in questions)
        {
            var docRef = collection.Document(question.Id);
            batch.Set(docRef, question, SetOptions.Overwrite);
        }

        await batch.CommitAsync(cancellationToken);
    }

    public async Task RemoveCategoryIdAsync(string categoryId, string userId, CancellationToken cancellationToken = default)
    {
        // Get all questions with this categoryId for this user
        var query = _firestoreDb.Collection(FirestoreCollections.Questions)
            .WhereEqualTo(nameof(Question.UserId), userId)
            .WhereEqualTo(nameof(Question.CategoryId), categoryId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Documents.Any())
        {
            return;
        }

        // Batch update to remove categoryId and categoryName
        var batch = _firestoreDb.StartBatch();

        foreach (var doc in snapshot.Documents)
        {
            batch.Update(doc.Reference, new Dictionary<string, object>
            {
                { nameof(Question.CategoryId), FieldValue.Delete },
                { nameof(Question.CategoryName), FieldValue.Delete },
                { nameof(Question.UpdatedAt), FieldValue.ServerTimestamp }
            });
        }

        await batch.CommitAsync(cancellationToken);
    }

    public async Task RemoveQualificationIdAsync(string qualificationId, string userId, CancellationToken cancellationToken = default)
    {
        // Get all questions with this qualificationId for this user
        var query = _firestoreDb.Collection(FirestoreCollections.Questions)
            .WhereEqualTo(nameof(Question.UserId), userId)
            .WhereEqualTo(nameof(Question.QualificationId), qualificationId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Documents.Any())
        {
            return;
        }

        // Batch update to remove qualificationId and qualificationName
        var batch = _firestoreDb.StartBatch();

        foreach (var doc in snapshot.Documents)
        {
            batch.Update(doc.Reference, new Dictionary<string, object>
            {
                { nameof(Question.QualificationId), FieldValue.Delete },
                { nameof(Question.QualificationName), FieldValue.Delete },
                { nameof(Question.UpdatedAt), FieldValue.ServerTimestamp }
            });
        }

        await batch.CommitAsync(cancellationToken);
    }
}
