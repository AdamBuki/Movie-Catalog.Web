using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieCatalogApi.Entities;
using MovieCatalogApi.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MovieCatalog.Web.Pages
{
    public class TitleModel : PageModel
    {
        private readonly IMovieCatalogDataService _dataService;

        public TitleModel(IMovieCatalogDataService dataService)
        {
            _dataService = dataService;
        }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A cím megadása kötelező")]
        [StringLength(500, ErrorMessage = "A cím legfeljebb 500 karakter lehet")]
        public string PrimaryTitle { get; set; }

        [BindProperty]
        public string OriginalTitle { get; set; }

        [BindProperty]
        [Range(1900, 2100, ErrorMessage = "Az évnek 1900 és 2100 között kell lennie")]
        public int? StartYear { get; set; }

        [BindProperty]
        public int? EndYear { get; set; }

        [BindProperty]
        [Range(1, 9999, ErrorMessage = "Az időtartamnak 1 és 9999 perc között kell lennie")]
        public int? RuntimeMinutes { get; set; }

        [BindProperty]
        public TitleType TitleType { get; set; }

        [BindProperty]
        [MaxLength(3, ErrorMessage = "Legfeljebb 3 gerne választhatsz")]
        public List<int> Genres { get; set; } = new();

        [TempData]
        public string SuccessMessage { get; set; }

        public async Task<IReadOnlyCollection<SelectListItem>> GetGenreOptionsAsync()
        {
            var genres = await _dataService.GetGenresAsync();
            return genres.Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name }).ToList();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id.HasValue)
            {
                var title = await _dataService.GetTitleByIdAsync(Id.Value);
                if (title == null) return NotFound();

                PrimaryTitle = title.PrimaryTitle;
                OriginalTitle = title.OriginalTitle;
                StartYear = title.StartYear;
                EndYear = title.EndYear;
                RuntimeMinutes = title.RuntimeMinutes;
                TitleType = title.TitleType;
                Genres = title.TitleGenres.Select(g => g.Genre.Id).ToList();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await _dataService.InsertOrUpdateTitleAsync(Id, PrimaryTitle, OriginalTitle, TitleType, StartYear, EndYear, RuntimeMinutes, Genres.ToArray());
            SuccessMessage = "A film sikeresen mentve lett!";

            return RedirectToPage("/Title", new { Id });
        }
    }
}
