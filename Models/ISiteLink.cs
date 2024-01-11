namespace UAAAS.Models
{
    public interface ISiteLink
    {
        int Id { get; }
        int ParentId { get; }
        string Text { get; }
        string Url { get; }
        bool OpenInNewWindow { get; }
        int SortOrder { get; }
    }
}