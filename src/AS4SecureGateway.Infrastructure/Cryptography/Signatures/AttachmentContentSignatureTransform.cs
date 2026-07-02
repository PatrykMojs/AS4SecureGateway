using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace AS4SecureGateway.Infrastructure.Cryptography.Signatures;

public sealed class AttachmentContentSignatureTransform : Transform
{
    public const string AlgorithmUri = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Signature-Transform";

    private readonly MemoryStream _internalStream = new();

    public AttachmentContentSignatureTransform()
    {
        Algorithm = AlgorithmUri;
    }

    public override Type[] InputTypes =>
    [
        typeof(Stream),
        typeof(XmlDocument),
        typeof(XmlNodeList)
    ];

    public override Type[] OutputTypes =>
    [
        typeof(Stream)
    ];

    public override void LoadInput(object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        _internalStream.SetLength(0);
        _internalStream.Position = 0;

        switch (obj)
        {
            case Stream stream:
                stream.CopyTo(_internalStream);
                break;

            case XmlDocument xmlDocument:
                WriteXmlDocument(xmlDocument);
                break;

            case XmlNodeList nodeList:
                WriteXmlNodeList(nodeList);
                break;

            default:
                throw new ArgumentException(
                    $"Unsupported input type: {obj.GetType().FullName}",
                    nameof(obj));
        }

        _internalStream.Position = 0;
    }

    public override object GetOutput()
    {
        return GetOutput(typeof(Stream));
    }

    public override object GetOutput(Type type)
    {
        if (type != typeof(Stream))
            throw new ArgumentException("Unsupported output type.", nameof(type));

        _internalStream.Position = 0;
        return _internalStream;
    }

    protected override XmlNodeList? GetInnerXml()
    {
        return null;
    }

    public override void LoadInnerXml(XmlNodeList nodeList)
    {
    }

    private void WriteXmlDocument(XmlDocument xmlDocument)
    {
        using var writer = new StreamWriter(
            _internalStream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            bufferSize: 1024,
            leaveOpen: true);

        xmlDocument.Save(writer);
        writer.Flush();
    }

    private void WriteXmlNodeList(XmlNodeList nodeList)
    {
        using var writer = new StreamWriter(
            _internalStream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            bufferSize: 1024,
            leaveOpen: true);

        foreach (XmlNode node in nodeList)
        {
            writer.Write(node.OuterXml);
        }

        writer.Flush();
    }
}