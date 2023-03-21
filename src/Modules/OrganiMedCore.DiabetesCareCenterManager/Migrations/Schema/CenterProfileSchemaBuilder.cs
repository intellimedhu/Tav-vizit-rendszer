using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Indexes;
using System;
using YesSql.Sql;

namespace OrganiMedCore.DiabetesCareCenterManager.Migrations.Schema
{
    public static class CenterProfileSchemaBuilder
    {
        public static void Build(ISchemaBuilder schemaBuilder)
        {
            schemaBuilder.CreateMapIndexTable(nameof(CenterProfilePartIndex), table => table
                .Column<int>(nameof(CenterProfilePartIndex.AccreditationStatus))
                .Column<DateTime>(nameof(CenterProfilePartIndex.AccreditationStatusDateUtc))
                .Column<int>(nameof(CenterProfilePartIndex.MemberRightId))
                .Column<int>(nameof(CenterProfilePartIndex.CenterZipCode))
                .Column<bool>(nameof(CenterProfilePartIndex.Created))
            );

            schemaBuilder.CreateColumnIndexes(nameof(CenterProfilePartIndex),
                nameof(CenterProfilePartIndex.AccreditationStatus),
                nameof(CenterProfilePartIndex.AccreditationStatusDateUtc),
                nameof(CenterProfilePartIndex.MemberRightId),
                nameof(CenterProfilePartIndex.CenterZipCode),
                nameof(CenterProfilePartIndex.Created));

            // Colleagues table
            schemaBuilder.CreateMapIndexTable(nameof(CenterProfileColleagueIndex), table => table
                .Column<string>(nameof(CenterProfileColleagueIndex.CenterProfileContentItemId))
                .Column<string>(nameof(CenterProfileColleagueIndex.CenterProfileContentItemVersionId))
                .Column<string>(nameof(CenterProfileColleagueIndex.ColleagueId))
                .Column<string>(nameof(CenterProfileColleagueIndex.Email))
                .Column<string>(nameof(CenterProfileColleagueIndex.FirstName))
                .Column<string>(nameof(CenterProfileColleagueIndex.LastName))
                .Column<int>(nameof(CenterProfileColleagueIndex.MemberRightId))
                .Column<int>(nameof(CenterProfileColleagueIndex.Occupation))
                .Column<string>(nameof(CenterProfileColleagueIndex.Prefix))
            );

            schemaBuilder.CreateColumnIndexes(nameof(CenterProfileColleagueIndex),
                nameof(CenterProfileColleagueIndex.CenterProfileContentItemId),
                nameof(CenterProfileColleagueIndex.CenterProfileContentItemVersionId),
                nameof(CenterProfileColleagueIndex.ColleagueId),
                nameof(CenterProfileColleagueIndex.Email),
                nameof(CenterProfileColleagueIndex.FirstName),
                nameof(CenterProfileColleagueIndex.LastName),
                nameof(CenterProfileColleagueIndex.MemberRightId),
                nameof(CenterProfileColleagueIndex.Occupation),
                nameof(CenterProfileColleagueIndex.Prefix));
        }
    }
}
