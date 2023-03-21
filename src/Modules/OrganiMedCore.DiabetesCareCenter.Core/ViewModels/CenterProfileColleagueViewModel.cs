using IntelliMed.Core.Extensions;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OrganiMedCore.DiabetesCareCenter.Core.ViewModels
{
    public class CenterProfileColleagueViewModel
    {
        public string Id { get; set; }

        public int? MemberRightId { get; set; }

        public string Prefix { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName
        {
            get => string.Join(
                " ",
                new[] { Prefix, LastName, FirstName }.Where(x => !string.IsNullOrWhiteSpace(x) && !string.IsNullOrEmpty(x)));
        }

        [Required]
        public string Email { get; set; }

        public string CenterProfileContentItemId { get; set; }

        public string CenterProfileContentItemVersionId { get; set; }

        [Required]
        public Occupation? Occupation { get; set; }

        public IList<ColleagueStatusItem> StatusHistory { get; set; } = new List<ColleagueStatusItem>();

        public ColleagueStatusItem LatestStatusItem
        {
            get => StatusHistory.OrderByDescending(x => x.StatusDateUtc).FirstOrDefault();
        }


        public void UpdateViewModel(Colleague model)
        {
            model.ThrowIfNull();

            Id = model.Id.ToString();
            MemberRightId = model.MemberRightId;
            Prefix = model.Prefix;
            FirstName = model.FirstName;
            LastName = model.LastName;
            Email = model.Email;
            CenterProfileContentItemId = model.CenterProfileContentItemId;
            CenterProfileContentItemVersionId = model.CenterProfileContentItemVersionId;
            Occupation = model.Occupation;
            StatusHistory = model.StatusHistory;
        }

        public void UpdateModel(Colleague model)
        {
            model.ThrowIfNull();

            if (!string.IsNullOrEmpty(Id) && Guid.TryParse(Id, out var id))
            {
                model.Id = id;
            }
            model.MemberRightId = MemberRightId;
            model.Prefix = Prefix;
            model.FirstName = FirstName;
            model.LastName = LastName;
            model.Email = Email;
            model.CenterProfileContentItemId = CenterProfileContentItemId;
            model.CenterProfileContentItemVersionId = CenterProfileContentItemVersionId;
            model.Occupation = Occupation;
            model.StatusHistory = StatusHistory;
        }
    }
}
