using System.Xml.Serialization;
using MessageRouter.Interfaces;

namespace SampleTypes.CSharp.Commands.Bar
{
    [XmlRoot(ElementName = "SampleTypes.CSharp.Commands.Bar.Command1")]
    public class Command1 : ICommand
    {
        public int MyInt { get; set; }
    }
}
