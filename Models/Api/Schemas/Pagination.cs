namespace Shoebill.Models.Api.Schemas;

public record Pagination(
    int Total,
    int Count,
    int Per_page,
    int Current_page,
    int Total_pages,
    object Links
);