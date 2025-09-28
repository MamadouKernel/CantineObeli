using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obeli_K.Data;
using Obeli_K.Enums;
using Obeli_K.Models;
using System;

namespace Obeli_K.Controllers
{
    [Authorize(Roles = "PrestataireCantine")]
    public class PrestataireController : Controller
    {
        private readonly ObeliDbContext _db;
        private readonly ILogger<PrestataireController> _logger;

        public PrestataireController(ObeliDbContext db, ILogger<PrestataireController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Page d'accueil du prestataire
        /// </summary>
        [Authorize(Roles = "PrestataireCantine")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var today = DateTime.Today;
                
                ViewBag.Date = today;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement de la page Index du prestataire");
                TempData["ErrorMessage"] = "Une erreur est survenue lors du chargement des statistiques.";
                return View();
            }
        }
    }
}