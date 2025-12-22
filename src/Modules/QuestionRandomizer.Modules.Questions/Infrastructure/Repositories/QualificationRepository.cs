namespace QuestionRandomizer.Modules.Questions.Infrastructure.Repositories;

using Google.Cloud.Firestore;
using QuestionRandomizer.Modules.Questions.Domain.Entities;
using QuestionRandomizer.Modules.Questions.Domain.Interfaces;
using QuestionRandomizer.SharedKernel.Infrastructure.Firebase;

/// <summary>
/// Firestore implementation of IQualificationRepository
/// </summary>
public class QualificationRepository : IQualificationRepository
{
    private readonly FirestoreDb _firestoreDb;

    public QualificationRepository(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<Qualification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var query = _firestoreDb.Collection(FirestoreCollections.Qualifications)
            .WhereEqualTo(nameof(Qualification.UserId), userId);

        var snapshot = await query.GetSnapshotAsync(cancellationToken);

        return snapshot.Documents
            .Select(doc =>
            {
                var qualification = doc.ConvertTo<Qualification>();
                qualification.Id = doc.Id;
                return qualification;
            })
            .ToList();
    }

    public async Task<Qualification?> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Qualifications).Document(id);
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
        {
            return null;
        }

        var qualification = snapshot.ConvertTo<Qualification>();
        qualification.Id = snapshot.Id;

        // Verify ownership
        if (qualification.UserId != userId)
        {
            return null;
        }

        return qualification;
    }

    public async Task<Qualification> CreateAsync(Qualification qualification, CancellationToken cancellationToken = default)
    {
        var docRef = await _firestoreDb.Collection(FirestoreCollections.Qualifications)
            .AddAsync(qualification, cancellationToken);

        qualification.Id = docRef.Id;
        return qualification;
    }

    public async Task<List<Qualification>> CreateManyAsync(List<Qualification> qualifications, CancellationToken cancellationToken = default)
    {
        var batch = _firestoreDb.StartBatch();
        var collection = _firestoreDb.Collection(FirestoreCollections.Qualifications);
        var createdQualifications = new List<Qualification>();

        foreach (var qualification in qualifications)
        {
            var docRef = collection.Document();
            batch.Set(docRef, qualification);

            var createdQualification = qualification;
            createdQualification.Id = docRef.Id;
            createdQualifications.Add(createdQualification);
        }

        await batch.CommitAsync(cancellationToken);
        return createdQualifications;
    }

    public async Task<bool> UpdateAsync(Qualification qualification, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Qualifications).Document(qualification.Id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var existingQualification = snapshot.ConvertTo<Qualification>();
        if (existingQualification.UserId != qualification.UserId)
        {
            return false;
        }

        await docRef.SetAsync(qualification, SetOptions.Overwrite, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var docRef = _firestoreDb.Collection(FirestoreCollections.Qualifications).Document(id);

        // Verify document exists and belongs to user
        var snapshot = await docRef.GetSnapshotAsync(cancellationToken);
        if (!snapshot.Exists)
        {
            return false;
        }

        var qualification = snapshot.ConvertTo<Qualification>();
        if (qualification.UserId != userId)
        {
            return false;
        }

        // Soft delete by setting IsActive = false
        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { nameof(Qualification.IsActive), false },
            { nameof(Qualification.UpdatedAt), FieldValue.ServerTimestamp }
        }, cancellationToken: cancellationToken);

        return true;
    }
}
