using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrganiMedCore.DiabetesCareCenter.Core;
using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenter.Core.Services;
using OrganiMedCore.DiabetesCareCenterManager.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.DiabetesCareCenterManager.Controllers
{
    [Admin, Authorize]
    public class SettlementsController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly ITerritoryService _territoryService;
        private readonly YesSql.ISession _session;


        public IHtmlLocalizer T { get; set; }


        public SettlementsController(
            IAuthorizationService authorizationService,
            IHtmlLocalizer<SettlementsController> htmlLocalizer,
            INotifier notifier,
            ITerritoryService territoryService,
            YesSql.ISession session)
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _territoryService = territoryService;
            _session = session;

            T = htmlLocalizer;
        }


        public async Task<IActionResult> Import()
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageZipCodes))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost, ActionName(nameof(Import))]
        public async Task<IActionResult> ImportPost(IFormFile[] files)
        {
            if (!await _authorizationService.AuthorizeAsync(User, ManagerPermissions.ManageZipCodes))
            {
                return Unauthorized();
            }

            if (files == null || files.Length != 2)
            {
                _notifier.Warning(T["Pontosan 2 db excel fájl kiválasztása kötelező"]);

                return Redirect(nameof(Import));
            }

            if (!files.All(file => file.ContentType.ToLower().Contains("spreadsheetml")))
            {
                _notifier.Warning(T["Csak excel formátum megengedett"]);

                return Redirect(nameof(Import));
            }

            var filePosta = files.FirstOrDefault(file => file.FileName == "posta.xlsx");
            if (filePosta == null || filePosta.Length == 0)
            {
                _notifier.Warning(T["Nem lett feltöltve a 'posta.xlsx'!"]);

                return Redirect(nameof(Import));
            }

            var fileCounties = files.FirstOrDefault(file => file.FileName == "megyek.xlsx");
            if (fileCounties == null)
            {
                _notifier.Warning(T["Nem lett feltöltve a 'megyek.xlsx'!"]);

                return Redirect(nameof(Import));
            }

            IEnumerable<SettlementDto> settlements;
            var cities = new Dictionary<string, IEnumerable<CityDto>>();
            using (var fileStream = filePosta.OpenReadStream())
            {
                // City names ending with * are in other sheets.
                settlements = fileStream.ConvertSheetToObjects<SettlementDto>(0, 1);

                cities.Add("Budapest", fileStream.ConvertSheetToObjects<CityDto>(2, 1));
                cities.Add("Miskolc", fileStream.ConvertSheetToObjects<CityDto>(3, 1));
                cities.Add("Debrecen", fileStream.ConvertSheetToObjects<CityDto>(4, 1));
                cities.Add("Szeged", fileStream.ConvertSheetToObjects<CityDto>(5, 1));
                cities.Add("Pécs", fileStream.ConvertSheetToObjects<CityDto>(6, 1));
                cities.Add("Győr", fileStream.ConvertSheetToObjects<CityDto>(7, 1));
            }

            IEnumerable<SettlementDto> settlementsToCounties;
            IEnumerable<TerritoryDto> consultants;
            using (var fileStream = fileCounties.OpenReadStream())
            {
                settlementsToCounties = fileStream.ConvertSheetToObjects<SettlementDto>(0, 1);
                consultants = fileStream.ConvertSheetToObjects<TerritoryDto>(1, 1);
            }

            try
            {
                var territories = new List<Territory>();
                var counties = settlementsToCounties.Select(x => x.Description.Trim()).Distinct();
                foreach (var county in counties)
                {
                    var dto = consultants.FirstOrDefault(x => x.County == county);
                    if (dto == null)
                    {
                        throw new Exception($"A második munkalapon nem található az első munkalap szerinti megye: '{county}'");
                    }

                    if (string.IsNullOrEmpty(dto.Consultant?.Trim()))
                    {
                        throw new Exception($"'{county}' megyéhez nincs szaktanácsadó megadva.");
                    }

                    var territory = new Territory()
                    {
                        Name = county.Trim(),
                        Consultant = dto.Consultant.Trim()
                    };

                    _session.Save(territory);
                    territories.Add(territory);
                }

                foreach (var settlement in settlements)
                {
                    if ((string.IsNullOrEmpty(settlement.Name) &&
                        string.IsNullOrEmpty(settlement.ZipCode)) ||
                        settlement.Name.Trim().EndsWith("*"))
                    {
                        continue;
                    }


                    if (!int.TryParse(settlement.ZipCode, out int zipCode))
                    {
                        throw new Exception(string.Format("Hibás irányítószám: {0}, {1}", settlement.ZipCode, settlement.Name));
                    }

                    if (string.IsNullOrEmpty(settlement.Name))
                    {
                        throw new Exception("Nincs település név: " + settlement.ZipCode);
                    }

                    var settlementName = settlement.Name.Trim();
                    var connection = settlementsToCounties.FirstOrDefault(x => int.Parse(x.ZipCode) == zipCode && x.Name.Trim() == settlementName);
                    if (connection == null)
                    {
                        throw new Exception($"A megyei adatbázisból hinyzik az alábbi település: {zipCode} {settlementName}");
                    }

                    var territory = territories.First(t => t.Name == connection.Description.Trim());

                    _session.Save(new Settlement()
                    {
                        Name = settlementName,
                        ZipCode = zipCode,
                        Description = settlement.Description?.Trim(),
                        TerritoryId = territory.Id
                    });
                }

                foreach (var cityGroup in cities)
                {
                    var unitfiedCities = cityGroup.Value
                        .Where(x =>
                            !string.IsNullOrEmpty(x.Street) &&
                            !string.IsNullOrEmpty(x.ZipCode)
                        )
                        .GroupBy(x => new
                        {
                            x.ZipCode,
                            x.Street,
                            x.PublicLand
                            //x.District
                        }).Select(x => x.First());

                    foreach (var city in unitfiedCities)
                    {
                        if (!int.TryParse(city.ZipCode, out int zipCode))
                        {
                            throw new Exception(string.Format("Hibás irányítószám: {0}, {1}", city.ZipCode, cityGroup.Key));
                        }

                        var description = (city.Street?.Trim() + " " + city.PublicLand?.Trim()).Trim();

                        var settlement = new Settlement()
                        {
                            Name = cityGroup.Key,
                            ZipCode = zipCode,
                            Description = description
                        };

                        var connection = settlementsToCounties.FirstOrDefault(x => int.Parse(x.ZipCode) == zipCode && x.Name.Trim() == settlement.Name);
                        if (connection == null)
                        {
                            throw new Exception($"A megyei adatbázisból hinyzik az alábbi település: {zipCode} {settlement.Name}");
                        }

                        var territory = territories.First(t => t.Name == connection.Description.Trim());
                        settlement.TerritoryId = territory.Id;

                        _session.Save(settlement);
                    }
                }

                _territoryService.ClearTerritoryCache();

                _notifier.Success(T["Az importálás sikeresen befejeződött."]);
            }
            catch (Exception ex)
            {
                _session.Cancel();

                _notifier.Warning(T["Hiba a feltöltés során: " + ex.Message]);
                _notifier.Warning(T[ex.StackTrace]);
            }

            return Redirect(nameof(Import));
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            return View(await _territoryService.GetTerritoriesAsync());
        }

        public async Task<IActionResult> Edit(int? id = null)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessAdminPanel))
            {
                return Unauthorized();
            }

            if (!id.HasValue)
            {
                return BadRequest();
            }

            var territory = await _territoryService.GetTerritoryAsync(id.Value);
            if (territory == null)
            {
                return NotFound();
            }

            return View(territory);
        }
    }
}
