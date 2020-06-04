namespace Nbody.Gui.Attributes
{
    public class PropEditAttribute : System.Attribute
    {
        public string Name { get; set; }
        public bool Editable { get; set; }
        public PropEditAttribute(string name = default, bool editable = true)
        {
            Name = name;
            Editable = editable;
        }
    }
}
