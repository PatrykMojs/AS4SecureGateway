namespace AS4SecureGateway.Infrastructure.Files;

public static class DirectoryFinder
{
    public static string FindAncestorNamed(string startPath, string folderName, int maxDepth = 15)
    {
        if (string.IsNullOrWhiteSpace(startPath))
            throw new ArgumentException("Start path cannot be empty.", nameof(startPath));

        if (string.IsNullOrWhiteSpace(folderName))
            throw new ArgumentException("Folder name cannot be empty.", nameof(folderName));

        var directory = new DirectoryInfo(startPath);

        for (var i = 0; i < maxDepth && directory is not null; i++)
        {
            if (directory.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Ancestor directory '{folderName}' was not found starting from '{startPath}'.");
    }
}