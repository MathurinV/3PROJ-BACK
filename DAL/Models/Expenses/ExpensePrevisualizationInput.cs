namespace DAL.Models.Expenses;

public class ExpensePrevisualizationInput
{
    public ICollection<KeyValuePair<Guid, decimal?>> UserAmountsList { get; set; } = null!;
    public Guid GroupId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
}