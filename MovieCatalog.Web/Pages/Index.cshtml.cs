using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MovieCatalogApi.Services;
using MovieCatalogApi.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MovieCatalog.Web.Utils;

namespace MovieCatalog.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IMovieCatalogDataService _dataService;

        public IndexModel(ILogger<IndexModel> logger, IMovieCatalogDataService dataService)
        {
            _logger = logger;
            _dataService = dataService;
        }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 20;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public TitleSort SortField { get; set; } = TitleSort.ReleaseYear;

        [BindProperty(SupportsGet = true)]
        public bool SortDescending { get; set; } = true;

        public List<Title> FilteredTitles { get; set; } = new();
        public Dictionary<Genre, int> GenresWithCounts { get; set; } = new();

        public int LastPage => (FilteredTitles.Count + PageSize - 1) / PageSize;

        public IReadOnlyList<int> PageOptions =>
            new[]
            {
                1, 2, 3,
                PageNumber - 1, PageNumber, PageNumber + 1,
                LastPage - 2, LastPage - 1, LastPage
            }
            .Where(i => i > 0 && i <= LastPage)
            .Distinct()
            .OrderBy(i => i)
            .ToArray();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!Request.Query.ContainsKey("PageSize") || !Request.Query.ContainsKey("PageNumber") ||
                !Request.Query.ContainsKey("SortField") || !Request.Query.ContainsKey("SortDescending"))
            {
                return RedirectToPage(new
                {
                    PageSize,
                    PageNumber,
                    SortField,
                    SortDescending
                });
            }

            GenresWithCounts = await _dataService.GetGenresWithTitleCountsAsync();

            var filter = new TitleFilter
            {
                TitleTypes = new List<TitleType> { TitleType.Movie },
                StartYearMax = 2022
            };

            var pagedResult = await _dataService.GetTitlesAsync(
                pageSize: PageSize,
                page: PageNumber,
                filter: filter,
                titleSort: SortField,
                sortDescending: SortDescending
            );

            FilteredTitles = pagedResult.Results.ToList();

            return Page();
        }
    }
}
