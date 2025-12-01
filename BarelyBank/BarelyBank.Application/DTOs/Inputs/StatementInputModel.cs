namespace BarelyBank.Application.DTOs.Inputs
{
    public class StatementInputModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "timestamp"; // "timestamp" ou "amount"
        public string SortOrder { get; set; } = "desc"; // "asc" ou "desc"
    }
}