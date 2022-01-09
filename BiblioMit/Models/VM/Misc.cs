namespace BiblioMit.Models.VM
{
    public class AmData
    {
        public AmData(string date, double value)
        {
            Date = date;
            Value = value;
        }
        public string Date { get; private set; }
        public double Value { get; private set; }
    }
    public class GMapCoordinate
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
    public class GMapPolygonCentre : GMapPolygon
    {
        public int Rut { get; set; }
        public string? BusinessName { get; set; }
        public int ComunaId { get; set; }
    }
    public abstract class GMapPolygonBase
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Comuna { get; set; }
        public string? Provincia { get; set; }
        public string? Region { get; set; }
        public int Code { get; set; }
    }
    public class GMapPolygon : GMapPolygonBase
    {
        public IEnumerable<GMapCoordinate>? Position { get; set; }
    }
    public class GMapMultiPolygon : GMapPolygonBase
    {
        public IEnumerable<IEnumerable<GMapCoordinate>>? Position { get; set; }
    }
    public class IChoices
    {
        public string? Label { get; set; }
    }
    public class ChoicesItem : IChoices
    {
        public object? Value { get; set; }
    }
    public class ChoicesItemSelected : ChoicesItem
    {
        public bool Selected { get; set; }
    }
    public class ChoicesGroup : IChoices
    {
        public IEnumerable<ChoicesItem>? Choices { get; set; }
    }
}
