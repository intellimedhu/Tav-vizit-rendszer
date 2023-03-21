using OrganiMedCore.Core.Models;
using OrchardCore.ContentManagement;
using System;
using System.Collections.Generic;
using System.Text;
using YesSql.Indexes;

namespace OrganiMedCore.Core.Indexes
{
    public class EVisitOrganizationUserProfilePartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<EVisitOrganizationUserProfilePartIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<EVisitOrganizationUserProfilePart>();
                    if (part == null) return null;

                    return new EVisitOrganizationUserProfilePartIndex()
                    {
                        SharedUserId = part.SharedUserId,
                        StampNumber = part.StampNumber,
                        Email = part.Email
                    };
                });
        }
    }
}