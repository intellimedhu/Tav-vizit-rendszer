using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Environment.Shell;
using OrganiMedCore.Manager.ViewModels;
using System;
using System.IO;
using System.Linq;

namespace OrganiMedCore.Manager.Controllers
{
    [Admin]
    public class AccessLogController : Controller
    {
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;


        public AccessLogController(IOptions<ShellOptions> shellOptions, ShellSettings shellSettings)
        {
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
        }


        public ActionResult Index()
        {
            var files = new DirectoryInfo(GetLogsPath())
                .GetFiles("access-log*")
                .Select(x => new AccessLogViewModel { Name = x.Name, Date = ConvertLogFileDate(x.Name) });

            return View(files);
        }

        public IActionResult Display(string id)
        {
            var file = new DirectoryInfo(GetLogsPath()).GetFiles(id).FirstOrDefault();

            if (file == null)
            {
                return NotFound();
            }

            var entries = (System.IO.File.ReadAllLines(file.FullName))
                .Select(x =>
                {
                    var splitted = x.Split('|');
                    return new AccessLogEntryViewModel
                    {
                        Date = ConvertLogFileDate(splitted[0]),
                        OrganizationId = splitted[1],
                        Message = splitted[2]
                    };
                });

            var viewModel = new AccessLogViewModel
            {
                Name = id,
                Date = ConvertLogFileDate(id),
                Entries = entries
            };

            return View(viewModel);
        }


        private DateTime ConvertLogFileDate(string file) =>
            Convert.ToDateTime(file.Substring(11, 10));

        private string GetLogsPath() =>
            Path.Combine(_shellOptions.Value.ShellsApplicationDataPath, "logs");
    }
}
