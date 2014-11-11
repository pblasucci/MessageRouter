using System.Xml.Serialization;
using MessageRouter.Interfaces;

namespace SampleTypes.CSharp.Commands.Bar
{
    [XmlRoot(ElementName = "BarCommand2")]
    public class Command2 : ICommand
    {
    }
}
