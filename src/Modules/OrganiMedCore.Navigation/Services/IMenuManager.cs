using Microsoft.AspNetCore.Mvc;
using OrganiMedCore.Navigation.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrganiMedCore.Navigation.Services
{
    public interface IMenuManager
    {
        Task<IEnumerable<MenuItem>> BuildMenuAsync(
            ActionContext actionContext, 
            string menuId,
            object additionalData = null);
    }
}
