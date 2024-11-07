using Markdig;
using System.Text.RegularExpressions;

namespace GenerateHelp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: GenerateHelp <markdownFilePath> <htmlOutputPath>");
                return;
            }

            string markdownFilePath = args[0];
            string htmlOutputPath = args[1];

            string markdownContent = File.ReadAllText(markdownFilePath);

            // Configure the Markdown pipeline with advanced extensions
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            // Convert Markdown to HTML
            string bodyContent = Markdown.ToHtml(markdownContent, pipeline);

            // Embed images as Base64 in the HTML content
            bodyContent = EmbedImagesAsBase64(bodyContent, Path.GetDirectoryName(markdownFilePath) ?? ".");

            // Generate final HTML with GitHub Markdown CSS and Bootstrap for responsive layout
            string htmlContent = GenerateResponsiveHtmlWithStyles(bodyContent);

            // Save the generated HTML to the output path
            File.WriteAllText(htmlOutputPath, htmlContent);

            Console.WriteLine($"Markdown converted to styled and responsive HTML with embedded images at: {htmlOutputPath}");
        }

        // Embed images as Base64 for <img src="..."> tags in the HTML content
        static string EmbedImagesAsBase64(string htmlContent, string markdownDir)
        {
            string imgTagPattern = @"<img\s+[^>]*src=""([^""]+)""";
            Regex regex = new Regex(imgTagPattern, RegexOptions.IgnoreCase);

            var matches = regex.Matches(htmlContent);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string imagePath = match.Groups[1].Value;

                    // Handle relative and absolute paths
                    string fullImagePath = Path.IsPathRooted(imagePath) ? imagePath : Path.Combine(markdownDir, imagePath);

                    if (File.Exists(fullImagePath))
                    {
                        string base64Image = ConvertImageToBase64(fullImagePath);
                        string mimeType = GetImageMimeType(fullImagePath);

                        string base64ImageTag = $"data:image/{mimeType};base64,{base64Image}";

                        htmlContent = htmlContent.Replace(imagePath, base64ImageTag);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Image not found: {fullImagePath}");
                    }
                }
            }

            return htmlContent;
        }

        // Generate the final HTML with GitHub-like CSS and Bootstrap
        static string GenerateResponsiveHtmlWithStyles(string bodyContent)
        {
            // GitHub Markdown CSS hosted on CDN
            string githubCssLink = "<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.1.0/github-markdown.min.css\">";

            // Bootstrap CSS for responsive layout
            string bootstrapCssLink = "<link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css\" integrity=\"sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm\" crossorigin=\"anonymous\">";

            return $@"
                <!DOCTYPE html>
                <html lang=""en"">
                <head>
                    <meta charset=""UTF-8"">
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <title>Generated Help</title>
                    {githubCssLink}
                    {bootstrapCssLink}
                    <style>
                        /* Additional responsive design: max-width for better readability */
                        .markdown-body {{ max-width: 800px; margin: auto; padding: 2rem; }}
                    </style>
                </head>
                <body class=""markdown-body container"">
                    {bodyContent}
                </body>
                </html>";
        }

        // Convert an image file to a Base64 string
        static string ConvertImageToBase64(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }

        // Get the MIME type based on the file extension
        static string GetImageMimeType(string imagePath)
        {
            string extension = Path.GetExtension(imagePath).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "jpeg",
                ".png" => "png",
                ".gif" => "gif",
                ".bmp" => "bmp",
                ".svg" => "svg+xml",
                _ => "octet-stream"
            };
        }
    }
}
