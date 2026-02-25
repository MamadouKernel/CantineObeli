using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Web;

namespace Obeli_K.Models.ViewModels
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

        private readonly HttpContext _httpContext;
        private readonly string _actionName;
        private readonly string _controllerName;
        private readonly object? _routeValues;

        public PaginationViewModel(HttpContext httpContext, string actionName, string controllerName, object? routeValues = null)
        {
            _httpContext = httpContext;
            _actionName = actionName;
            _controllerName = controllerName;
            _routeValues = routeValues;
        }

        public string GetPageUrl(int page)
        {
            var query = _httpContext.Request.Query;
            var queryString = new List<string>();
            
            // Conserver tous les paramètres de requête existants sauf 'page'
            foreach (var kvp in query)
            {
                if (kvp.Key.ToLower() != "page")
                {
                    queryString.Add($"{kvp.Key}={Uri.EscapeDataString(kvp.Value.ToString())}");
                }
            }
            
            // Ajouter le nouveau numéro de page
            queryString.Add($"page={page}");
            
            var baseUrl = $"/{_controllerName}/{_actionName}";
            var fullQueryString = string.Join("&", queryString);
            
            return $"{baseUrl}?{fullQueryString}";
        }

        public static PaginationViewModel Create<T>(IEnumerable<T> items, int page, int pageSize, HttpContext httpContext, string actionName, string controllerName, object? routeValues = null)
        {
            var totalItems = items.Count();

            return new PaginationViewModel(httpContext, actionName, controllerName, routeValues)
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }
    }
}
