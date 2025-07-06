using GamesFinder.Domain.Enums;

namespace GamesFinder.Domain.Interfaces.Requests;

public class GamesFilters
{
    public string Search { get; set; } = "";
    public EPriceCompare? PriceCompare { get; set; }
    public decimal PriceValue { get; set; } = 0;
    public decimal PriceRangeMin { get; set; } = 0;
    public decimal PriceRangeMax { get; set; } = 0;
    public string SortField { get; set; } = "";
    public ESort SortOrder { get; set; } = ESort.Ascending;

    public override string ToString()
    {
        return $"\tSearch: {Search}\nPriceCompare: {PriceCompare}\nPriceValue: ${PriceValue}\n" +
               $"\tPriceRangeMin: {PriceRangeMin}\nPriceRangeMax: {PriceRangeMax}\n" +
               $"\tSortField: {SortField}\nSortOrder: {SortOrder}\n";
    }
}