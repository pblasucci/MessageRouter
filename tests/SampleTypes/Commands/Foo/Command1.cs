using System.Xml.Serialization;
using MessageRouter.Interfaces;

namespace SampleTypes.CSharp.Commands.Foo
{
    [XmlRoot(ElementName = "FooCommand1")]
    public class Command1 : ICommand
    {
        public float MyFloat { get; set; }
    }
}
