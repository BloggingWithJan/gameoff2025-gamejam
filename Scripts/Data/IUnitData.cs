namespace Data
{
    public interface IUnit
    {
        string UnitName { get; set; }
        int AttackPower { get; set; }
        string Status { get; set; }
        float Health { get; set; }
    }
}