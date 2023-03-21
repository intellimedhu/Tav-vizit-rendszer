using Lucene.Net.Documents;
using OrchardCore.Indexing;
using OrganiMedCore.Core.Constants;
using OrganiMedCore.Core.Models;
using System.Threading.Tasks;

namespace OrganiMedCore.Manager.SharedData.Indexing
{
    public class EVisitPatientProfilePartIndexHandler : ContentPartIndexHandler<EVisitPatientProfilePart>
    {
        public override Task BuildIndexAsync(EVisitPatientProfilePart part, BuildPartIndexContext context)
        {
            var options = context.Settings.ToOptions()
                | DocumentIndexOptions.Analyze
                ;

            context.DocumentIndex.Entries.Add(new DocumentIndex.DocumentIndexEntry(LuceneEntryNames.FirstName, part.FirstName, DocumentIndex.Types.Text, options));
            context.DocumentIndex.Entries.Add(new DocumentIndex.DocumentIndexEntry(LuceneEntryNames.LastName, part.LastName, DocumentIndex.Types.Text, options));
            context.DocumentIndex.Entries.Add(new DocumentIndex.DocumentIndexEntry(LuceneEntryNames.MothersName, part.MothersName, DocumentIndex.Types.Text, options));
            if (part.DateOfBirth.HasValue)
            {
                context.DocumentIndex.Entries.Add(new DocumentIndex.DocumentIndexEntry(LuceneEntryNames.DateOfBirth, DateTools.DateToString(part.DateOfBirth.Value, DateTools.Resolution.DAY), DocumentIndex.Types.Text, options));
            }
            context.DocumentIndex.Entries.Add(new DocumentIndex.DocumentIndexEntry(LuceneEntryNames.Email, part.Email, DocumentIndex.Types.Text, options));
            context.DocumentIndex.Entries.Add(new DocumentIndex.DocumentIndexEntry(LuceneEntryNames.PatientIdentifierValue, part.PatientIdentifierValue, DocumentIndex.Types.Text, options));
            context.DocumentIndex.Entries.Add(new DocumentIndex.DocumentIndexEntry(LuceneEntryNames.AttendedOrganizationNames, string.Join(" ", part.AttendedOrganizationNames), DocumentIndex.Types.Text, options));

            return Task.CompletedTask;
        }
    }
}
