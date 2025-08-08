namespace AzureSqlWebApiSample.Models
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
