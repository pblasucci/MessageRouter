using System.Xml.Serialization;
using MessageRouter.Interfaces;

namespace SampleTypes.CSharp.Commands.Bar
{
    [XmlRoot(ElementName = "BarCommand4")]
    public class Command4 : ICommand
    {
    }
}
