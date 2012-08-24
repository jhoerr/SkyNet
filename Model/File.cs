namespace SkyNet.Model
{
    public class File : Resource
    {
        /// <summary>
        ///   The size, in bytes, of the file.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        ///   The number of comments that are associated with the file.
        /// </summary>
        public long Comments_Count { get; set; }

        /// <summary>
        ///   A value that indicates whether comments are enabled for the file. If comments can be made, this value is true; otherwise, it is false.
        /// </summary>
        public bool Comments_Enabled { get; set; }

        /// <summary>
        ///   The URL to use to download the file from SkyDrive. This value is not persistent. Use it immediately after making the request, and avoid caching.  This structure is not available if the file is a Office OneNote notebook.
        /// </summary>
        public string Source { get; set; }
    }
}